using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Battle/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Visuales")]
    public int enemyLevel = 1;
    public string enemyTitle = "";

    [Header("Identidad")]
    public string enemyName;
    public Sprite sprite;                                // Sprite estático (fallback)
    public RuntimeAnimatorController animatorController; // Animator con idle, hit, etc. (opcional)

    [Header("Stats")]
    public int maxHP = 100;
    public int attackDamage = 15;
    public int defense = 5;

    [Header("Preguntas matemáticas")]
    public bool useAddition = true;
    public bool useSubtraction = true;
    public bool useMultiplication = false;
    public bool useDivision = false;

    [Header("Dificultad de números")]
    public int minNumber = 1;
    public int maxNumber = 20;
    public int questionTimeLimit = 15; // segundos

    [Header("Dificultad de IA")]
    [Range(0.5f, 2.0f)]
    public float damageMultiplier = 1.0f;     // Escala el attackDamage
    [Range(0f, 0.5f)]
    public float criticalChance = 0.0f;       // Probabilidad de golpe crítico
    public int criticalBonusDamage = 10;       // Daño extra en crítico
    [Range(0f, 1f)]
    public float dodgeChance = 0.0f;           // Probabilidad de esquivar ataque del jugador
}