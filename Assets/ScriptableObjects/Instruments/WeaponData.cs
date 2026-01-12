using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Gun")]
public class WeaponData : ScriptableObject
{
    [Header("Main")]
    public GameObject Prefab;

    [Header("Visuals")]
    public Image Icon;

    [Header("Audio")]
    public AudioClip AudioClip;

}
