using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFishScr : MonoBehaviour
{
    [Header("Настройки точек")]
    [SerializeField] List<Transform> waypoints = new List<Transform>(); // Лист всех точек

    [Header("Настройки движения")]
    [SerializeField] float speed = 3f;
    [SerializeField] float pauseTime = 0.5f;
    [SerializeField] float arrivalDistance = 0.1f;

    [Header("Выбор следующей точки")]
    [SerializeField] bool randomSelection = true; // true = случайно, false = по порядку
    [SerializeField] bool avoidImmediateReturn = true; // Избегать возврата в предыдущую точку
    [SerializeField] float chanceToReturn = 0.1f; // Шанс вернуться к предыдущей точке

    [SerializeField] float rotationSpeed = 180f;

    int _currentWaypointIndex = 0;
    int _previousWaypointIndex = -1;
    float _pauseTimer = 0f;
    bool _isPaused = false;
    float _currentAngle = 0f;
    bool Freezed = false;
    Coroutine FreezeCoroutine;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //collision.GetComponent<PlayerDashScr>().StopRb();
            Destroy(this.gameObject);
        }
        else if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);
            Freeze();
        }
    }
    void Freeze()
    {
        Freezed = true;
        if (FreezeCoroutine != null)
        {
            StopCoroutine(FreezeCoroutine);
            FreezeCoroutine = null;
        }
        FreezeCoroutine = StartCoroutine(BeFrezzed());
        //Visual
    }
    IEnumerator BeFrezzed()
    {
        yield return new WaitForSeconds(2.5f);
        Freezed = false;
    }
    void Start()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("Добавьте точки в список waypoints!");
            enabled = false;
            return;
        }

        // Начинаем с первой точки
        _currentWaypointIndex = 0;
        if (waypoints[_currentWaypointIndex] != null)
        {
            transform.position = waypoints[_currentWaypointIndex].position;
        }

        // Выбираем следующую точку
        SelectNextWaypoint();
        _currentAngle = transform.eulerAngles.z;
    }

    void Update()
    {
        if (Freezed == true)
        {
            return;
        }
        // Движение к текущей точке
        Vector2 targetPos = waypoints[_currentWaypointIndex].position;
        Vector2 moveDirection = (targetPos - (Vector2)transform.position).normalized;
        if (_isPaused)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f) _isPaused = false;

            if (moveDirection.magnitude > 0.1f)
            {
                RotateTowardsDirection(moveDirection);
            }
            return;
        }

        if (_currentWaypointIndex >= waypoints.Count || waypoints[_currentWaypointIndex] == null)
        {
            Debug.LogWarning("Некорректная точка назначения!");
            return;
        }


        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        // Поворот
        if (moveDirection.magnitude > 0.1f)
        {
            RotateTowardsDirection(moveDirection);
        }

        // Проверка прибытия
        if (Vector2.Distance(transform.position, targetPos) <= arrivalDistance)
        {
            OnWaypointReached();
        }
    }

    void SelectNextWaypoint()
    {
        _previousWaypointIndex = _currentWaypointIndex;

        if (randomSelection)
        {
            ChooseRandomWaypoint();
        }
        else
        {
            ChooseSequentialWaypoint();
        }
    }

    void ChooseRandomWaypoint()
    {
        if (waypoints.Count == 1) return;

        List<int> availableIndices = new List<int>();

        // Собираем все доступные индексы
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null && i != _currentWaypointIndex)
            {
                // Если включено избегание немедленного возврата, исключаем предыдущую точку
                if (!avoidImmediateReturn || i != _previousWaypointIndex)
                {
                    availableIndices.Add(i);
                }
            }
        }

        // Если нет доступных точек (все исключены), используем все кроме текущей
        if (availableIndices.Count == 0)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null && i != _currentWaypointIndex)
                {
                    availableIndices.Add(i);
                }
            }
        }

        // Иногда позволяем вернуться к предыдущей точке
        if (availableIndices.Count > 0 && Random.value < chanceToReturn && _previousWaypointIndex != -1)
        {
            _currentWaypointIndex = _previousWaypointIndex;
        }
        else if (availableIndices.Count > 0)
        {
            // Выбираем случайную точку из доступных
            _currentWaypointIndex = availableIndices[Random.Range(0, availableIndices.Count)];
        }
    }

    void ChooseSequentialWaypoint()
    {
        _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;

        // Пропускаем null точки
        while (waypoints[_currentWaypointIndex] == null && waypoints.Count > 1)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;
        }
    }

    void OnWaypointReached()
    {
        _isPaused = true;
        _pauseTimer = pauseTime;

        // Запоминаем достигнутую точку как предыдущую
        _previousWaypointIndex = _currentWaypointIndex;

        // Выбираем следующую точку
        SelectNextWaypoint();
    }

    void RotateTowardsDirection(Vector2 direction)
    {
        Vector2 NewDirection = new Vector2(
        direction.x - transform.position.x,
        direction.y - transform.position.y
        );

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        ApplyRotation(targetAngle);
    }

    float Calculate8DirectionAngle(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        float snappedAngle = Mathf.Round(angle / 45f) * 45f;
        return snappedAngle;
    }

    void ApplyRotation(float targetAngle)
    {
        float angleDiff = Mathf.DeltaAngle(_currentAngle, targetAngle);
        float maxRotation = rotationSpeed * Time.deltaTime;
        float rotationThisFrame = Mathf.Clamp(angleDiff, -maxRotation, maxRotation);

        _currentAngle += rotationThisFrame;
        transform.rotation = Quaternion.Euler(0f, 0f, _currentAngle);
    }

    // Публичные методы для управления
    public void AddWaypoint(Transform newWaypoint)
    {
        if (newWaypoint != null && !waypoints.Contains(newWaypoint))
        {
            waypoints.Add(newWaypoint);
            Debug.Log($"Добавлена точка: {newWaypoint.name}");
        }
    }

    public void RemoveWaypoint(Transform waypointToRemove)
    {
        if (waypoints.Contains(waypointToRemove))
        {
            waypoints.Remove(waypointToRemove);

            // Корректируем индексы если нужно
            if (_currentWaypointIndex >= waypoints.Count)
            {
                _currentWaypointIndex = 0;
            }
        }
    }

    public void ClearWaypoints()
    {
        waypoints.Clear();
        _currentWaypointIndex = 0;
        _previousWaypointIndex = -1;
    }

    public void TeleportToWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count && waypoints[index] != null)
        {
            transform.position = waypoints[index].position;
            _currentWaypointIndex = index;
            SelectNextWaypoint();
        }
    }

    // Метод для выбора следующей точки по определенному условию
    public void SelectSpecificWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count && waypoints[index] != null)
        {
            _currentWaypointIndex = index;
        }
    }

    // Получить информацию о текущем маршруте
    public string GetCurrentRouteInfo()
    {
        if (_previousWaypointIndex >= 0 && _previousWaypointIndex < waypoints.Count &&
            _currentWaypointIndex < waypoints.Count)
        {
            string prevName = waypoints[_previousWaypointIndex]?.name ?? "null";
            string currName = waypoints[_currentWaypointIndex]?.name ?? "null";
            return $"Из: {prevName} → В: {currName}";
        }
        return "Нет данных о маршруте";
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Gizmos.color = Color.green;

        // Рисуем все точки
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);

                // Номера точек
#if UNITY_EDITOR
                UnityEditor.Handles.Label(waypoints[i].position + Vector3.up * 0.3f,
                    $"Point {i}\n{waypoints[i].name}");
#endif

                // Линии между точками
                if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    Gizmos.color = Color.green;
                }
            }
        }

        // Линия от последней к первой (замыкание)
        if (waypoints.Count > 1 && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
        }

        // Текущий путь
        if (waypoints.Count > 0 && _currentWaypointIndex < waypoints.Count &&
            waypoints[_currentWaypointIndex] != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, waypoints[_currentWaypointIndex].position);

            // Стрелка направления
            Vector2 dir = ((Vector2)waypoints[_currentWaypointIndex].position - (Vector2)transform.position).normalized;
            //DrawArrow(transform.position, dir, 0.5f, Color.red);
        }
    }
}
