using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WaterPhysics2D : MonoBehaviour
{
    [Header("Параметры движения")]
    public bool useGlobalSettings = true; // Брать базовые значения из GlobalWater
    public float moveSpeed = 6f;          // Целевая скорость при полном вводе
    public float acceleration = 12f;      // Разгон до целевой скорости
    public float drag = 4f;               // Торможение, когда нет ввода

    Rigidbody2D rb;
    Vector2 moveInput;                    // Вектор направления от контроллера
    Vector2 controlledVelocity;           // Скорость от "обычного" движения
    Vector2 externalVelocity;             // Доп. скорость (рывки, отдача и т.п.)

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
        rb.linearVelocity = controlledVelocity + externalVelocity; // вместо rb.velocity

        // TODO: сюда можно повесить VFX пузырьков при достаточной скорости
        // if (rb.linearVelocity.sqrMagnitude > someThreshold * someThreshold) { ... }
    }

    // Вызывается контроллером (игрок/моб) в Update
    public void SetMoveInput(Vector2 dir)
    {
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
}
