using UnityEngine;

public class StupidFish : MonoBehaviour
{
    [SerializeField] GameObject Player;
    bool IsCaring;

    // Update is called once per frame
    void Update()
    {
        IsCaring = Vector2.Distance(Player.transform.position, transform.position) < 5f;
        Act();
    }

    void Act()
    {

    }
}
