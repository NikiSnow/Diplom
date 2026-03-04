using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player3d : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] int LayerIndex = 0;
    [SerializeField] int AmountOfLayers;
    [SerializeField] float PosDiff;
    [SerializeField] float transitionDuration = 0.35f;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float Speed;

    [Header("YLayers")]
    [SerializeField] StructureLayersByOne Stru;
    [SerializeField] bool IsAboveWater = true;

    private bool isTransitioning = false;
    private float transitionProgress = 0f;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Color targetColor;
    private bool movingDown = false;
    float hor;
    float ver;

    void Update()
    {
        Diving();
        if (isTransitioning) return;
        Moving();
    }

    public void Moving()
    {
        hor = Input.GetAxis("Horizontal");
        ver = Input.GetAxis("Vertical");
        if (hor != 0 || ver != 0)
        {
            rb.linearVelocity = new Vector2(Speed * hor, Speed * ver);
        }
    }

    public void Diving()
    {
        if (IsAboveWater)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !isTransitioning) //Down
            {
                StartTransition(true);
            }

            if (Input.GetKeyDown(KeyCode.E) && !isTransitioning) //Up
            {
                StartTransition(false);
            }
        }

        if (isTransitioning)
        {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;
                rb.linearVelocity = Vector2.zero;
            }

            // Плавное изменение позиции
            transform.position = Vector3.Lerp(startPosition, targetPosition, transitionProgress);

        }
    }

    void StartTransition(bool IsDown)
    {
        isTransitioning = true;
        rb.linearVelocity = Vector2.zero;
        transitionProgress = 0f;
        movingDown = IsDown;
        // Запоминаем начальные значения
        startPosition = transform.position;

        Stru.StartTransition(IsDown);

        // Устанавливаем целевые значения
        if (IsDown) //Down
        {
            if (LayerIndex == AmountOfLayers - 1)
            {
                isTransitioning = false;
                return;
            }
            LayerIndex++;
            targetPosition = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y - PosDiff, this.gameObject.transform.position.z);
        }
        else //Up
        {
            if (LayerIndex == 0)
            {
                isTransitioning = false;
                return;
            }
            LayerIndex--;
            targetPosition = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + PosDiff, this.gameObject.transform.position.z);
        }
    }
}
