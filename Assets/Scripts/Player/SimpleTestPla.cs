using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class SimpleTestPla : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float BaseSpeed;
    [SerializeField] float PullingSpeed;
    [SerializeField] float Speed;
    [Header("HarpSettings")]
    [SerializeField] GameObject Target;
    [SerializeField] Rigidbody2D RopeRb;
    [SerializeField] HingeJoint2D Rope;
    [SerializeField] float PullSpeed;

    // Update is called once per frame
    void Update()
    {
        Move();
        //Pull();
    }

    void Move()
    {

        float Hor = Input.GetAxis("Horizontal");
        float Ver = Input.GetAxis("Vertical");

        if (Hor != 0 || Ver != 0)
        {
            rb.linearVelocity = new Vector2(Hor * Speed, Ver * Speed);
        }
    }

    void Pull()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Speed = PullingSpeed;
            //Rope.enabled = false;
            Vector2 directionAwayFromTarget = (transform.position - Target.transform.position).normalized;
            RopeRb.linearVelocity = directionAwayFromTarget * Speed;

            Vector2 anchor = Rope.anchor;
            anchor.y = anchor.y - 0.01f;
            Rope.anchor = anchor;
            //Rope.anchor.y = Rope.anchor.y - 0.1;
            //Rope.transform.Translate(directionAwayFromTarget * Speed * Time.deltaTime);
            //Rope.enabled = true;
        }
        else
        {
            Speed = BaseSpeed;
            //Rope.enabled = true;
        }

    }
}
