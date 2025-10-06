using UnityEngine;

public class CannonTest : MonoBehaviour
{
    [SerializeField] private TrajectoryRenderer _trajectory;
    [SerializeField] private Transform _launchPoint;
    [SerializeField] private float _speed = 15f;

    // Параметры воздуха — можно менять в инспекторе CannonRoot (TrajectoryRenderer), но дублируем для удобства
    [SerializeField] private float _mass = 1f;
    [SerializeField] private float _radius = 0.1f;
    [SerializeField] private float _Cd = 0.47f;
    [SerializeField] private float _rho = 1.225f;
    [SerializeField] private Vector3 _wind = Vector3.zero;

    private void Update()
    {
        if (_trajectory == null || _launchPoint == null) return;

        // синхронизируем параметры воздуха (перед рисованием)
        _trajectory.SetAirPhysicsParams(_mass, _radius, _Cd, _rho, _wind);

        // рассчитываем начальную скорость
        Vector3 v0 = _launchPoint.forward * _speed;

        // Рисуем траекторию с воздухом (Эйлер)
        _trajectory.DrawWithAirEuler(_launchPoint.position, v0);

        // Отдельно: стрельба — по пробелу
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _trajectory.Fire(_launchPoint.position, v0);
        }
    }
}
