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

    // ─── Historial detallado de preguntas individuales ───
    public List<QuestionRecord> questionHistory = new List<QuestionRecord>();

    // ─── Estadísticas globales ───
    public int totalCorrectAnswers = 0;
    public int totalWrongAnswers = 0;
    public int totalBattlesWon = 0;
    public int totalBattlesLost = 0;

    // ─── Modo Frenesí (NUEVO) ───
    public int frenzyHighscore = 0;
    public int totalFrenzyEnemiesDefeated = 0;
    public int totalFrenzySessions = 0;

    // ─── Tutoriales ───
    public bool battleTutorialCompleted = false;

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

    /// <summary>
    /// Registra el resultado de una sesión de modo frenesí.
    /// </summary>
    public void RecordFrenzySession(int enemiesDefeated)
    {
        totalFrenzySessions++;
        totalFrenzyEnemiesDefeated += enemiesDefeated;
        if (enemiesDefeated > frenzyHighscore)
        {
            frenzyHighscore = enemiesDefeated;
        }
    }

    // ═══════════════════════════════════════════
    //  REGISTRO DE PREGUNTAS INDIVIDUALES
    // ═══════════════════════════════════════════

    /// <summary>
    /// Registra una pregunta individual respondida por el alumno.
    /// </summary>
    public void RecordQuestion(string operation, int numberA, int numberB,
                               int correctAnswer, int playerAnswer,
                               bool wasCorrect, float timeUsed, string levelName)
    {
        var record = new QuestionRecord
        {
            operation = operation,
            numberA = numberA,
            numberB = numberB,
            correctAnswer = correctAnswer,
            playerAnswer = playerAnswer,
            wasCorrect = wasCorrect,
            timeUsed = timeUsed,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            levelName = levelName
        };

        questionHistory.Add(record);
    }

    // ═══════════════════════════════════════════
    //  MÉTODOS DE CONSULTA ESTADÍSTICA
    // ═══════════════════════════════════════════

    /// <summary>
    /// Porcentaje de acierto para una operación específica ("+", "-", "×", "÷").
    /// Retorna -1 si no hay datos para esa operación.
    /// </summary>
    public float GetAccuracyByOperation(string operation)
    {
        int total = 0;
        int correct = 0;

        foreach (var q in questionHistory)
        {
            if (q.operation == operation)
            {
                total++;
                if (q.wasCorrect) correct++;
            }
        }

        if (total == 0) return -1f;
        return (correct / (float)total) * 100f;
    }

    /// <summary>
    /// Porcentaje de acierto global del alumno.
    /// Retorna -1 si no hay datos.
    /// </summary>
    public float GetOverallAccuracy()
    {
        if (questionHistory.Count == 0) return -1f;

        int correct = 0;
        foreach (var q in questionHistory)
        {
            if (q.wasCorrect) correct++;
        }

        return (correct / (float)questionHistory.Count) * 100f;
    }

    /// <summary>
    /// Total de preguntas respondidas.
    /// </summary>
    public int GetTotalQuestionsAnswered()
    {
        return questionHistory.Count;
    }

    /// <summary>
    /// Retorna las últimas N preguntas respondidas.
    /// </summary>
    public List<QuestionRecord> GetRecentQuestions(int count)
    {
        int start = Mathf.Max(0, questionHistory.Count - count);
        int length = Mathf.Min(count, questionHistory.Count);
        return questionHistory.GetRange(start, length);
    }

    /// <summary>
    /// Cantidad de preguntas respondidas de una operación específica.
    /// </summary>
    public int GetQuestionsCountByOperation(string operation)
    {
        int total = 0;
        foreach (var q in questionHistory)
        {
            if (q.operation == operation) total++;
        }
        return total;
    }

    /// <summary>
    /// Tiempo promedio de respuesta en segundos.
    /// Retorna -1 si no hay datos.
    /// </summary>
    public float GetAverageResponseTime()
    {
        if (questionHistory.Count == 0) return -1f;

        float totalTime = 0f;
        foreach (var q in questionHistory)
        {
            totalTime += q.timeUsed;
        }
        return totalTime / questionHistory.Count;
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
