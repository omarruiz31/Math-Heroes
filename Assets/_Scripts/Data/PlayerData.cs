using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Datos persistentes del jugador. Se serializa a JSON para guardado/carga.
/// Esta clase NO es un MonoBehaviour ni ScriptableObject — es un objeto C# puro
/// para que JsonUtility pueda serializarlo fácilmente.
/// </summary>
[Serializable]
public class PlayerData
{
    // ─── Identidad ───
    public string playerName = "";

    // ─── Progresión ───
    public int level = 1;
    public int currentXP = 0;
    public int maxHP = 100;

    // ─── Zonas desbloqueadas ───
    // Lista de nombres de zonas que el jugador ha desbloqueado.
    // La primera zona siempre está desbloqueada por defecto.
    public List<string> unlockedZones = new List<string>() { "Zona1_Suma" };

    // ─── Historial de desempeño (RF 10) ───
    public List<BattleRecord> battleHistory = new List<BattleRecord>();

    // ─── Estadísticas globales ───
    public int totalCorrectAnswers = 0;
    public int totalWrongAnswers = 0;
    public int totalBattlesWon = 0;
    public int totalBattlesLost = 0;

    // ═══════════════════════════════════════════
    //  MÉTODOS DE PROGRESIÓN (RF 05, 11, 13)
    // ═══════════════════════════════════════════

    /// <summary>
    /// XP necesario para pasar al siguiente nivel.
    /// Fórmula: 50 + (nivel actual * 30)
    /// Nivel 1 → 80 XP | Nivel 2 → 110 XP | Nivel 5 → 200 XP
    /// </summary>
    public int XPToNextLevel()
    {
        return 50 + (level * 30);
    }

    /// <summary>
    /// Agrega XP y sube de nivel si se alcanza el umbral.
    /// Retorna true si el jugador subió de nivel.
    /// </summary>
    public bool AddXP(int amount)
    {
        currentXP += amount;
        bool leveledUp = false;

        while (currentXP >= XPToNextLevel())
        {
            currentXP -= XPToNextLevel();
            level++;
            maxHP += 10; // +10 HP por nivel
            leveledUp = true;
        }

        return leveledUp;
    }

    /// <summary>
    /// Rango máximo de números que el jugador puede resolver,
    /// escalado por su nivel (RF 13).
    /// Nivel 1 → 20 | Nivel 3 → 50 | Nivel 5 → 80 | Nivel 10 → 155
    /// </summary>
    public int MaxNumberForLevel()
    {
        return 20 + (level - 1) * 15;
    }

    /// <summary>
    /// Desbloquea una nueva zona si no estaba ya desbloqueada.
    /// </summary>
    public void UnlockZone(string zoneName)
    {
        if (!unlockedZones.Contains(zoneName))
            unlockedZones.Add(zoneName);
    }

    /// <summary>
    /// Verifica si una zona está desbloqueada.
    /// </summary>
    public bool IsZoneUnlocked(string zoneName)
    {
        return unlockedZones.Contains(zoneName);
    }

    /// <summary>
    /// Registra el resultado de una batalla en el historial (RF 10).
    /// </summary>
    public void RecordBattle(string enemyName, bool won, int correctAnswers,
                             int wrongAnswers, float timeSeconds)
    {
        var record = new BattleRecord
        {
            enemyName = enemyName,
            won = won,
            correctAnswers = correctAnswers,
            wrongAnswers = wrongAnswers,
            timeSeconds = timeSeconds,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        battleHistory.Add(record);

        // Actualizar estadísticas globales
        totalCorrectAnswers += correctAnswers;
        totalWrongAnswers += wrongAnswers;
        if (won) totalBattlesWon++;
        else totalBattlesLost++;
    }
}

/// <summary>
/// Registro individual de una batalla para el historial (RF 10, 22).
/// </summary>
[Serializable]
public class BattleRecord
{
    public string enemyName;
    public bool won;
    public int correctAnswers;
    public int wrongAnswers;
    public float timeSeconds;
    public string date;
}
