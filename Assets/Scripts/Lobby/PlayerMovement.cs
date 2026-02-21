using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float Speed;
    [SerializeField] float JumpForce;
    [SerializeField] private float rayLength = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] float posY;    //On lobby ground pos Y
    float hor;
    bool IsGrounded = false;
    public bool LockControls = false;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (LockControls) return;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            rayLength,
            groundLayer
        );

        IsGrounded = hit.collider != null;

        hor = Input.GetAxis("Horizontal");
        if (hor != 0)
        {
            rb.linearVelocity = new Vector2(Speed * hor, rb.linearVelocity.y);
        }
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
        }
    }

    public void ChangeJump(bool AbleJump)
    {
        if (AbleJump)
        {
            rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        }
        else
        {
            rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
            transform.position = new Vector2(transform.position.x,posY);
        }
    }
}
