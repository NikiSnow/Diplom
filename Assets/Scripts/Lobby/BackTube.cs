using UnityEngine;

public class BackTube : MonoBehaviour
{
    [SerializeField] Transform TeleportTo;
    [SerializeField] LobbyCamera LCamera;
    [SerializeField] GameObject LobbyPlayer;
    [SerializeField] Rigidbody2D LobbyPlayerRb;
    [SerializeField] GameObject WaterPlayer;
    [SerializeField] Vector2 ForceVector = new Vector2 (1f,1f);
    [SerializeField] JumpTrigger LobbyJTrigger;

    bool InZone = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InZone = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InZone = false;
        }
    }

    private void Update()
    {
        if (InZone && Input.GetKeyDown(KeyCode.E))
        {
            LobbyPlayer.SetActive(true);
            //WaterPlayer.SetActive(false);
            LCamera.SetTarget(LobbyPlayer.transform);
            LCamera.SetLobby();
            LobbyPlayer.transform.position = TeleportTo.position;

            LobbyPlayerRb.linearVelocity = Vector2.zero;
            LobbyPlayerRb.AddForce(ForceVector,ForceMode2D.Impulse);
            LobbyJTrigger.SetStop();
        }
    }
}
