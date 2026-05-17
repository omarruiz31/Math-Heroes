using UnityEngine;

[CreateAssetMenu(fileName = "NewEnvironment", menuName = "Battle/Environment Data")]
public class BattleEnvironmentData : ScriptableObject
{
    [Header("Identidad")]
    public string environmentName;         // "Bosque", "Cueva", "Desierto"

    [Header("Fondo Visual")]
    public GameObject tilemapPrefab;        // Prefab con el Grid+Tilemap ya pintado
    public Color ambientColor = Color.white; // Color de iluminación ambiental

    [Header("Audio")]
    public AudioClip battleMusic;           // Música específica de este ambiente
}
