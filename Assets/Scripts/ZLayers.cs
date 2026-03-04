using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

public class ZLayers : MonoBehaviour
{
    [Header("Layers")]

    [SerializeField] List<Transform> LayerTrans;
    [SerializeField] List<GameObject> Prefabs;
    [SerializeField] List<SpriteRenderer> LayerSprite;

    [Header("Scale")]
    [SerializeField] float SizeEndToMonitor = 2f;
    [SerializeField] float SizePlayerLayer = 1;
    [SerializeField] float SizeBehindLayer = 0.5f;

    [Header("PosY")]
    [SerializeField] float YOut = 5.7f;
    [SerializeField] float YPlayer = 0.3f;
    [SerializeField] float YBehind = -2f;

    [Header("Other")]
    [SerializeField] float transitionDuration = 0.35f;


    float transitionProgress = 0f;
    int LayerIndex = 0;
    bool isTransitioning;
    Color ColorTrans = new Color(1, 1, 1, 0);
    Color ColorOpaq = new Color(1, 1, 1, 1);
    Color ColorHalfDark = new Color(0.5f, 0.5f, 0.5f, 1);
    bool IsToMonitor;



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isTransitioning) //Down
        {
            StartTransitionUnder(true);
        }

        if (Input.GetKeyDown(KeyCode.E) && !isTransitioning) //Up
        {
            StartTransitionUnder(false);
        }

        if (isTransitioning)
        {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;
            }

            if (IsToMonitor)
            {
                float CurrScale = Mathf.Lerp(SizeEndToMonitor, SizePlayerLayer, transitionProgress);
                float CurrY = Mathf.Lerp(YOut, YPlayer, transitionProgress);
                LayerTrans[0].localScale = new Vector3(CurrScale, CurrScale, 1);
                LayerTrans[0].position = new Vector3(0, CurrY, 0);
                LayerSprite[0].color = Color.Lerp(ColorTrans, ColorOpaq, transitionProgress);

                float CurrScale2 = Mathf.Lerp(SizePlayerLayer, SizeBehindLayer, transitionProgress);
                float CurrY2 = Mathf.Lerp(YPlayer, YBehind, transitionProgress);
                LayerTrans[1].localScale = new Vector3(CurrScale2, CurrScale2, 1);
                LayerTrans[1].position = new Vector3(0, CurrY2, 0);
                LayerSprite[1].color = Color.Lerp(ColorOpaq, ColorHalfDark, transitionProgress);
            }
            else
            {
                float CurrScale = Mathf.Lerp(SizePlayerLayer, SizeEndToMonitor, transitionProgress);
                float CurrY = Mathf.Lerp(YPlayer, YOut, transitionProgress);
                LayerTrans[0].localScale = new Vector3(CurrScale, CurrScale, 1);
                LayerTrans[0].position = new Vector3(0, CurrY, 0);
                LayerSprite[0].color = Color.Lerp(ColorOpaq, ColorTrans, transitionProgress);

                float CurrScale2 = Mathf.Lerp(SizeBehindLayer, SizePlayerLayer, transitionProgress);
                float CurrY2 = Mathf.Lerp(YBehind, YPlayer, transitionProgress);
                LayerTrans[1].localScale = new Vector3(CurrScale2, CurrScale2, 1);
                LayerTrans[1].position = new Vector3(0, CurrY2, 0);
                LayerSprite[1].color = Color.Lerp(ColorHalfDark, ColorOpaq, transitionProgress);
            }
        }
    }

    void StartTransitionUnder(bool ToMonitor)
    {
        isTransitioning = true;
        transitionProgress = 0f;
        IsToMonitor = ToMonitor;
    }
}
