using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    /// <summary>
    /// Calcula el daño del enemigo al jugador, usando los campos de dificultad de IA.
    /// </summary>
    public int CalculateDamage(EnemyData enemy)
    {
        // Variación del ±20% sobre el daño base, escalado por el multiplicador
        float variation = Random.Range(0.8f, 1.2f);
        int baseDamage = Mathf.RoundToInt(enemy.attackDamage * variation * enemy.damageMultiplier);

        // ¿Golpe crítico?
        if (Random.value < enemy.criticalChance)
        {
            baseDamage += enemy.criticalBonusDamage;
            Debug.Log($"¡{enemy.enemyName} asestó un golpe crítico! (+{enemy.criticalBonusDamage})");
        }

        return baseDamage;
    }

    /// <summary>
    /// Determina si el enemigo esquiva el ataque del jugador.
    /// </summary>
    public bool TryDodge(EnemyData enemy)
    {
        bool dodged = Random.value < enemy.dodgeChance;
        if (dodged)
            Debug.Log($"¡{enemy.enemyName} esquivó el ataque!");
        return dodged;
    }
}