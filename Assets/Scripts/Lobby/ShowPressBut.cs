using TMPro;
using UnityEngine;

public class ShowPressBut : MonoBehaviour
{
    [SerializeField] TMP_Text Button;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Button.gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Button.gameObject.SetActive(false);
        }
    }
}
