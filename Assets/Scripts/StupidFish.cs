using UnityEngine;

public class StupidFish : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] float Speed;
    [SerializeField] Rigidbody2D rb;
    //bool IsCaring;

    // Update is called once per frame
    void Update()
    {
        //IsCaring = Vector2.Distance(Player.transform.position, transform.position) < 5f;
        Act();
    }

    void Act()
    {
        Vector2 directionAwayFromTarget = (transform.position - Player.transform.position).normalized;

        rb.linearVelocity = directionAwayFromTarget * Speed;
        // Движение от объекта
        //transform.Translate(directionAwayFromTarget * Speed * Time.deltaTime);

    }
}
