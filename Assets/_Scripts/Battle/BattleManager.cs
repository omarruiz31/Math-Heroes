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
        if (GameManager.Instance == null)
        {
            Debug.LogError("BattleManager necesita un GameManager activo en la escena.");
            ui.ShowResult(false);
            return;
        }

        enemyData = GameManager.Instance.currentEnemy;
        if (enemyData == null)
        {
            Debug.LogError("No hay enemigo seleccionado para la batalla.");
            ui.ShowResult(false);
            return;
        }

        enemyCurrentHP = enemyData.maxHP;

        ui.Setup(enemyData, GameManager.Instance.playerMaxHP,
                 GameManager.Instance.playerCurrentHP);
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
        }
        else
        {
            int damage = Random.Range(18, 30); // puedes conectar esto a stats del jugador
            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - damage);
            ui.UpdateEnemyHP(enemyCurrentHP, enemyData.maxHP);
            ui.PlayEnemyHitAnim();
        }

        yield return new WaitForSeconds(1.2f);

        if (enemyCurrentHP <= 0) { EndBattle(true); yield break; }
        StartCoroutine(NextTurn());
    }

    IEnumerator EnemyAttackTurn()
    {
        yield return new WaitForSeconds(0.8f);

        int damage = enemyAI.CalculateDamage(enemyData);
        GameManager.Instance.playerCurrentHP =
            Mathf.Max(0, GameManager.Instance.playerCurrentHP - damage);

        ui.UpdatePlayerHP(GameManager.Instance.playerCurrentHP,
                          GameManager.Instance.playerMaxHP);
        ui.PlayPlayerHitAnim();

        yield return new WaitForSeconds(1.2f);

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
