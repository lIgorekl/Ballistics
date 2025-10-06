using UnityEngine;

[RequireComponent(typeof(TrajectoryRenderer))]
public class CannonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _muzzle; // LaunchPoint / Muzzle (точка выстрела)
    [SerializeField] private Transform _rootForMove; // обычно CannonRoot (тот, на котором висит этот скрипт)
    [SerializeField] private TrajectoryRenderer _trajectoryRenderer;

    [Header("Camera anchor (optional)")]
    [Tooltip("Anchor дочерний от CannonRoot, который задаёт позицию камеры (например CameraAnchor). Если не задан — камера не будет подниматься.")]
    [SerializeField] private Transform _cameraAnchor;
    private Vector3 _cameraAnchorOriginalLocalPos;

    [Header("Move / Rotate")]
    [SerializeField] private float _moveSpeed = 5f; // скорость перемещения по XZ
    [SerializeField] private float _rotateSpeed = 90f; // yaw градусы/сек (Q/E)
    [SerializeField] private float _pitchSpeed = 45f; // градусы/сек для наклона дула (стрелки / R F)
    [SerializeField] private float _minPitch = -5f;
    [SerializeField] private float _maxPitch = 80f;

    [Header("Camera lift")]
    [Tooltip("Максимальное поднятие камеры по Y относительно исходного локального положения, когда дула смотрит в максимальный угол.")]
    [SerializeField] private float _cameraMaxLift = 1.2f;

    [Header("Shot")]
    [SerializeField] private float _shotSpeed = 15f;

    [Header("Random projectile params")]
    [SerializeField] private float _mMin = 0.5f;
    [SerializeField] private float _mMax = 3f;
    [SerializeField] private float _rMin = 0.05f;
    [SerializeField] private float _rMax = 0.25f;

    private void Reset()
    {
        if (_trajectoryRenderer == null) _trajectoryRenderer = GetComponent<TrajectoryRenderer>();
        if (_rootForMove == null) _rootForMove = this.transform;
    }

    private void Start()
    {
        // защита: если root оказался под землёй (y < 0.1), поднимаем его чуть выше
        if (_rootForMove.position.y < 0.1f)
        {
            Vector3 pos = _rootForMove.position;
            pos.y = 1f;
            _rootForMove.position = pos;
        }

        // Если _muzzle отсутствует, пробуем найти child "LaunchPoint"
        if (_muzzle == null)
        {
            Transform t = transform.Find("LaunchPoint");
            if (t != null) _muzzle = t;
        }

        // если есть _cameraAnchor, запишем его исходную localPosition
        if (_cameraAnchor != null)
        {
            _cameraAnchorOriginalLocalPos = _cameraAnchor.localPosition;
        }
    }

    private void Update()
    {
        HandleMovement();    // WASD / Q E
        HandlePitchInput();  // стрелки или R/F
        UpdatePreview();     // рисуем траекторию текущим направлением

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireRandomProjectile();
        }
    }

    private void HandleMovement()
    {
        // Перемещение: WASD — локальная ось XZ (перемещаем _rootForMove по земле)
        float hx = Input.GetKey(KeyCode.D) ? 1f : (Input.GetKey(KeyCode.A) ? -1f : 0f);
        float hz = Input.GetKey(KeyCode.W) ? 1f : (Input.GetKey(KeyCode.S) ? -1f : 0f);

        Vector3 move = new Vector3(hx, 0f, hz);
        if (move.sqrMagnitude > 0.001f)
        {
            move = move.normalized * _moveSpeed * Time.deltaTime;
            _rootForMove.Translate(move, Space.Self); // локально, чтобы W шёл в "вперёд" относительно поворота
        }

        // Поворот по Y (yaw): Q/E
        float rotDir = Input.GetKey(KeyCode.E) ? 1f : (Input.GetKey(KeyCode.Q) ? -1f : 0f);
        if (Mathf.Abs(rotDir) > 0.001f)
        {
            _rootForMove.Rotate(Vector3.up, rotDir * _rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    private void HandlePitchInput()
    {
        if (_muzzle == null) return;

        // два варианта ввода: стрелки вверх/вниз или R/F
        float p = 0f;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.R)) p = 1f;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.F)) p = -1f;

        if (Mathf.Abs(p) > 0.001f)
        {
            Vector3 e = _muzzle.localEulerAngles;
            // localEulerAngles.x в Unity в диапазоне 0..360, поэтому преобразуем в -180..180
            float curPitch = e.x;
            if (curPitch > 180f) curPitch -= 360f;
            curPitch += p * _pitchSpeed * Time.deltaTime;
            curPitch = Mathf.Clamp(curPitch, _minPitch, _maxPitch);
            e.x = curPitch;
            _muzzle.localEulerAngles = e;

            // теперь поднимем/опустим CameraAnchor пропорционально углу наклона
            if (_cameraAnchor != null)
            {
                // нормализуем текущий угол в [0..1] относительно диапазона [minPitch..maxPitch]
                float tNorm = Mathf.InverseLerp(_minPitch, _maxPitch, curPitch); // 0..1
                Vector3 lp = _cameraAnchorOriginalLocalPos;
                lp.y = _cameraAnchorOriginalLocalPos.y + tNorm * _cameraMaxLift;
                _cameraAnchor.localPosition = lp;
            }
        }
    }

    private void UpdatePreview()
    {
        if (_muzzle == null || _trajectoryRenderer == null) return;

        // стартовая скорость от направления дула
        Vector3 v0 = _muzzle.forward * _shotSpeed;

        // рисуем с учётом текущих параметров в TrajectoryRenderer
        _trajectoryRenderer.DrawWithAirEuler(_muzzle.position, v0);
    }

    private void FireRandomProjectile()
    {
        if (_muzzle == null || _trajectoryRenderer == null) return;

        float m = Random.Range(_mMin, _mMax);
        float r = Random.Range(_rMin, _rMax);

        // установим параметры в Preview, чтобы превью соответствовало этому выстрелу
        _trajectoryRenderer.SetAirPhysicsParams(m, r, _trajectoryRenderer.DragCoefficient,
            _trajectoryRenderer.AirDensity, _trajectoryRenderer.Wind);

        Vector3 v0 = _muzzle.forward * _shotSpeed;

        _trajectoryRenderer.Fire(_muzzle.position, v0);
    }
}
