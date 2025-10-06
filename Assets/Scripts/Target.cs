using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Target : MonoBehaviour
{
    private Rigidbody _rb;
    private float _radius = 0.2f;
    private float _mass = 1f;
    private bool _isDead = false;

    // поведение удержания в зоне
    private Vector3 _spawnCenter = Vector3.zero;
    private float _maxDistance = 15f;
    private float _maxSpeed = 3f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void SetParams(float mass, float radius, Vector3 horizontalVelocity, Vector3 spawnCenter, float maxDistance, float maxSpeed)
    {
        _mass = Mathf.Max(0.0001f, mass);
        _radius = Mathf.Max(0.001f, radius);

        _rb.mass = _mass;
        _rb.useGravity = false; // летают по горизонту
        _rb.linearDamping = 0f;

        transform.localScale = Vector3.one * (_radius * 2f);

        // задаём начальную горизонтальную скорость (Y = 0)
        Vector3 v = horizontalVelocity;
        v.y = 0f;
        if (v.magnitude > maxSpeed) v = v.normalized * maxSpeed;
        _rb.linearVelocity = v;

        _spawnCenter = spawnCenter;
        _maxDistance = Mathf.Max(5f, maxDistance);
        _maxSpeed = Mathf.Max(0.5f, maxSpeed);
    }

    private void Update()
    {
        if (_isDead) return;

        // Ограничиваем скорость
        Vector3 vel = _rb.linearVelocity;
        vel.y = 0f;
        if (vel.magnitude > _maxSpeed)
        {
            vel = vel.normalized * _maxSpeed;
            _rb.linearVelocity = new Vector3(vel.x, _rb.linearVelocity.y, vel.z);
        }

        // Если улетели за пределы зоны — отражаем горизонтальную составляющую
        float dist = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z),
                                      new Vector3(_spawnCenter.x, 0f, _spawnCenter.z));
        if (dist > _maxDistance)
        {
            Vector3 dirToCenter = (_spawnCenter - transform.position);
            dirToCenter.y = 0f;
            dirToCenter.Normalize();

            // задаём скорость обратно к центру с небольшой случайной составляющей
            Vector3 newVel = (dirToCenter + new Vector3(Random.Range(-0.3f,0.3f),0f,Random.Range(-0.3f,0.3f))).normalized * (_maxSpeed * 0.8f);
            _rb.linearVelocity = newVel;
        }

        // небольшой плавный поворот/манёвры (по желанию)
        // _rb.velocity += new Vector3(Mathf.Sin(Time.time * 0.5f) * 0.01f, 0f, Mathf.Cos(Time.time * 0.5f)*0.01f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isDead) return;

        if (collision.collider.GetComponent<QuadraticDrag>() != null)
        {
            _isDead = true;
            gameObject.SetActive(false);
            HitCounterUI.Instance?.RegisterHit();
        }
    }
}
