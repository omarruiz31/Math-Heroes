using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrenzyBattleManager : MonoBehaviour
{
    public static FrenzyBattleManager Instance { get; private set; }

    [Header("Referencias")]
    public BattleUI ui;
    public QuestionGenerator questionGenerator;
    public EnemyAI enemyAI;

    [Header("Environment")]
    public BattleEnvironmentData frenzyEnvironment;
    public Transform environmentParent;

    [Header("Frenzy Config")]
public List<EnemyData> enemyPool;
    public int enemiesDefeated = 0;

    // Estado interno
    private EnemyData enemyData;
    private int enemyCurrentHP;
    private QuestionGenerator.Question currentQuestion;
    private bool waitingForAnswer = false;
    private float questionStartTime;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("FrenzyBattleManager necesita un GameManager activo.");
            return;
        }

        SetupEnvironment();
        SpawnNewEnemy();
    }

    void SetupEnvironment()
    {
        if (frenzyEnvironment != null && environmentParent != null)
        {
            Instantiate(frenzyEnvironment.tilemapPrefab, environmentParent);
            if (Camera.main != null)
                Camera.main.backgroundColor = frenzyEnvironment.ambientColor;
        }
    }

    void SpawnNewEnemy()
    {
        if (enemyPool == null || enemyPool.Count == 0)
        {
            // Fallback to current enemy if pool is empty
            enemyData = GameManager.Instance.currentEnemy;
        }
        else
        {
            enemyData = enemyPool[Random.Range(0, enemyPool.Count)];
        }

        if (enemyData == null)
        {
            Debug.LogError("No hay datos de enemigo para Frenzy Mode.");
            return;
        }

        enemyCurrentHP = enemyData.maxHP;
        ui.Setup(enemyData, GameManager.Instance.playerMaxHP, GameManager.Instance.playerCurrentHP);
        
        // Update a UI text if possible or just log
        Debug.Log("Frenzy Mode: Enemy Spawned - " + enemyData.enemyName);
        
        StartCoroutine(NextTurn());
    }

    IEnumerator NextTurn()
    {
        yield return new WaitForSeconds(0.5f);
        currentQuestion = questionGenerator.GenerateQuestion(enemyData);
        ui.ShowQuestion(currentQuestion.text, enemyData.questionTimeLimit);
        questionStartTime = Time.time;
        waitingForAnswer = true;
    }

    public void SubmitAnswer(string input)
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;
        ui.StopTimer();

        float timeUsed = Time.time - questionStartTime;
        int parsedAnswer = 0;
        bool parsed = int.TryParse(input, out parsedAnswer);
        bool isCorrect = parsed && parsedAnswer == currentQuestion.correctAnswer;

        // Stats recording
        string enemyName = enemyData != null ? enemyData.enemyName : "FrenzyEnemy";
        GameManager.Instance.playerData.RecordQuestion(
            currentQuestion.operation, currentQuestion.numberA, currentQuestion.numberB,
            currentQuestion.correctAnswer, parsed ? parsedAnswer : -999, isCorrect, timeUsed, enemyName
        );

        if (isCorrect)
        {
            GameManager.Instance.battleCorrectAnswers++;
            StartCoroutine(PlayerAttack());
        }
        else
        {
            GameManager.Instance.battleWrongAnswers++;
            ui.ShowFeedback(false, currentQuestion.correctAnswer, currentQuestion.explanation);
            StartCoroutine(EnemyAttackTurn());
        }
    }

    public void TimeOut()
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;
        
        float timeUsed = Time.time - questionStartTime;
        GameManager.Instance.battleWrongAnswers++;
        ui.ShowFeedback(false, currentQuestion.correctAnswer, currentQuestion.explanation);
        StartCoroutine(EnemyAttackTurn());
    }

    IEnumerator PlayerAttack()
    {
        ui.ShowFeedback(true, currentQuestion.correctAnswer, currentQuestion.explanation);
        
        if (enemyAI.TryDodge(enemyData))
        {
            ui.TriggerPlayerAttack(ui.enemyRenderer.transform.position, false);
        }
        else
        {
            int damage = Random.Range(20, 35); 
            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - damage);
            ui.UpdateEnemyHP(enemyCurrentHP, enemyData.maxHP);
            ui.TriggerPlayerAttack(ui.enemyRenderer.transform.position, true);
        }

        yield return new WaitForSeconds(0.7f);

        if (enemyCurrentHP <= 0)
        {
            enemiesDefeated++;
            // Heal player slightly for streak
            GameManager.Instance.playerCurrentHP = Mathf.Min(GameManager.Instance.playerMaxHP, GameManager.Instance.playerCurrentHP + 10);
            ui.UpdatePlayerHP(GameManager.Instance.playerCurrentHP, GameManager.Instance.playerMaxHP);
            
            yield return new WaitForSeconds(1f);
            SpawnNewEnemy();
        }
        else
        {
            StartCoroutine(NextTurn());
        }
    }

    IEnumerator EnemyAttackTurn()
    {
        yield return new WaitForSeconds(0.4f);
        int damage = enemyAI.CalculateDamage(enemyData);
        GameManager.Instance.playerCurrentHP = Mathf.Max(0, GameManager.Instance.playerCurrentHP - damage);
        ui.UpdatePlayerHP(GameManager.Instance.playerCurrentHP, GameManager.Instance.playerMaxHP);
        ui.TriggerEnemyAttack(true);

        yield return new WaitForSeconds(0.8f);

        if (GameManager.Instance.playerCurrentHP <= 0)
        {
            EndFrenzy();
        }
        else
        {
            StartCoroutine(NextTurn());
        }
    }

    private bool sessionRecorded = false;

    void EndFrenzy()
    {
        if (sessionRecorded) return;

        // Guardar estadísticas del modo frenesí
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.RecordFrenzySession(enemiesDefeated);
            SaveSystem.Save(GameManager.Instance.playerData);
            GameManager.Instance.HealPlayerToFull(); // Restaurar vida al terminar
        }

        sessionRecorded = true;

        // Record the session as a lost battle for the history (so it shows up in general history too)
        GameManager.Instance.OnBattleLost("Frenzy Mode"); 
        ui.ShowResult(false);
    }

    public void ManualExit()
    {
        // Guardar antes de salir si no se ha guardado ya (por ejemplo al morir)
        if (!sessionRecorded && GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.RecordFrenzySession(enemiesDefeated);
            SaveSystem.Save(GameManager.Instance.playerData);
            GameManager.Instance.HealPlayerToFull(); // Restaurar vida al salir
            sessionRecorded = true;
        }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("menu");
    }
}
