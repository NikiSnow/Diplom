using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WaterPhysics2D : MonoBehaviour
{
    [SerializeField] private PlayerController PController;

    [Header("Параметры движения")]
    [SerializeField] private bool useGlobalSettings = true;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float drag = 4f;

    [Header("Dash Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float dashSpeed = 6f;
    [SerializeField] private float DashCD = 1f;
    [SerializeField] private float DashLenght = 5f; // пока не используется, оставил чтобы не потерять поле в инспекторе
    [SerializeField] private float controlReturnDelay = 0.5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 controlledVelocity;
    private Vector2 externalVelocity;
    private Vector2 direction = Vector2.right;

    private Coroutine dashCooldownCoroutine;
    private Coroutine dashControlCoroutine;

    private bool isDashing;
    private bool canDash = true;
    private bool canControl = true;

    public float BaseMoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    public bool IsHolding { get; set; }
    public bool IsDashing => isDashing;
    public bool CanDash => canDash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (PController == null)
            PController = GetComponent<PlayerController>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;

        if (useGlobalSettings && GlobalWater.Instance != null && GlobalWater.Instance.settings != null)
        {
            WaterSettings s = GlobalWater.Instance.settings;
            moveSpeed = s.baseMoveSpeed;
            acceleration = s.baseAcceleration;
            drag = s.baseDrag;
        }
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        Vector2 targetVelocity = moveInput * moveSpeed;
        controlledVelocity = Vector2.Lerp(controlledVelocity, targetVelocity, acceleration * dt);

        if (moveInput.sqrMagnitude < 0.0001f)
            controlledVelocity = Vector2.Lerp(controlledVelocity, Vector2.zero, drag * dt);

        if (externalVelocity.sqrMagnitude > 0.0001f)
            externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, drag * dt);
        else
            externalVelocity = Vector2.zero;

        rb.linearVelocity = controlledVelocity + externalVelocity;
    }

    public void SetRotation(Vector2 dir)
    {
        if (dir.sqrMagnitude <= 0.0001f)
            return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

        if (IsHolding)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (isDashing)
        {
            if (rb.linearVelocity.sqrMagnitude >= 5f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    (rotationSpeed * 2f) * Time.deltaTime
                );
            }

            return;
        }

        if (!canControl)
            return;

        if (Quaternion.Angle(transform.rotation, targetRotation) > 1.5f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    public void SetMoveInput(Vector2 dir)
    {
        if (IsHolding || !canControl || isDashing)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        moveInput = dir;
    }

    public void AddImpulse(Vector2 impulse)
    {
        externalVelocity += impulse;
    }

    public void ApplyWaterSettings(WaterSettings newSettings)
    {
        moveSpeed = newSettings.baseMoveSpeed;
        acceleration = newSettings.baseAcceleration;
        drag = newSettings.baseDrag;
    }

    public void RotateTowardsMouse()
    {
        if (!canDash || !canControl || isDashing || mainCamera == null)
            return;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );

        if (aimDirection.sqrMagnitude <= 0.0001f)
            return;

        direction = aimDirection.normalized;

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

    public void Release()
    {
        if (!canDash || isDashing || !canControl || direction.sqrMagnitude <= 0.0001f)
            return;

        Vector2 dashDirection = direction.normalized;

        if (PController != null)
            PController.lastMoveDir = dashDirection;

        moveInput = Vector2.zero;
        canDash = false;
        canControl = false;
        isDashing = true;

        AddImpulse(dashDirection * dashSpeed);

        if (dashCooldownCoroutine != null)
            StopCoroutine(dashCooldownCoroutine);

        if (dashControlCoroutine != null)
            StopCoroutine(dashControlCoroutine);

        dashCooldownCoroutine = StartCoroutine(DashCooldownRoutine());
        dashControlCoroutine = StartCoroutine(DashControlReturnRoutine());
    }

    private IEnumerator DashCooldownRoutine()
    {
        while (isDashing)
            yield return null;

        yield return new WaitForSecondsRealtime(DashCD);

        canDash = true;
        dashCooldownCoroutine = null;
    }

    private IEnumerator DashControlReturnRoutine()
    {
        yield return new WaitForSecondsRealtime(controlReturnDelay);

        StopRb();
        isDashing = false;
        canControl = true;
        dashControlCoroutine = null;
    }

    public void StopRb()
    {
        moveInput = Vector2.zero;
        controlledVelocity = Vector2.zero;
        externalVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }
}