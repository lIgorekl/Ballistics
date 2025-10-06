using UnityEngine;

[AddComponentMenu("Camera/Camera Follow (Preserve Height, Look Along Barrel)")]
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Точка, за которой следует камера (обычно CameraAnchor на CannonRoot)")]
    public Transform target;

    [Tooltip("Куда смотреть — обычно LaunchPoint (точка выстрела)")]
    public Transform aimTarget;

    [Tooltip("Сколько вперед смотреть от aimTarget вдоль его forward (м)")]
    public float lookAheadDistance = 4f;

    [Header("Параметры сглаживания")]
    [Tooltip("Время сглаживания позиции (меньше = более резко)")]
    public float positionSmoothTime = 0.12f;

    [Tooltip("Скорость сглаживания поворота (0..1)")]
    [Range(0f, 1f)]
    public float rotationSmoothFactor = 0.12f;

    private Vector3 _vel = Vector3.zero;
    private float _fixedHeight;

    private void Start()
    {
        // запомним текущую высоту камеры — будем её сохранять
        _fixedHeight = transform.position.y;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 1) Позиция: плавно двигаемся к target, но сохраняем Y (высоту) камеры неизменной
        Vector3 desiredPos = target.position;
        desiredPos.y = _fixedHeight; // фиксируем высоту — не поднимаем камеру

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _vel, positionSmoothTime);

        // 2) Поворот: смотрим вдоль дула — на точку немного вперед от aimTarget
        if (aimTarget != null)
        {
            Vector3 aimPoint = aimTarget.position + aimTarget.forward * lookAheadDistance;
            Vector3 dir = (aimPoint - transform.position);
            if (dir.sqrMagnitude > 1e-6f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothFactor);
            }
        }
    }
}
