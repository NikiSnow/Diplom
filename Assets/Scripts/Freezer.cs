using UnityEngine;

public class Freezer : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float Cd = 0.35f;
    [SerializeField] private GameObject Bullet;
    [SerializeField] private Transform bulletPos;
    [SerializeField] private float Speed = 14f;

    private Vector2 direction;
    private float nextShootTime;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        RotateToMouse();
    }

    private void RotateToMouse()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        direction = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void Shoot()
    {
        if (Time.time < nextShootTime)
            return;

        if (mainCamera == null || Bullet == null || bulletPos == null)
        {
            Debug.LogWarning("Freezer: не назначены mainCamera / Bullet / bulletPos");
            return;
        }

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        direction = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        direction.Normalize();

        GameObject bulletInstance = Instantiate(Bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody2D bulletRb = bulletInstance.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
            bulletRb.linearVelocity = direction * Speed;
        else
            Debug.LogWarning("Freezer: у пули нет Rigidbody2D");

        nextShootTime = Time.time + Cd;
    }
}