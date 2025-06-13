using UnityEngine;

public class BillboardOppositeCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f; // Keep upright
        transform.forward = -camForward; // Face away from camera
    }
}
