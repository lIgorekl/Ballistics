using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _targetPrefab;
    [SerializeField] private int _spawnCount = 6;
    [SerializeField] private Vector3 _spawnAreaCenter = new Vector3(10f, 2f, 10f);
    [SerializeField] private Vector3 _spawnAreaSize = new Vector3(20f, 6f, 20f);

    [Header("Target random params")]
    [SerializeField] private float _tMassMin = 0.2f;
    [SerializeField] private float _tMassMax = 2.5f;
    [SerializeField] private float _tRadiusMin = 0.08f;
    [SerializeField] private float _tRadiusMax = 0.4f;
    [SerializeField] private float _tHorizSpeedMin = 0.5f;
    [SerializeField] private float _tHorizSpeedMax = 3f;

    [Header("Behaviour limits")]
    [SerializeField] private float _maxDistanceFromCenter = 12f;
    [SerializeField] private float _maxTargetSpeed = 3f;

    private void Start()
    {
        for (int i = 0; i < _spawnCount; i++)
            SpawnOne();
    }

    private void SpawnOne()
    {
        Vector3 localPos = new Vector3(
            Random.Range(-_spawnAreaSize.x * 0.5f, _spawnAreaSize.x * 0.5f),
            Random.Range(0.5f, _spawnAreaSize.y),
            Random.Range(-_spawnAreaSize.z * 0.5f, _spawnAreaSize.z * 0.5f)
        );
        Vector3 worldPos = _spawnAreaCenter + localPos;

        GameObject go = Instantiate(_targetPrefab, worldPos, Quaternion.identity);
        float mass = Random.Range(_tMassMin, _tMassMax);
        float radius = Random.Range(_tRadiusMin, _tRadiusMax);

        // случайная начальная горизонтальная скорость направление + magnitude
        Vector2 dir = Random.insideUnitCircle.normalized;
        float speed = Random.Range(_tHorizSpeedMin, _tHorizSpeedMax);
        Vector3 horizVel = new Vector3(dir.x, 0f, dir.y) * speed;

        Target t = go.GetComponent<Target>();
        if (t != null)
        {
            t.SetParams(mass, radius, horizVel, _spawnAreaCenter, _maxDistanceFromCenter, _maxTargetSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(_spawnAreaCenter, _spawnAreaSize);
    }
}
