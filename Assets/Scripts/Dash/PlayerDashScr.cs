using UnityEngine;

public class PlayerDashScr : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] Camera mainCamera;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float dashSpeed = 6f;
    [SerializeField] float DashCD = 1f;
    [SerializeField] float DashLenght = 5f;
    bool IsHolding = false;
    bool IsDasing = true;
    Vector2 direction;
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            RotateTowardsMouse();
        }
        else if (IsHolding)
        {
            IsHolding = false;
            Time.timeScale = 1f;
            Release();
        }
    }

    private void RotateTowardsMouse()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        direction = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
        IsHolding = true;
        Time.timeScale = 0.4f;
    }
    private void Release()
    {
        rb.linearVelocity = Vector3.zero;
        rb.linearVelocity = direction * dashSpeed;
        IsDasing = true;
    }
    public void StopRb()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x/100, rb.linearVelocity.y / 100,0);
    }
}
