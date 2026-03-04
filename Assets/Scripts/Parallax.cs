using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] Transform Cam;
    [SerializeField] List<Transform> Layers;
    [SerializeField] List<float> OffsetsX;
    [SerializeField] List<float> OffsetsY;
    [SerializeField] List<float> StartPosX;
    [SerializeField] List<float> StartPosY;
    float DistX;
    float DistY;
    private void Update()
    {
        for (int i = 1; i < Layers.Count; i++)
        {
            DistX = (Cam.position.x * (1 - OffsetsX[i]));
            DistY = (Cam.position.y * (1 - OffsetsY[i]));
            Layers[i].position = new Vector3(StartPosX[i] + DistX, StartPosY[i] + DistY, 0f);
        }
    }
}
