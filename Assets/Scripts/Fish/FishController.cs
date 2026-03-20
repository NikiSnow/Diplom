using UnityEngine;

public enum FishAttitude
{
    Neutral,    // игнорирует игрока
    Aggressive, // преследует
    Fearful     // боится игрока
}

public enum FishGroupType
{
    Solo,   // одиночная рыба
    Shoal   // косяк / стая
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(FishWaterPhysics2D))]
public class FishController : MonoBehaviour
{
    [Header("Базовые параметры")]
    public string fishId = "default_fish";
    public FishGroupType groupType = FishGroupType.Solo;
    public FishAttitude attitude = FishAttitude.Neutral;
    public float swimSpeed = 3f;
    public float viewRadius = 5f;
    public int maxHealth = 1;

    [Header("2D side-profile")]
    public float verticalInfluence = 0.5f; // насколько сильно разрешаем движение по Y (0..1)
    public float forwardBias = 0.3f;       // небольшой прижим к текущему курсу

    [Header("Зона обитания")]
    public Transform zoneCenterOverride;
    public float zoneRadius = 8f;
    public float zoneSoftRadius = 5f;

    [Header("Вертикальный синус")]
    public float swayStrength = 0.4f;
    public float swayFrequency = 1.2f;

    [Header("Wander / шум курса")]
    public float wanderNoiseStrength = 0.6f;
    public float wanderNoiseSpeed = 0.4f;

    [Header("Shoal (boids)")]
    public LayerMask fishLayerMask;
    public float neighborRadius = 3f;
    public float separationRadius = 0.7f;
    public float separationWeight = 1.8f;
    public float cohesionWeight = 1.0f;
    public float alignmentWeight = 1.2f;

    [Header("Steering")]
    public float steeringResponsiveness = 4f;
    public float idleDamping = 2f;

    [Header("Паника / общий страх")]
    public bool shareFearInShoal = true;
    public float panicDuration = 2f;
    public float panicWeightSelf = 3.0f;
    public float panicWeightFromShoal = 2.5f;

    [Header("Препятствия")]
    public LayerMask obstacleMask;          // слой стен / декора
    public float obstacleCheckDistance = 1.5f;
    public float obstacleSideOffset = 0.5f; // сейчас не используется, можно оставить под будущее
    public float obstacleAvoidWeight = 3f;
    public float bodyRadius = 0.5f;         // примерный "радиус" рыбы (автооценка в Awake)

    [Header("Память об опасных направлениях")]
    public bool enableMemory = true;        // включена ли память
    public float memoryDuration = 2f;       // сколько секунд помнить
    public float memoryAvoidWeight = 2f;    // насколько сильно избегать запомненное направление

    FishWaterPhysics2D waterPhysics;
    Rigidbody2D rb;
    Transform player;

    Vector3 homePosition;
    int currentHealth;

    Vector2 lastMoveDir = Vector2.right;

    float noiseSeedX;
    float noiseSeedY;
    float swayPhase;

    bool isPanicked;
    float panicTimer;

    bool playerInRange;            // был ли игрок в радиусе в прошлом кадре
    bool wasAvoidingObstacle;      // избегали ли препятствие в прошлом кадре

    // память
    Vector2 lastDangerDirection;
    float memoryTimer;

    public Vector2 CurrentDir => lastMoveDir;
    public bool IsPanicked => isPanicked;

    void Awake()
    {
        waterPhysics = GetComponent<FishWaterPhysics2D>();
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;

        waterPhysics.moveSpeed = swimSpeed;

        homePosition = transform.position;
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        noiseSeedX = Random.value * 100f;
        noiseSeedY = Random.value * 100f;
        swayPhase = Random.value * Mathf.PI * 2f;

        // Примерная оценка размера рыбы по коллайдеру
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            bodyRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
        }
    }

    void Update()
    {
        // таймер паники
        if (isPanicked)
        {
            panicTimer -= Time.deltaTime;
            if (panicTimer <= 0f)
            {
                isPanicked = false;
                Debug.Log($"{fishId}: паника закончилась");
            }
        }

        // таймер памяти
        if (memoryTimer > 0f)
            memoryTimer -= Time.deltaTime;

        Vector2 steering = ComputeSteering();

        if (steering.sqrMagnitude > 0.0001f)
        {
            Vector2 targetDir = steering.normalized;
            lastMoveDir = Vector2.Lerp(
                lastMoveDir,
                targetDir,
                steeringResponsiveness * Time.deltaTime
            );
        }
        else
        {
            lastMoveDir = Vector2.Lerp(
                lastMoveDir,
                Vector2.zero,
                idleDamping * Time.deltaTime
            );
        }

        waterPhysics.SetMoveInput(lastMoveDir);

        if (lastMoveDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(lastMoveDir.y, lastMoveDir.x) * Mathf.Rad2Deg;
            rb.SetRotation(angle);
        }
    }

    Vector2 ComputeSteering()
    {
        Vector2 result = Vector2.zero;
        Vector2 pos = transform.position;

        // -------- 1. Зона обитания --------
        Vector3 zoneCenter = zoneCenterOverride != null
            ? zoneCenterOverride.position
            : homePosition;

        Vector2 toZoneCenter = (Vector2)(zoneCenter - transform.position);
        float distToZoneCenter = toZoneCenter.magnitude;

        if (distToZoneCenter > zoneSoftRadius)
        {
            float t = Mathf.InverseLerp(zoneSoftRadius, zoneRadius, distToZoneCenter);
            t = Mathf.Clamp01(t);
            result += toZoneCenter.normalized * (1f + t * 2f);
        }

        // -------- 2. Wander (Perlin noise) --------
        float tTime = Time.time * wanderNoiseSpeed;
        float nx = Mathf.PerlinNoise(noiseSeedX, tTime) * 2f - 1f;
        float ny = Mathf.PerlinNoise(noiseSeedY, tTime) * 2f - 1f;
        Vector2 noiseDir = new Vector2(nx, ny);

        if (noiseDir.sqrMagnitude > 0.0001f)
        {
            noiseDir.Normalize();
            result += noiseDir * wanderNoiseStrength;
        }

        // -------- 3. Вертикальный синус --------
        if (swayStrength > 0f)
        {
            float sway = Mathf.Sin(Time.time * swayFrequency + swayPhase);
            result += Vector2.up * (sway * swayStrength);
        }

        // -------- 4. Реакция на игрока / собственный страх --------
        bool playerInRangeNow = false;

        if (player != null)
        {
            Vector2 toPlayer = (Vector2)(player.position - transform.position);
            float dist = toPlayer.magnitude;

            if (dist <= viewRadius && dist > 0.01f)
            {
                playerInRangeNow = true;
                Vector2 dirToPlayer = toPlayer / dist;

                switch (attitude)
                {
                    case FishAttitude.Aggressive:
                        result += dirToPlayer * 2.5f;
                        break;

                    case FishAttitude.Fearful:
                        result += -dirToPlayer * panicWeightSelf;
                        SetPanicked("увидела игрока, боится");
                        RememberDanger(dirToPlayer, "игрок (страх)");
                        break;

                    case FishAttitude.Neutral:
                        break;
                }
            }

            if (isPanicked)
            {
                Vector2 fleeDir = (Vector2)(transform.position - player.position);
                if (fleeDir.sqrMagnitude > 0.0001f)
                {
                    fleeDir.Normalize();
                    result += fleeDir * panicWeightSelf;
                }
            }
        }

        // debug: вход/выход из радиуса игрока
        if (playerInRangeNow && !playerInRange)
        {
            Debug.Log($"{fishId}: игрок вошёл в радиус ({attitude})");
            if (attitude == FishAttitude.Aggressive)
                Debug.Log($"{fishId}: агр на игрока");
            if (attitude == FishAttitude.Fearful)
                Debug.Log($"{fishId}: напугана игроком");
        }
        else if (!playerInRangeNow && playerInRange)
        {
            Debug.Log($"{fishId}: игрок вышел из радиуса");
        }
        playerInRange = playerInRangeNow;

        // -------- 5. Shoal / boids + общий страх --------
        if (groupType == FishGroupType.Shoal && fishLayerMask != 0)
        {
            Collider2D[] neighbors = Physics2D.OverlapCircleAll(
                pos,
                neighborRadius,
                fishLayerMask
            );

            if (neighbors.Length > 1)
            {
                Vector2 cohesion = Vector2.zero;
                Vector2 alignment = Vector2.zero;
                Vector2 separation = Vector2.zero;
                int count = 0;

                bool neighborPanicked = false;

                foreach (var col in neighbors)
                {
                    if (col.attachedRigidbody == null || col.attachedRigidbody == rb)
                        continue;

                    FishController other = col.GetComponent<FishController>();
                    if (other == null) continue;

                    Vector2 otherPos = other.transform.position;
                    Vector2 toOther = otherPos - pos;
                    float d = toOther.magnitude;
                    if (d < 0.0001f) continue;

                    cohesion += otherPos;
                    alignment += other.CurrentDir;

                    if (d < separationRadius)
                        separation += -toOther.normalized / Mathf.Max(d, 0.1f);

                    if (other.IsPanicked)
                        neighborPanicked = true;

                    count++;
                }

                if (count > 0)
                {
                    cohesion = (cohesion / count - pos).normalized;
                    if (alignment.sqrMagnitude > 0.0001f)
                        alignment.Normalize();
                    if (separation.sqrMagnitude > 0.0001f)
                        separation.Normalize();

                    result += cohesion * cohesionWeight;
                    result += alignment * alignmentWeight;
                    result += separation * separationWeight;
                }

                if (neighborPanicked && shareFearInShoal && player != null)
                {
                    Vector2 toPlayer = (Vector2)(player.position - transform.position);
                    Vector2 fleeDir = -toPlayer;

                    if (fleeDir.sqrMagnitude > 0.0001f)
                    {
                        fleeDir.Normalize();
                        result += fleeDir * panicWeightFromShoal;

                        SetPanicked("заразилась паникой от стаи");
                        RememberDanger(toPlayer, "паника от стаи (игрок)");
                    }
                }
            }
        }

        // -------- 6. Обход препятствий (скольжение по стене) --------
        bool avoiding = false;

        if (obstacleMask != 0)
        {
            Vector2 fwd = lastMoveDir.sqrMagnitude > 0.0001f
                ? lastMoveDir.normalized
                : Vector2.right;

            // начинаем луч не из центра, а чуть впереди (у "носа" рыбы)
            Vector2 rayOrigin = pos + fwd * bodyRadius * 0.8f;

            RaycastHit2D hitForward = Physics2D.Raycast(
                rayOrigin,
                fwd,
                obstacleCheckDistance,
                obstacleMask
            );

            if (hitForward.collider != null)
            {
                Vector2 normal = hitForward.normal;
                Vector2 tangent = new Vector2(-normal.y, normal.x);

                if (Vector2.Dot(tangent, fwd) < 0f)
                    tangent = -tangent;

                result += tangent * obstacleAvoidWeight;
                avoiding = true;

                // запоминаем, что прямо по курсу была стена
                RememberDanger(fwd, "препятствие");
            }
        }

        if (avoiding && !wasAvoidingObstacle)
        {
            Debug.Log($"{fishId}: избегает препятствие (скольжение по стене)");
        }
        wasAvoidingObstacle = avoiding;

        // -------- 7. Память об опасных направлениях --------
        if (enableMemory && memoryTimer > 0f && lastDangerDirection.sqrMagnitude > 0.0001f)
        {
            float t = Mathf.Clamp01(memoryTimer / memoryDuration);
            Vector2 away = (-lastDangerDirection).normalized;

            result += away * memoryAvoidWeight * t;
        }

        // небольшое "желание" продолжать двигаться вперёд,
        // чтобы рыба не залипала на месте
        if (lastMoveDir.sqrMagnitude > 0.01f)
        {
            result += lastMoveDir.normalized * forwardBias;
        }

        // уплощаем движение по вертикали, чтобы меньше было "закруток"
        result.y *= verticalInfluence;

        return result;
    }

    void SetPanicked(string reason)
    {
        if (!isPanicked)
        {
            Debug.Log($"{fishId}: вошла в состояние паники ({reason})");
        }
        isPanicked = true;
        panicTimer = panicDuration;
    }

    void RememberDanger(Vector2 dir, string reason)
    {
        if (!enableMemory) return;
        if (dir.sqrMagnitude < 0.0001f) return;

        lastDangerDirection = dir.normalized;
        memoryTimer = memoryDuration;

        Debug.Log($"{fishId}: запомнила опасное направление ({reason})");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        PlayerDashScr dash = collision.collider.GetComponent<PlayerDashScr>();

        if (dash != null && dash.IsDashing)
        {
            Debug.Log($"{fishId}: поймана дэшем");
            OnCaughtByPlayer();
        }
        else
        {
            Debug.Log($"{fishId}: столкнулась с игроком без дэша");

            Vector2 toPlayer = (Vector2)collision.transform.position - (Vector2)transform.position;
            RememberDanger(toPlayer, "столкновение с игроком");

            if (groupType == FishGroupType.Shoal && shareFearInShoal)
                SetPanicked("столкнулась с игроком");
        }
    }

    void OnCaughtByPlayer()
    {
        currentHealth = 0;

        Debug.Log($"рыба поймана: {fishId}");

        // TODO: сюда добавить:
        // - добавление в улов/инвентарь
        // - анимацию поимки
        // - звук
        // - UI-плашку
        // - уведомление менеджеру

        Destroy(gameObject);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Debug.Log($"{fishId}: погибла от урона");
            OnCaughtByPlayer();
        }
    }
}
