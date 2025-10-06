using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryRenderer : MonoBehaviour
{
    [Header("Отрисовка")]
    [SerializeField] private int _pointsCount = 60;     // сколько точек рисуем
    [SerializeField] private float _timeStep = 0.02f;   // шаг по времени (сек)
    [SerializeField] private float _widthLine = 0.02f;  // толщина линии

    [Header("Физика воздуха (по умолчанию)")]
    [SerializeField] private float _mass = 1f;             // кг
    [SerializeField] private float _radius = 0.1f;         // м
    [SerializeField] private float _dragCoefficient = 0.47f; // Cd для сферы
    [SerializeField] private float _airDensity = 1.225f;   // rho (кг/м^3)
    [SerializeField] private Vector3 _wind = Vector3.zero; // ветер (м/с)

    [Header("Стрельба")]
    [SerializeField] private GameObject _projectilePrefab;

    private LineRenderer _line;
    private float _area;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.useWorldSpace = true;
        _line.startWidth = _widthLine;
        _line.endWidth = _widthLine;
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.positionCount = 0;

        _area = Mathf.PI * _radius * _radius;
    }

    // -----------------
    // Публичные свойства / доступ
    // -----------------
    public float DragCoefficient => _dragCoefficient;
    public float AirDensity => _airDensity;
    public Vector3 Wind => _wind;
    public float Mass => _mass;
    public float Radius => _radius;

    // Публичный метод для установки параметров воздуха/сопротивления (используется извне)
    public void SetAirPhysicsParams(float mass, float radius, float Cd, float rho, Vector3 wind)
    {
        _mass = Mathf.Max(0.0001f, mass);
        _radius = Mathf.Max(0.0001f, radius);
        _dragCoefficient = Mathf.Max(0f, Cd);
        _airDensity = Mathf.Max(0f, rho);
        _wind = wind;
        _area = Mathf.PI * _radius * _radius;
    }

    // Альтернативный простой сеттер, если нужно отдельно задать m и r для превью
    public void SetMassRadiusForPreview(float mass, float radius)
    {
        _mass = Mathf.Max(0.0001f, mass);
        _radius = Mathf.Max(0.0001f, radius);
        _area = Mathf.PI * _radius * _radius;
    }

    // -----------------
    // Вакуум (аналитика)
    // -----------------
    public void DrawVacuum(Vector3 startPosition, Vector3 startVelocity)
    {
        if (_pointsCount < 2) _pointsCount = 2;

        _line.positionCount = _pointsCount;

        for (int i = 0; i < _pointsCount; i++)
        {
            float t = i * _timeStep;
            Vector3 p = startPosition + startVelocity * t + 0.5f * Physics.gravity * (t * t);
            _line.SetPosition(i, p);
        }
    }

    // ---------------------------------------
    // Численная интеграция методом Эйлера с квадратичным сопротивлением
    // Fd = -0.5 * rho * Cd * A * |v_rel| * v_rel
    // ---------------------------------------
    public void DrawWithAirEuler(Vector3 startPosition, Vector3 startVelocity)
    {
        if (_pointsCount < 2) _pointsCount = 2;

        Vector3 p = startPosition;
        Vector3 v = startVelocity;

        _line.positionCount = _pointsCount;

        // пересчитать площадь на случай, если radius изменился извне
        _area = Mathf.PI * _radius * _radius;

        for (int i = 0; i < _pointsCount; i++)
        {
            _line.SetPosition(i, p);

            Vector3 vRel = v - _wind;
            float speed = vRel.magnitude;

            Vector3 drag = Vector3.zero;
            if (speed > 1e-6f)
            {
                drag = -0.5f * _airDensity * _dragCoefficient * _area * speed * vRel;
            }

            Vector3 a = Physics.gravity + drag / Mathf.Max(0.0001f, _mass);

            // Эйлеровский шаг: сначала скорость, затем позиция (как в методичке)
            v += a * _timeStep;
            p += v * _timeStep;
        }
    }

    // -----------------
    // Fire: создает префаб и передает параметры QuadraticDrag (если компонент есть)
    // -----------------
    public void Fire(Vector3 position, Vector3 initialVelocity)
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning("TrajectoryRenderer.Fire: projectile prefab is not set.");
            return;
        }

        GameObject newCore = Instantiate(_projectilePrefab, position, Quaternion.identity);

        // Принудительно выставим scale префаба по текущему _radius (чтобы визуал и расчёт совпадали)
        newCore.transform.localScale = Vector3.one * (_radius * 2f);

        // Лог для отладки
        Debug.Log($"Fire: mass={_mass:F3}, radius={_radius:F3}, Cd={_dragCoefficient:F3}, rho={_airDensity:F3}, wind={_wind}, initVel={initialVelocity}");

        QuadraticDrag qd = newCore.GetComponent<QuadraticDrag>();
        if (qd != null)
        {
            qd.SetPhysicalParams(_mass, _radius, _dragCoefficient, _airDensity, _wind, initialVelocity);
        }
        else
        {
            // Если компонента QuadraticDrag нет — попытаемся настроить Rigidbody вручную
            Rigidbody rb = newCore.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = _mass;
                rb.linearVelocity = initialVelocity;
                rb.useGravity = true;
                rb.linearDamping = 0f;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
