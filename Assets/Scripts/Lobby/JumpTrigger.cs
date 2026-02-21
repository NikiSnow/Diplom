using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
    [SerializeField] bool EnableJump = false;
    bool ShoudStop = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player ThePlayer = other.gameObject.GetComponent<Player>();
            ThePlayer.ChangeJump(EnableJump);
            ThePlayer.LockControls = false;
            if (ShoudStop)
            {
                Rigidbody2D PlaRb = ThePlayer.GetComponent<Rigidbody2D>();
                PlaRb.linearVelocity = Vector2.zero;
                ShoudStop = false;
            }
        }
    }
    public void SetStop()
    {
        ShoudStop = true;
    }
}
