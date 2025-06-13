using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // Player
    public Transform pivot;       // Camera pivot (rotates horizontally)
    public float followSpeed = 10f;
    public float rotationSpeed = 100f;
    public bool allowRotation = true;

    private Vector3 fixedPivotOffset;

    void Start()
    {
        if (pivot != null && target != null)
        {
            fixedPivotOffset = pivot.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null || pivot == null) return;

        // Lock pivot's height
        Vector3 targetPos = target.position;
        targetPos.y = target.position.y + fixedPivotOffset.y;
        pivot.position = Vector3.Lerp(pivot.position, targetPos, followSpeed * Time.deltaTime);

        // Rotate around Y-axis (horizontal rotation only)
        if (allowRotation && Input.GetMouseButton(1))
        {
            float horizontal = Input.GetAxis("Mouse X");
            pivot.RotateAround(target.position, Vector3.up, horizontal * rotationSpeed * Time.deltaTime);
        }
    }
}