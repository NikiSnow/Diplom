using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WaterPhysics2D : MonoBehaviour
{
    [SerializeField] private PlayerController PController;

    [Header("Параметры движения")]
    [SerializeField] private bool useGlobalSettings = true; // Брать базовые значения из GlobalWater
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float drag = 4f;

    public float BaseMoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    private Rigidbody2D rb;
    private Vector2 moveInput;           // Вектор направления от контроллера
    private Vector2 controlledVelocity;  // Скорость от "обычного" движения
    private Vector2 externalVelocity;    // Доп. скорость (рывки, отдача и т.п.)

    [Header("DashSettings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float dashSpeed = 6f;
    [SerializeField] private float DashCD = 1f;
    [SerializeField] private float DashLenght = 5f;
    [SerializeField] private float controlReturnDelay = 0.5f;

    public bool IsHolding = false;

    private bool IsDasing = false;
    private bool canDash = true;
    private bool canControl = true;

    private Vector2 direction;

    private Coroutine dashCooldownCoroutine;
    private Coroutine dashControlCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (useGlobalSettings && GlobalWater.Instance != null && GlobalWater.Instance.settings != null)
        {
            var s = GlobalWater.Instance.settings;

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
        {
            controlledVelocity = Vector2.Lerp(controlledVelocity, Vector2.zero, drag * dt);
        }

        if (externalVelocity.sqrMagnitude > 0.0001f)
        {
            externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, drag * dt);
        }

        // Завершение рывка теперь определяется здесь,
        // чтобы оно не зависело от того, вызывается ли SetMoveInput.
        if (IsDasing && externalVelocity.sqrMagnitude <= 0.0001f)
        {
            externalVelocity = Vector2.zero;
            IsDasing = false;
        }

        rb.linearVelocity = controlledVelocity + externalVelocity;
    }

    public void SetRotation(Vector2 dir)
    {
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

        if (IsHolding)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (IsDasing)
        {
            if (rb.linearVelocity.sqrMagnitude >= 5f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    (rotationSpeed * 2f) * Time.deltaTime);
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
                rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    // Вызывается контроллером (игрок/моб) в Update
    public void SetMoveInput(Vector2 dir)
    {
        if (IsHolding || !canControl)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (IsDasing)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        moveInput = dir;
    }

    // Хук под рывки, отдачу, удары и т.п.
    public void AddImpulse(Vector2 impulse)
    {
        externalVelocity += impulse;
    }

    // Хук под смену зоны воды (другая "плотность", глубина и т.п.)
    public void ApplyWaterSettings(WaterSettings newSettings)
    {
        moveSpeed = newSettings.baseMoveSpeed;
        acceleration = newSettings.baseAcceleration;
        drag = newSettings.baseDrag;
    }

    public void RotateTowardsMouse()
    {
        if (!canDash || !canControl || IsDasing)
            return;

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

    public void Release()
    {
        if (!canDash || IsDasing || !canControl || direction.sqrMagnitude <= 0.0001f)
            return;

        if (PController != null)
            PController.lastMoveDir = direction.normalized;

        moveInput = Vector2.zero;
        canDash = false;
        canControl = false;
        IsDasing = true;

        AddImpulse(direction * dashSpeed);

        if (dashCooldownCoroutine != null)
            StopCoroutine(dashCooldownCoroutine);

        if (dashControlCoroutine != null)
            StopCoroutine(dashControlCoroutine);

        dashCooldownCoroutine = StartCoroutine(DashCooldownRoutine());
        dashControlCoroutine = StartCoroutine(DashControlReturnRoutine());
    }

    private IEnumerator DashCooldownRoutine()
    {
        yield return new WaitUntil(() => !IsDasing);
        yield return new WaitForSeconds(DashCD);

        canDash = true;
        dashCooldownCoroutine = null;
    }

    private IEnumerator DashControlReturnRoutine()
    {
        //yield return new WaitUntil(() => !IsDasing);
        yield return new WaitForSeconds(controlReturnDelay);
        StopRb();
        IsDasing = false;
        canControl = true;
        dashControlCoroutine = null;
    }

    public void StopRb()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x / 100f, rb.linearVelocity.y / 100f, 0f);
    }
}