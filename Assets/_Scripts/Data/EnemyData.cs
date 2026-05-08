using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Battle/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identidad")]
    public string enemyName;
    public Sprite sprite;

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
}