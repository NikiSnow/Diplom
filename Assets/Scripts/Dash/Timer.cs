using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] TMP_Text TimerText;
    float NowTime = 0;

    private void Update()
    {
        NowTime = NowTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(NowTime / 60);
        int seconds = Mathf.FloorToInt(NowTime % 60);
        int miliseconds = Mathf.FloorToInt((NowTime % 1) * 100);
        TimerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, miliseconds);

    }
}
