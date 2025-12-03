using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CameraViewLimiter2D : MonoBehaviour
{
    public Camera targetCamera;
    public float margin = 0.5f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void FixedUpdate()
    {
        if (targetCamera == null) return;

        float vertExtent = targetCamera.orthographicSize;
        float horExtent = vertExtent * targetCamera.aspect;

        Vector2 camPos = targetCamera.transform.position;
        Vector2 pos = rb.position;

        float minX = camPos.x - horExtent + margin;
        float maxX = camPos.x + horExtent - margin;
        float minY = camPos.y - vertExtent + margin;
        float maxY = camPos.y + vertExtent - margin;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        rb.position = pos;
    }
}
