using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] LobbyCamera MainCam;
    [SerializeField] bool Falling = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Falling)
            {
                MainCam.SetLobby();
            }
            else
            {
                MainCam.SetFall();
            }
        }
    }
}
