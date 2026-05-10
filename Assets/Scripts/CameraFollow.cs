using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float offsetX = 3f;
    public float offsetY = 2f;
    public float smoothSpeed = 5f;
    public float minX = 0f; // カメラの左端制限

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = Mathf.Max(target.position.x + offsetX, minX);
        Vector3 desired = new Vector3(targetX, offsetY, -10f);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
