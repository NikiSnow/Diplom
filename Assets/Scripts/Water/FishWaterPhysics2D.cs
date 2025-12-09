using UnityEngine;

// ѕроста€ физика движени€ рыбы в воде
[RequireComponent(typeof(Rigidbody2D))]
public class FishWaterPhysics2D : MonoBehaviour
{
    [Header("ƒвижение в воде")]
    public float moveSpeed = 3f;      // базова€ скорость
    public float acceleration = 12f;  // как быстро набираем скорость
    public float drag = 4f;           // как быстро гасим скорость (сопротивление)

    Rigidbody2D rb;
    Vector2 moveInput;           // желаемое направление [-1..1]
    Vector2 controlledVelocity;  // управл€ема€ скорость
    Vector2 externalVelocity;    // внешние импульсы

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // рыба в воде, гравитаци€ не нужна
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
    }

    // ¬ызывает »» рыбы
    public void SetMoveInput(Vector2 dir)
    {
        // ограничиваем длину вектора
        moveInput = dir.sqrMagnitude > 1f ? dir.normalized : dir;
    }

    // »мпульсы от внешних событий (взрыв, удар и т.п.)
    public void AddImpulse(Vector2 impulse)
    {
        externalVelocity += impulse;
    }

    void FixedUpdate()
    {
        // целева€ скорость по вводу
        Vector2 targetVelocity = moveInput * moveSpeed;

        // плавный выход на целевую скорость
        controlledVelocity = Vector2.MoveTowards(
            controlledVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );

        // затухание внешних импульсов
        externalVelocity = Vector2.Lerp(
            externalVelocity,
            Vector2.zero,
            drag * Time.fixedDeltaTime
        );

        // итогова€ скорость рыбы
        rb.linearVelocity = controlledVelocity + externalVelocity;
    }
}
