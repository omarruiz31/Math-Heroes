using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Referencias")]
    public BattleUI ui;
    public QuestionGenerator questionGenerator;
    public EnemyAI enemyAI;

    // Estado interno
    private EnemyData enemyData;
    private int enemyCurrentHP;
    private QuestionGenerator.Question currentQuestion;
    private bool waitingForAnswer = false;
    private float questionStartTime;  // Tiempo en que se mostró la pregunta actual

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Debug.Log($"[BattleManager] Starting battle initialization. GameManager: {(GameManager.Instance != null ? "Found" : "NULL")}");

        if (GameManager.Instance == null)
        {
            Debug.LogError("BattleManager: No se encontró el GameManager. Asegúrate de iniciar el juego desde el Menú.");
            // No mostramos el resultado de inmediato para evitar pantalla negra sin feedback
            if (ui != null) ui.feedbackText.text = "Error: GameManager no encontrado";
            return;
        }

        enemyData = GameManager.Instance.currentEnemy;
        Debug.Log($"[BattleManager] Current Enemy: {(enemyData != null ? enemyData.enemyName : "NULL")}");

        if (enemyData == null)
        {
            Debug.LogError("BattleManager: No hay enemigo seleccionado para la batalla.");
            if (ui != null) ui.feedbackText.text = "Error: No hay enemigo seleccionado";
            // ui.ShowResult(false); // Evitamos mostrar el panel de derrota inmediatamente
            return;
        }

        enemyCurrentHP = enemyData.maxHP;

        if (ui == null)
        {
            Debug.LogError("BattleManager: Referencia a BattleUI es NULL.");
            return;
        }

        ui.Setup(enemyData, GameManager.Instance.playerMaxHP,
                 GameManager.Instance.playerCurrentHP);

        // Tutorial logic (RF 14)
        if (GameManager.Instance.playerData != null && !GameManager.Instance.playerData.battleTutorialCompleted)
        {
            var tutorial = GetComponent<BattleTutorial>();
            if (tutorial != null)
            {
                tutorial.StartTutorial(ui);
                return;
            }
        }

        StartCoroutine(NextTurn());
    }

    /// <summary>
    /// Llamado desde BattleTutorial al terminar.
    /// </summary>
    public void StartBattleAfterTutorial()
    {
        StartCoroutine(NextTurn());
    }

    IEnumerator NextTurn()
    {
        yield return new WaitForSeconds(0.2f);

        currentQuestion = questionGenerator.GenerateQuestion(enemyData);
        ui.ShowQuestion(currentQuestion.text, enemyData.questionTimeLimit);
        questionStartTime = Time.time;
        waitingForAnswer = true;
    }

    // Llamado por BattleUI cuando el jugador envía su respuesta
    public void SubmitAnswer(string input)
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;
        ui.StopTimer();

        float timeUsed = Time.time - questionStartTime;
        int parsedAnswer = 0;
        bool parsed = int.TryParse(input, out parsedAnswer);
        bool isCorrect = parsed && parsedAnswer == currentQuestion.correctAnswer;

        // Registrar la pregunta en el historial detallado
        string enemyName = enemyData != null ? enemyData.enemyName : "Desconocido";
        GameManager.Instance.playerData.RecordQuestion(
            currentQuestion.operation,
            currentQuestion.numberA,
            currentQuestion.numberB,
            currentQuestion.correctAnswer,
            parsed ? parsedAnswer : -999,
            isCorrect,
            timeUsed,
            enemyName
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

    // El timer se agotó sin respuesta
    public void TimeOut()
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;

        float timeUsed = Time.time - questionStartTime;

        // Registrar timeout como respuesta incorrecta
        string enemyName = enemyData != null ? enemyData.enemyName : "Desconocido";
        GameManager.Instance.playerData.RecordQuestion(
            currentQuestion.operation,
            currentQuestion.numberA,
            currentQuestion.numberB,
            currentQuestion.correctAnswer,
            -999,  // No respondió
            false,
            timeUsed,
            enemyName
        );

        GameManager.Instance.battleWrongAnswers++;
        ui.ShowFeedback(false, currentQuestion.correctAnswer, currentQuestion.explanation);
        StartCoroutine(EnemyAttackTurn());
    }

    IEnumerator PlayerAttack()
    {
        ui.ShowFeedback(true, currentQuestion.correctAnswer, currentQuestion.explanation);

        // ¿El enemigo esquiva?
        if (enemyAI.TryDodge(enemyData))
        {
            ui.ShowFeedback(true, currentQuestion.correctAnswer,
                "¡Respuesta correcta, pero el enemigo esquivó tu ataque!");
            ui.TriggerPlayerAttack(ui.enemyRenderer.transform.position, false);
        }
        else
        {
            int damage = Random.Range(18, 30); // puedes conectar esto a stats del jugador
            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - damage);
            ui.UpdateEnemyHP(enemyCurrentHP, enemyData.maxHP);
            ui.TriggerPlayerAttack(ui.enemyRenderer.transform.position, true);
        }

        yield return new WaitForSeconds(0.7f);

        if (enemyCurrentHP <= 0) { EndBattle(true); yield break; }
        StartCoroutine(NextTurn());
    }

    IEnumerator EnemyAttackTurn()
    {
        yield return new WaitForSeconds(0.4f);

        int damage = enemyAI.CalculateDamage(enemyData);
        GameManager.Instance.playerCurrentHP =
            Mathf.Max(0, GameManager.Instance.playerCurrentHP - damage);

        ui.UpdatePlayerHP(GameManager.Instance.playerCurrentHP,
                          GameManager.Instance.playerMaxHP);
        ui.TriggerEnemyAttack(true);

        yield return new WaitForSeconds(0.8f);

        if (GameManager.Instance.playerCurrentHP <= 0) { EndBattle(false); yield break; }
        StartCoroutine(NextTurn());
    }

    void EndBattle(bool playerWon)
    {
        string enemyName = enemyData != null ? enemyData.enemyName : "Desconocido";

        if (playerWon)
            GameManager.Instance.OnBattleWon(enemyName);
        else
            GameManager.Instance.OnBattleLost(enemyName);

        ui.ShowResult(playerWon);
    }
}
