using UnityEngine;

// ќбщие настройки воды дл€ всей игры
[CreateAssetMenu(fileName = "WaterSettings", menuName = "Game/Water Settings")]
public class WaterSettings : ScriptableObject
{
    public float baseMoveSpeed = 6f;      // Ѕазова€ скорость плавани€
    public float baseAcceleration = 12f;  // Ќасколько быстро разгон€емс€
    public float baseDrag = 4f;           // Ќасколько быстро тормозим

    public Color waterColor = new Color(0f, 0.3f, 0.5f, 1f); // ÷вет воды (дл€ шейдеров/поста)
    // TODO: сюда можно добавить глобальные параметры звука/постобработки дл€ глубины
}
