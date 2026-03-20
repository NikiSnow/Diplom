using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WaterPhysics2D : MonoBehaviour
{
    [SerializeField] PlayerController PController;
    [Header("Параметры движения")]
    [SerializeField] bool useGlobalSettings = true; // Брать базовые значения из GlobalWater
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float acceleration = 12f;
    [SerializeField] float drag = 4f;

    // Публичное свойство/метод для доступа к скорости
    public float BaseMoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    Rigidbody2D rb;
    Vector2 moveInput;                    // Вектор направления от контроллера
    Vector2 controlledVelocity;           // Скорость от "обычного" движения
    Vector2 externalVelocity;             // Доп. скорость (рывки, отдача и т.п.)

    [Header("DashSettings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] Camera mainCamera;
    [SerializeField] float dashSpeed = 6f;
    [SerializeField] float DashCD = 1f;
    [SerializeField] float DashLenght = 5f;
    public bool IsHolding = false;
    bool IsDasing = false;
    Vector2 direction;

    void Awake()
    {
        // Кешируем rigidbody, отключаем гравитацию и демпфирование — сами контролируем
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f; // вместо rb.drag

        // Подтягиваем глобальные настройки воды
        if (useGlobalSettings && GlobalWater.Instance != null && GlobalWater.Instance.settings != null)
        {
            var s = GlobalWater.Instance.settings;
            moveSpeed = s.baseMoveSpeed;
            acceleration = s.baseAcceleration;
            drag = s.baseDrag;

            // TODO: здесь можно подвязать глобальный цвет воды / эффекты по желанию
        }
    }
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Целевая скорость от ввода
        Vector2 targetVelocity = moveInput * moveSpeed;

        // Плавный выход на целевую скорость (эффект воды)
        controlledVelocity = Vector2.Lerp(controlledVelocity, targetVelocity, acceleration * dt);

        // Если ввода нет — дотормаживаем
        if (moveInput.sqrMagnitude < 0.0001f)
        {
            controlledVelocity = Vector2.Lerp(controlledVelocity, Vector2.zero, drag * dt);
        }

        // Затухание внешней скорости (рывки, отдача и т.п.)
        if (externalVelocity.sqrMagnitude > 0.0001f)
        {
            externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, drag * dt);
        }

        // Итоговая скорость = основное движение + импульсы
        rb.linearVelocity = controlledVelocity + externalVelocity;

        // TODO: сюда можно повесить VFX пузырьков при достаточной скорости
        // if (rb.linearVelocity.sqrMagnitude > someThreshold * someThreshold) { ... }
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
                (rotationSpeed*2) * Time.deltaTime);
            }
            else
            {
                return;
            }
        }

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

    // Вызывается контроллером (игрок/моб) в Update
    public void SetMoveInput(Vector2 dir)
    {
        if (IsHolding)
        {
            moveInput = Vector2.zero;
            return;
        }
        if (IsDasing)
        {
            if (rb.linearVelocity.sqrMagnitude <= 0.0001f) IsDasing = false;
            return;
        }

        // Нормализуем, чтобы по диагонали не было бонусной скорости
        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        moveInput = dir;

        // TODO: триггер смены анимаций плавания (idle/swim/fast)
        // по dir.magnitude и направлению
    }

    // Хук под рывки, отдачу, удары и т.п.
    public void AddImpulse(Vector2 impulse)
    {
        externalVelocity += impulse;

        // TODO: сюда подвязать:
        // - звук рывка
        // - всплеск/пузырьки
        // - Cinemachine Impulse для тряски камеры
    }

    // Хук под смену зоны воды (другая "плотность", глубина и т.п.)
    public void ApplyWaterSettings(WaterSettings newSettings)
    {
        moveSpeed = newSettings.baseMoveSpeed;
        acceleration = newSettings.baseAcceleration;
        drag = newSettings.baseDrag;

        // TODO: здесь можно менять постэффекты по глубине/зоне
    }

    public void RotateTowardsMouse()
    {
        if (IsDasing) return;
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
        //rb.linearVelocity = Vector3.zero;
        //rb.linearVelocity = direction * dashSpeed;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        //if (Quaternion.Angle(transform.rotation, targetRotation) > 25f)
        //    transform.rotation = Quaternion.Slerp(
        //        transform.rotation,
        //        targetRotation,
        //        50 * Time.deltaTime);
        PController.lastMoveDir = direction.normalized;
        IsDasing = true;
        AddImpulse(direction * dashSpeed);
    }
    public void StopRb()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x / 100, rb.linearVelocity.y / 100, 0);
    }
}
