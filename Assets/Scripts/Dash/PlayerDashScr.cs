using UnityEngine;

public class PlayerDashScr : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float dashSpeed = 6f;
    [SerializeField] private float dashDuration = 0.25f; // длительность рывка

    [Header("Slow Motion")]
    [SerializeField] private float aimTimeScale = 0.15f; // замедление мира при зажатой ЛКМ

    bool isAiming = false;    // прицеливание (ЛКМ зажата)
    bool isDashing = false;   // сейчас идёт рывок
    float dashTimer = 0f;     // таймер рывка
    Vector2 dashDirection;    // запомненное направление рывка

    public bool IsDashing => isDashing; // нужно рыбам

    void Update()
    {
        HandleInput();
        UpdateDash();
    }

    void HandleInput()
    {
        // НАЧАЛО прицеливания — ЛКМ нажали
        if (Input.GetMouseButtonDown(0) && !isDashing)
        {
            isAiming = true;
            Time.timeScale = aimTimeScale;
        }

        // УДЕРЖИВАЕМ ЛКМ — крутимся к мыши
        if (isAiming && Input.GetMouseButton(0))
        {
            RotateTowardsMouse();
        }

        // КОНЕЦ прицеливания — отпустили ЛКМ -> рывок
        if (isAiming && Input.GetMouseButtonUp(0))
        {
            isAiming = false;
            Time.timeScale = 1f;
            StartDash();
        }
    }

    void UpdateDash()
    {
        if (!isDashing)
            return;

        dashTimer += Time.unscaledDeltaTime; // не зависит от слоумо
        if (dashTimer >= dashDuration)
        {
            isDashing = false;
        }
    }

    void RotateTowardsMouse()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        dashDirection = new Vector2(
            mouseWorld.x - transform.position.x,
            mouseWorld.y - transform.position.y
        );

        if (dashDirection.sqrMagnitude < 0.0001f)
            return;

        float targetAngle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.AngleAxis(targetAngle, Vector3.forward);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.unscaledDeltaTime
        );
    }

    void StartDash()
    {
        if (dashDirection.sqrMagnitude < 0.0001f)
            return;

        dashDirection.Normalize();

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = dashDirection * dashSpeed;

        isDashing = true;
        dashTimer = 0f;
    }

    public void StopRb()
    {
        rb.linearVelocity = rb.linearVelocity / 100f;
        isDashing = false;
    }
}
