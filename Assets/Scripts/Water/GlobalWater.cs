using UnityEngine;

// —инглтон дл€ доступа к настройкам воды
public class GlobalWater : MonoBehaviour
{
    public static GlobalWater Instance { get; private set; }

    [Header("√лобальные настройки воды")]
    public WaterSettings settings;

    void Awake()
    {
        // √арантируем один экземпл€р на сцену
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
