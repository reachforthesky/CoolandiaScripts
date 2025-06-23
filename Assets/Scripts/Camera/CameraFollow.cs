using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    public Transform target;      // Player
    public Transform pivot;       // Camera pivot (rotates horizontally)
    public float followSpeed = 10f;
    public float rotationSpeed = 100f;
    public bool allowRotation = true;

    public float loadingScreenThreshold = 20f;
    public float snapThreshold = 40f;

    private Vector3 fixedPivotOffset;
    private bool isFarFromTarget = false;

    void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        if (pivot != null && target != null)
        {
            fixedPivotOffset = pivot.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (!IsOwner)
            return;

        if (target == null || pivot == null) return;

        // Calculate desired pivot position
        Vector3 desiredPos = target.position + new Vector3(0, fixedPivotOffset.y, 0);
        float distance = Vector3.Distance(pivot.position, target.position);

        // Snap if too far, otherwise follow smoothly
        if (distance > snapThreshold)
        {
            pivot.position = desiredPos;
        }
        else
        {
            pivot.position = Vector3.Lerp(pivot.position, desiredPos, followSpeed * Time.deltaTime);
        }

        // Rotate pivot
        if (allowRotation && Input.GetMouseButton(1))
        {
            float horizontal = Input.GetAxis("Mouse X");
            pivot.RotateAround(target.position, Vector3.up, horizontal * rotationSpeed * Time.deltaTime);
        }

        // Show/hide loading screen
        if (distance > loadingScreenThreshold && !isFarFromTarget)
        {
            UIManager.LocalInstance.ShowLoadingScreen();
            isFarFromTarget = true;
        }
        else if (distance <= loadingScreenThreshold && isFarFromTarget)
        {
            UIManager.LocalInstance.HideLoadingScreen();
            isFarFromTarget = false;
        }
    }
}
