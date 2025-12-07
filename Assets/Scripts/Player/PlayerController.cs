using UnityEngine;

[RequireComponent(typeof(WaterPhysics2D))]
public class PlayerController : MonoBehaviour
{
    WaterPhysics2D waterPhysics;
    public Vector2 lastMoveDir; // Последнее направление движения (под анимации/поворот)

    // TODO: раскомментировать, когда будут анимации/звук
    // Animator animator;    // Контроллер анимаций плавания
    // AudioSource audioSrc; // Звук плавания / рывков

    void Awake()
    {
        // Кешируем ссылку на модуль воды
        waterPhysics = GetComponent<WaterPhysics2D>();

        // TODO: если аниматор/аудио на ребёнке:
        // animator = GetComponentInChildren<Animator>();
        // audioSrc = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Читаем ввод по осям (WASD/стрелки)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 inputDir = new Vector2(h, v);

        // Сохраняем направление для визуала, если реально двигаемся
        if (inputDir.sqrMagnitude > 0.0001f)
            lastMoveDir = inputDir.normalized;

        // Передаём ввод в модуль воды (он уже учитывает глобальную физику)
        waterPhysics.SetMoveInput(inputDir);
        waterPhysics.SetRotation(lastMoveDir);

        // TODO: анимации
        // if (animator != null)
        // {
        //     // Скорость для blend tree (idle / swim)
        //     animator.SetFloat("Speed", inputDir.magnitude);
        //
        //     // Направление взгляда/плавания
        //     animator.SetFloat("MoveX", lastMoveDir.x);
        //     animator.SetFloat("MoveY", lastMoveDir.y);
        // }

        // TODO: звук плавания
        // if (audioSrc != null)
        // {
        //     bool isMoving = inputDir.sqrMagnitude > 0.01f;
        //     audioSrc.mute = !isMoving; // простая заглушка: играем звук только при движении
        // }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            waterPhysics.RotateTowardsMouse();
        }
        else if (waterPhysics.IsHolding)
        {
            waterPhysics.IsHolding = false;
            Time.timeScale = 1f;
            waterPhysics.Release();
        }
    }


}
