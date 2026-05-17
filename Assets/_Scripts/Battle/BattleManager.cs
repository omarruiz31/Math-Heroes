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
        waitingForAnswer = true;
    }

    // Llamado por BattleUI cuando el jugador envía su respuesta
    public void SubmitAnswer(string input)
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;
        ui.StopTimer();

        if (int.TryParse(input, out int playerAnswer) &&
            playerAnswer == currentQuestion.correctAnswer)
        {
            StartCoroutine(PlayerAttack());
        }
        else
        {
            ui.ShowFeedback(false, currentQuestion.correctAnswer, currentQuestion.explanation);
            StartCoroutine(EnemyAttackTurn());
        }
    }

    // El timer se agotó sin respuesta
    public void TimeOut()
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;
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
        ui.ShowResult(playerWon);
    }
}
