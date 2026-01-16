using UnityEngine;
using UnityEngine.UIElements;

public class LobbyCamera : MonoBehaviour
{
    [SerializeField] Transform Target;
    [SerializeField] Vector3 LobbyOffset = new Vector3(6.3f, 0f, -10f);
    [SerializeField] Vector3 FallOffset = new Vector3(0, -5, -10f);
    [SerializeField] float LerpSpeed = 15f;
    Vector3 MainOffset;
    bool Swap = false;

    private void Start()
    {
        MainOffset = LobbyOffset;
    }

    void Update()
    {
        if (Target.position.x + MainOffset.x < 0)
        {
            transform.position = new Vector3(0, Target.position.y + MainOffset.y, MainOffset.z);
        }
        else
        {
            if (Mathf.Abs((Target.position.x + MainOffset.x) - transform.position.x) > 1f && Swap)
            {
                Vector3 next = new Vector3(Target.position.x + MainOffset.x, Target.position.y + MainOffset.y, MainOffset.z);

                transform.position = Vector3.Lerp(transform.position, next, LerpSpeed * Time.deltaTime);
                Debug.Log("Leping");
            }
            else
            {
                Swap = false;
                transform.position = new Vector3(Target.position.x + MainOffset.x, Target.position.y + MainOffset.y, MainOffset.z);
            }
        }
    }

    public void SetFall()
    {
        MainOffset = FallOffset;
        Swap = true;
    }

    public void SetLobby()
    {
        //Swap = true;
        MainOffset = LobbyOffset;
    }
}
