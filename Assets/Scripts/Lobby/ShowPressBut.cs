using UnityEngine;

public class ShowPressBut : MonoBehaviour
{
    [SerializeField] GameObject asd;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

        }
    }
}
