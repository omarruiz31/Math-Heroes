using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public int CalculateDamage(EnemyData enemy)
    {
        // Variación del ±20% sobre el daño base
        float variation = Random.Range(0.8f, 1.2f);
        return Mathf.RoundToInt(enemy.attackDamage * variation);
    }
}