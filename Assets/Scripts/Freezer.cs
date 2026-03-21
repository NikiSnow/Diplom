using UnityEngine;

public class Freezer : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float Cd;
    [SerializeField] GameObject Bullet;
    [SerializeField] Transform bulletPos;
    [SerializeField] float Speed;
    Vector2 direction;

    void Update()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        direction = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
        //Time.timeScale = 0.4f;
    }
    public void Shoot()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        direction = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );
        direction.Normalize();
        var Bul = Instantiate(Bullet, bulletPos.position, bulletPos.rotation);
        Bul.GetComponent<Rigidbody2D>().linearVelocity = direction * Speed;
    }
}
