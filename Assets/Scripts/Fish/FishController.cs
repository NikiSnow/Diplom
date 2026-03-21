using UnityEngine;

public enum FishAttitude
{
    Neutral,    // не реагирует на игрока
    Aggressive, // преследует игрока
    Fearful     // боится игрока
}

public enum FishGroupType
{
    Solo,
    Shoal
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

    [Header("Поимка / заморозка")]
    public bool requiresFreezeToCatch = false;
    public float freezeDuration = 2.5f;

    [Header("2D side-profile")]
    public float verticalInfluence = 0.5f;
    public float forwardBias = 0.3f;

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
    public LayerMask obstacleMask;
    public float obstacleCheckDistance = 1.5f;
    public float obstacleSideOffset = 0.5f;
    public float obstacleAvoidWeight = 3f;
    public float bodyRadius = 0.5f;

    [Header("Память об опасных направлениях")]
    public bool enableMemory = true;
    public float memoryDuration = 2f;
    public float memoryAvoidWeight = 2f;

    private FishWaterPhysics2D waterPhysics;
    private Rigidbody2D rb;
    private Transform player;
    private Vector3 homePosition;

    private int currentHealth;
    private Vector2 lastMoveDir = Vector2.right;

    private float noiseSeedX;
    private float noiseSeedY;
    private float swayPhase;

    private bool isPanicked;
    private float panicTimer;
    private bool playerInRange;
    private bool wasAvoidingObstacle;

    private Vector2 lastDangerDirection;
    private float memoryTimer;

    private bool isFrozen;
    private float frozenTimer;

    private bool isCaughtOrDead;
    private int lastHandledPlayerContactFrame = -1;

    public Vector2 CurrentDir => lastMoveDir;
    public bool IsPanicked => isPanicked;
    public bool IsFrozen => isFrozen;

    private void Awake()
    {
        waterPhysics = GetComponent<FishWaterPhysics2D>();
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;

        waterPhysics.moveSpeed = swimSpeed;

        homePosition = transform.position;
        currentHealth = maxHealth;

        TryResolvePlayer();

        noiseSeedX = Random.value * 100f;
        noiseSeedY = Random.value * 100f;
        swayPhase = Random.value * Mathf.PI * 2f;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            bodyRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
    }

    private void Update()
    {
        TryResolvePlayer();
        UpdateFrozenState();

        if (isCaughtOrDead)
            return;

        if (isFrozen)
        {
            waterPhysics.StopImmediately();
            return;
        }

        if (isPanicked)
        {
            panicTimer -= Time.deltaTime;
            if (panicTimer <= 0f)
            {
                isPanicked = false;
                Debug.Log($"{fishId}: паника закончилась");
            }
        }

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

    private void TryResolvePlayer()
    {
        if (player != null)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void UpdateFrozenState()
    {
        if (!isFrozen)
            return;

        frozenTimer -= Time.unscaledDeltaTime;

        waterPhysics.StopImmediately();
        rb.linearVelocity = Vector2.zero;

        if (frozenTimer <= 0f)
        {
            isFrozen = false;
            frozenTimer = 0f;
            Debug.Log($"{fishId}: разморозилась");

            if (attitude == FishAttitude.Fearful)
                SetPanicked("разморозилась рядом с игроком");
        }
    }

    private Vector2 ComputeSteering()
    {
        Vector2 result = Vector2.zero;
        Vector2 pos = transform.position;

        // 1. Зона обитания
        Vector3 zoneCenter = zoneCenterOverride != null ? zoneCenterOverride.position : homePosition;

        float safeZoneRadius = Mathf.Max(zoneRadius, 0.1f);
        float safeSoftRadius = Mathf.Clamp(zoneSoftRadius, 0f, safeZoneRadius - 0.01f);

        Vector2 toZoneCenter = (Vector2)(zoneCenter - transform.position);
        float distToZoneCenter = toZoneCenter.magnitude;

        if (distToZoneCenter > safeSoftRadius)
        {
            float t = Mathf.InverseLerp(safeSoftRadius, safeZoneRadius, distToZoneCenter);
            t = Mathf.Clamp01(t);
            result += toZoneCenter.normalized * (1f + t * 2f);
        }

        // 2. Wander
        float tTime = Time.time * wanderNoiseSpeed;
        float nx = Mathf.PerlinNoise(noiseSeedX, tTime) * 2f - 1f;
        float ny = Mathf.PerlinNoise(noiseSeedY, tTime) * 2f - 1f;

        Vector2 noiseDir = new Vector2(nx, ny);
        if (noiseDir.sqrMagnitude > 0.0001f)
        {
            noiseDir.Normalize();
            result += noiseDir * wanderNoiseStrength;
        }

        // 3. Вертикальный синус
        if (swayStrength > 0f)
        {
            float sway = Mathf.Sin(Time.time * swayFrequency + swayPhase);
            result += Vector2.up * (sway * swayStrength);
        }

        // 4. Реакция на игрока
        bool playerInRangeNow = false;

        if (player != null)
        {
            Vector2 toPlayer = (Vector2)(player.position - transform.position);
            float distToPlayer = toPlayer.magnitude;

            if (distToPlayer <= viewRadius)
            {
                playerInRangeNow = true;

                Vector2 dirToPlayer = distToPlayer > 0.0001f ? toPlayer / distToPlayer : Vector2.zero;

                switch (attitude)
                {
                    case FishAttitude.Aggressive:
                        result += dirToPlayer * 2f;
                        break;

                    case FishAttitude.Fearful:
                        SetPanicked("увидела игрока");
                        RememberDanger(dirToPlayer, "игрок (страх)");
                        break;

                    case FishAttitude.Neutral:
                        break;
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
        }

        if (playerInRangeNow && !playerInRange)
            Debug.Log($"{fishId}: игрок вошёл в радиус ({attitude})");
        else if (!playerInRangeNow && playerInRange)
            Debug.Log($"{fishId}: игрок вышел из радиуса");

        playerInRange = playerInRangeNow;

        // 5. Стайность
        if (groupType == FishGroupType.Shoal && fishLayerMask != 0)
        {
            Collider2D[] neighbors = Physics2D.OverlapCircleAll(
                transform.position,
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

                foreach (Collider2D col in neighbors)
                {
                    if (col.attachedRigidbody == null || col.attachedRigidbody == rb)
                        continue;

                    FishController other = col.GetComponent<FishController>();
                    if (other == null)
                        other = col.GetComponentInParent<FishController>();

                    if (other == null || other == this)
                        continue;

                    Vector2 otherPos = other.transform.position;
                    Vector2 toOther = otherPos - pos;
                    float d = toOther.magnitude;

                    if (d < 0.0001f)
                        continue;

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

        // 6. Обход препятствий
        result += ComputeObstacleAvoidance(pos);

        // 7. Память об опасных направлениях
        if (enableMemory && memoryTimer > 0f && lastDangerDirection.sqrMagnitude > 0.0001f)
        {
            float t = Mathf.Clamp01(memoryTimer / memoryDuration);
            Vector2 away = (-lastDangerDirection).normalized;
            result += away * memoryAvoidWeight * t;
        }

        // 8. Небольшое желание продолжать текущий курс
        if (lastMoveDir.sqrMagnitude > 0.01f)
            result += lastMoveDir.normalized * forwardBias;

        // Уплощаем вертикаль, чтобы рыбу меньше крутило
        result.y *= verticalInfluence;

        return result;
    }

    private Vector2 ComputeObstacleAvoidance(Vector2 pos)
    {
        if (obstacleMask == 0)
        {
            wasAvoidingObstacle = false;
            return Vector2.zero;
        }

        Vector2 fwd = lastMoveDir.sqrMagnitude > 0.0001f ? lastMoveDir.normalized : Vector2.right;
        Vector2 side = new Vector2(-fwd.y, fwd.x);

        float rayDistance = Mathf.Max(obstacleCheckDistance, bodyRadius + 0.2f);
        Vector2 rayOrigin = pos + fwd * bodyRadius * 0.8f;

        Vector2 avoidance = Vector2.zero;
        bool avoiding = false;

        RaycastHit2D hitForward = Physics2D.Raycast(rayOrigin, fwd, rayDistance, obstacleMask);
        if (hitForward.collider != null)
        {
            Vector2 normal = hitForward.normal;
            Vector2 tangent = new Vector2(-normal.y, normal.x);

            if (Vector2.Dot(tangent, fwd) < 0f)
                tangent = -tangent;

            float weight = 1f - (hitForward.distance / Mathf.Max(rayDistance, 0.001f));
            avoidance += tangent * Mathf.Lerp(1f, 1.75f, weight);
            avoiding = true;

            RememberDanger(fwd, "препятствие");
        }

        RaycastHit2D hitPlusSide = Physics2D.Raycast(
            rayOrigin + side * obstacleSideOffset,
            fwd,
            rayDistance * 0.85f,
            obstacleMask
        );

        if (hitPlusSide.collider != null)
        {
            avoidance -= side * 0.75f;
            avoiding = true;
        }

        RaycastHit2D hitMinusSide = Physics2D.Raycast(
            rayOrigin - side * obstacleSideOffset,
            fwd,
            rayDistance * 0.85f,
            obstacleMask
        );

        if (hitMinusSide.collider != null)
        {
            avoidance += side * 0.75f;
            avoiding = true;
        }

        if (avoiding && !wasAvoidingObstacle)
            Debug.Log($"{fishId}: избегает препятствие");

        wasAvoidingObstacle = avoiding;

        if (!avoiding || avoidance.sqrMagnitude <= 0.0001f)
            return Vector2.zero;

        return avoidance.normalized * obstacleAvoidWeight;
    }

    private void SetPanicked(string reason)
    {
        if (!isPanicked)
            Debug.Log($"{fishId}: вошла в состояние паники ({reason})");

        isPanicked = true;
        panicTimer = panicDuration;
    }

    private void RememberDanger(Vector2 dir, string reason)
    {
        if (!enableMemory)
            return;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        lastDangerDirection = dir.normalized;
        memoryTimer = memoryDuration;
        Debug.Log($"{fishId}: запомнила опасное направление ({reason})");
    }

    public void Freeze()
    {
        Freeze(freezeDuration);
    }

    public void Freeze(float duration)
    {
        if (isCaughtOrDead)
            return;

        float finalDuration = duration > 0f ? duration : freezeDuration;
        finalDuration = Mathf.Max(finalDuration, 0.1f);

        isFrozen = true;
        frozenTimer = Mathf.Max(frozenTimer, finalDuration);

        isPanicked = false;
        panicTimer = 0f;

        waterPhysics.StopImmediately();
        rb.linearVelocity = Vector2.zero;

        Debug.Log($"{fishId}: заморожена на {finalDuration:0.00} сек.");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerContact(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerContact(other);
    }

    private void HandlePlayerContact(Collider2D other)
    {
        if (isCaughtOrDead || other == null)
            return;

        if (lastHandledPlayerContactFrame == Time.frameCount)
            return;

        if (!IsPlayerCollider(other))
            return;

        lastHandledPlayerContactFrame = Time.frameCount;

        Vector2 playerPosition = other.attachedRigidbody != null
            ? other.attachedRigidbody.position
            : (Vector2)other.transform.position;

        Vector2 toPlayer = playerPosition - (Vector2)transform.position;

        bool isDashHit = TryGetPlayerWaterPhysics(other, out WaterPhysics2D playerWaterPhysics) &&
                         playerWaterPhysics.IsDashing;

        if (isDashHit)
        {
            if (requiresFreezeToCatch && !isFrozen)
            {
                Debug.Log($"{fishId}: дэш попал, но без заморозки не ловится");

                RememberDanger(toPlayer, "дэш игрока без заморозки");

                if (attitude == FishAttitude.Fearful || groupType == FishGroupType.Solo)
                    SetPanicked("дэш игрока рядом");

                return;
            }

            Debug.Log($"{fishId}: поймана новым дэшем");
            OnCaughtByPlayer();
            return;
        }

        Debug.Log($"{fishId}: столкнулась с игроком без дэша");

        RememberDanger(toPlayer, "столкновение с игроком");

        if (groupType == FishGroupType.Shoal && shareFearInShoal)
            SetPanicked("столкнулась с игроком");
        else if (attitude == FishAttitude.Fearful)
            SetPanicked("игрок подошёл слишком близко");
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return true;

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }

    private bool TryGetPlayerWaterPhysics(Collider2D other, out WaterPhysics2D playerWaterPhysics)
    {
        playerWaterPhysics = null;

        if (other.attachedRigidbody != null)
            playerWaterPhysics = other.attachedRigidbody.GetComponent<WaterPhysics2D>();

        if (playerWaterPhysics == null)
            playerWaterPhysics = other.GetComponentInParent<WaterPhysics2D>();

        return playerWaterPhysics != null;
    }

    private void OnCaughtByPlayer()
    {
        if (isCaughtOrDead)
            return;

        isCaughtOrDead = true;
        currentHealth = 0;

        Debug.Log($"рыба поймана: {fishId}");

        // TODO:
        // - добавить в улов / инвентарь
        // - анимацию поимки
        // - звук
        // - UI
        // - уведомление менеджеру/спавнеру

        Destroy(gameObject);
    }

    public void TakeDamage(int amount)
    {
        if (isCaughtOrDead)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Debug.Log($"{fishId}: погибла от урона");
            OnCaughtByPlayer();
        }
    }
}