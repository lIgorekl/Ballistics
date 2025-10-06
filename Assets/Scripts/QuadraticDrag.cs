using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class QuadraticDrag : MonoBehaviour
{
    private Rigidbody _rb;

    private float _mass = 1f;
    private float _radius = 0.1f;
    private float _dragCoefficient = 0.47f;
    private float _airDensity = 1.225f;
    private Vector3 _wind = Vector3.zero;
    private float _area = 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        UpdateArea();
    }

    private void UpdateArea()
    {
        _area = Mathf.PI * _radius * _radius;
    }

    private void FixedUpdate()
    {
        Vector3 vRel = _rb.linearVelocity - _wind;
        float speed = vRel.magnitude;
        if (speed < 1e-6f) return;
        Vector3 drag = -0.5f * _airDensity * _dragCoefficient * _area * speed * vRel;
        _rb.AddForce(drag, ForceMode.Force);
    }

    public void SetPhysicalParams(float mass, float radius, float Cd, float rho, Vector3 wind, Vector3 initialVelocity)
    {
        _mass = Mathf.Max(0.0001f, mass);
        _radius = Mathf.Max(0.0001f, radius);
        _dragCoefficient = Mathf.Max(0f, Cd);
        _airDensity = Mathf.Max(0f, rho);
        _wind = wind;
        UpdateArea();

        if (_rb != null)
        {
            _rb.mass = _mass;
            _rb.useGravity = true;
            _rb.linearVelocity = initialVelocity;
            _rb.angularVelocity = Vector3.zero;
            _rb.linearDamping = 0f;
        }

        // масштабируем визуал
        transform.localScale = Vector3.one * (_radius * 2f);
    }

    // для внешнего доступа (если нужно)
    public float Radius => _radius;
    public float Mass => _mass;
}
