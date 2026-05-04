using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("Enemigo")]
    public Image enemySprite;
    public Slider enemyHPBar;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyHPText;

    [Header("Jugador")]
    public Slider playerHPBar;
    public TextMeshProUGUI playerHPText;

    [Header("Pregunta")]
    public TextMeshProUGUI questionText;
    public TMP_InputField answerInput;
    public Button submitButton;
    public TextMeshProUGUI timerText;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText; // "¡Correcto!" / "¡Incorrecto!"
    public TextMeshProUGUI correctAnswerText;

    [Header("Resultado final")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button returnButton;

    [Header("Animaciones")]
    public Animator enemyAnimator;
    public Animator playerAnimator;

    private Coroutine timerCoroutine;

    public void Setup(EnemyData enemy, int playerMaxHP, int playerHP)
    {
        enemySprite.sprite  = enemy.sprite;
        enemyNameText.text  = enemy.enemyName;
        enemyHPBar.maxValue = enemy.maxHP;
        enemyHPBar.value    = enemy.maxHP;
        enemyHPText.text    = $"{enemy.maxHP} / {enemy.maxHP}";

        playerHPBar.maxValue = playerMaxHP;
        playerHPBar.value    = playerHP;
        playerHPText.text    = $"{playerHP} / {playerMaxHP}";

        resultPanel.SetActive(false);
        feedbackText.text      = "";
        correctAnswerText.text = "";

        submitButton.onClick.AddListener(() =>
            BattleManager.Instance.SubmitAnswer(answerInput.text));

        // Enter también envía la respuesta
        answerInput.onSubmit.AddListener(val =>
            BattleManager.Instance.SubmitAnswer(val));
    }

    public void ShowQuestion(string question, int timeLimit)
    {
        questionText.text  = question;
        answerInput.text   = "";
        feedbackText.text  = "";
        correctAnswerText.text = "";
        answerInput.interactable = true;
        answerInput.ActivateInputField();

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunTimer(timeLimit));
    }

    IEnumerator RunTimer(int seconds)
    {
        int remaining = seconds;
        while (remaining > 0)
        {
            timerText.text = remaining.ToString();
            timerText.color = remaining <= 5 ? Color.red : Color.white;
            yield return new WaitForSeconds(1f);
            remaining--;
        }
        timerText.text = "0";
        answerInput.interactable = false;
        BattleManager.Instance.TimeOut();
    }

    public void StopTimer()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        answerInput.interactable = false;
    }

    public void ShowFeedback(bool correct, int correctAnswer)
    {
        feedbackText.text  = correct ? "¡Correcto! ✓" : "¡Incorrecto! ✗";
        feedbackText.color = correct ? Color.green : Color.red;
        if (!correct)
            correctAnswerText.text = $"La respuesta era: {correctAnswer}";
    }

    public void UpdateEnemyHP(int current, int max)
    {
        enemyHPBar.value  = current;
        enemyHPText.text  = $"{current} / {max}";
    }

    public void UpdatePlayerHP(int current, int max)
    {
        playerHPBar.value = current;
        playerHPText.text = $"{current} / {max}";
    }

    public void PlayEnemyHitAnim()
    {
        if (enemyAnimator) enemyAnimator.SetTrigger("Hit");
    }

    public void PlayPlayerHitAnim()
    {
        if (playerAnimator) playerAnimator.SetTrigger("Hit");
    }

    public void ShowResult(bool playerWon)
    {
        resultPanel.SetActive(true);
        resultText.text = playerWon ? "¡Victoria! " : "Derrota... :(";
        returnButton.onClick.AddListener(() => GameManager.Instance.ReturnToMap());
    }
}