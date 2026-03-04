using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureLayers : MonoBehaviour
{
    [SerializeField] Player3d Player;

    [SerializeField] List<TilemapCollider2D> Colliders2d;
    [SerializeField] List<Tilemap> Layers;
    [SerializeField] List<Tilemap> VisualTiles;
    [SerializeField] float transitionDuration = 0.35f;

    private bool isTransitioning = false;

    int LayerIndex = 0;
    int PrevLayerIndex;
    private float transitionProgress = 0f;
    Color ColorTrans = new Color(1, 1, 1, 0);
    Color ColorOpaq = new Color(1, 1, 1, 1);
    private bool movingDown = false;


    void Update()
    {
        if (isTransitioning)
        {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;
                DoneTransitioning();
            }

            if (!movingDown)
            {
                Layers[LayerIndex].color = Color.Lerp(ColorTrans, ColorOpaq, transitionProgress);
            }
            else
            {
                Layers[PrevLayerIndex].color = Color.Lerp(ColorOpaq, ColorTrans, transitionProgress);
            }
            VisualTiles[PrevLayerIndex].color = Color.Lerp(ColorOpaq, ColorTrans, transitionProgress);
            VisualTiles[LayerIndex].color = Color.Lerp(ColorOpaq, ColorOpaq, transitionProgress);
        }
    }

    public void StartTransition(bool IsDown)
    {
        isTransitioning = true;
        transitionProgress = 0f;
        movingDown = IsDown;

        if (IsDown) //Down
        {
            if (LayerIndex == Colliders2d.Count - 1)
            {
                isTransitioning = false;
                return;
            }
            PrevLayerIndex = LayerIndex;
            Colliders2d[LayerIndex].enabled = false;
            LayerIndex++;
            Colliders2d[LayerIndex].enabled = true;
        }
        else //Up
        {
            if (LayerIndex == 0)
            {
                isTransitioning = false;
                return;
            }
            PrevLayerIndex = LayerIndex;
            Colliders2d[LayerIndex].enabled = false;
            LayerIndex--;
            Colliders2d[LayerIndex].enabled = true;
        }
    }

    public void DoneTransitioning()
    {
        if (!movingDown)
        {

        }
        else
        {

        }
    }
}
