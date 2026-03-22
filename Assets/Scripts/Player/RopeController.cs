using UnityEngine;

public class RopeController : MonoBehaviour
{
    [Header("Компоненты")]
    public Transform player;           // Игрок
    public Transform fish;             // Рыба
    public HingeJoint2D[] ropeSegments; // Все сегменты веревки

    [Header("Настройки")]
    public float pullSpeed = 2f;       // Скорость натяжения
    public float minRopeLength = 1f;    // Минимальная длина веревки
    public KeyCode pullKey = KeyCode.Mouse0; // Кнопка натяжения

    private float currentRopeLength;
    private float initialRopeLength;

    void Start()
    {
        // Получаем все сегменты веревки (если не назначены)
        if (ropeSegments == null || ropeSegments.Length == 0)
        {
            ropeSegments = GetComponentsInChildren<HingeJoint2D>();
        }

        // Вычисляем начальную длину веревки
        initialRopeLength = CalculateRopeLength();
        currentRopeLength = initialRopeLength;
    }

    void Update()
    {
        // Натяжение веревки при зажатой кнопке
        if (Input.GetKey(pullKey))
        {
            PullRope();
        }
    }

    void PullRope()
    {
        // Уменьшаем желаемую длину веревки
        currentRopeLength -= pullSpeed * Time.deltaTime;
        currentRopeLength = Mathf.Max(minRopeLength, currentRopeLength);

        // Применяем новую длину
        AdjustRopeLength();
    }

    void AdjustRopeLength()
    {
        // Вычисляем текущую длину
        float currentLength = CalculateRopeLength();

        if (currentLength > currentRopeLength)
        {
            // Веревка слишком длинная - нужно подтянуть
            Vector2 direction = (fish.position - player.position).normalized;

            // Применяем силу к рыбе, чтобы она двигалась к игроку
            Rigidbody2D fishRb = fish.GetComponent<Rigidbody2D>();
            if (fishRb != null)
            {
                float pullForce = (currentLength - currentRopeLength) * 10f;
                fishRb.AddForce(direction * pullForce, ForceMode2D.Force);
            }
        }
    }

    float CalculateRopeLength()
    {
        if (ropeSegments.Length == 0) return 0;

        float totalLength = 0;
        Vector2 previousPoint = player.position;

        foreach (var segment in ropeSegments)
        {
            Vector2 currentPoint = segment.transform.position;
            totalLength += Vector2.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        // Добавляем расстояние от последнего сегмента до рыбы
        totalLength += Vector2.Distance(previousPoint, fish.position);

        return totalLength;
    }
}
