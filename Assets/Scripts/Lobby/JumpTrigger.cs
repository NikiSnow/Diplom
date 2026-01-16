using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
    [SerializeField] bool EnableJump = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().ChangeJump(EnableJump);
        }
    }
}
