using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FishWaterPhysics2D : MonoBehaviour
{
    [Header("Движение в воде")]
    public float moveSpeed = 3f;
    public float acceleration = 12f;
    public float drag = 4f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 controlledVelocity;
    private Vector2 externalVelocity;

    public Vector2 CurrentVelocity => rb != null ? rb.linearVelocity : Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
    }

    public void SetMoveInput(Vector2 dir)
    {
        moveInput = dir.sqrMagnitude > 1f ? dir.normalized : dir;
    }

    public void AddImpulse(Vector2 impulse)
    {
        externalVelocity += impulse;
    }

    public void StopImmediately()
    {
        moveInput = Vector2.zero;
        controlledVelocity = Vector2.zero;
        externalVelocity = Vector2.zero;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;

        controlledVelocity = Vector2.MoveTowards(
            controlledVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );

        externalVelocity = Vector2.Lerp(
            externalVelocity,
            Vector2.zero,
            drag * Time.fixedDeltaTime
        );

        rb.linearVelocity = controlledVelocity + externalVelocity;
    }
}