using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class StructureLayersByOne : MonoBehaviour
{
    [SerializeField] List<TilemapCollider2D> Colliders2d;
    [SerializeField] List<Tilemap> Layers;
    [SerializeField] List<TilemapRenderer> Renderers;
    //[SerializeField] List<Tilemap> VisualTiles;
    [SerializeField] float transitionDuration = 0.35f;
    [SerializeField] int SortingAbove = 5;
    [SerializeField] int SortingBelow = -5;
    [SerializeField] int SortingUnder = -15;
    [SerializeField] Light2D GlobalLight;

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
                Layers[LayerIndex].color = Color.Lerp(ColorOpaq, ColorTrans, transitionProgress);
            }
            else
            {
                Layers[PrevLayerIndex].color = Color.Lerp(ColorTrans, ColorOpaq, transitionProgress);
            }
            //VisualTiles[PrevLayerIndex].color = Color.Lerp(ColorOpaq, ColorTrans, transitionProgress);
            //VisualTiles[LayerIndex].color = Color.Lerp(ColorOpaq, ColorOpaq, transitionProgress);
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
            Renderers[LayerIndex].sortingOrder = SortingAbove;
            LayerIndex++;
            Renderers[LayerIndex].sortingOrder = SortingBelow;
            //Colliders2d[LayerIndex].enabled = true;
            GlobalLight.intensity = 1 - (0.1f * LayerIndex);

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
            //Renderers[LayerIndex].sortingOrder = SortingUnder;
            LayerIndex--;
            //Colliders2d[LayerIndex].enabled = true;
            GlobalLight.intensity = 1 - (0.1f * LayerIndex);
        }
    }

    public void DoneTransitioning()
    {
        Colliders2d[LayerIndex].enabled = true;
        if (!movingDown)
        {

        }
        else
        {

        }
    }
}
