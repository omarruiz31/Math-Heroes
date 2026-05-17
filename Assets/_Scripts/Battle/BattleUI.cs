using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("Enemigo")]
    public SpriteRenderer enemyRenderer;
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
    public Button exitButton;

    [Header("Animaciones")]
    public Animator enemyAnimator;
    public Animator playerAnimator;

    [Header("Sonidos")]
    public AudioSource sfxSource;
    public AudioClip playerHitSound;
    public AudioClip enemyHitSound;

    private Coroutine timerCoroutine;

    public void Setup(EnemyData enemy, int playerMaxHP, int playerHP)
    {
        enemyNameText.text  = enemy.enemyName;
        enemyHPBar.maxValue = enemy.maxHP;
        enemyHPBar.value    = enemy.maxHP;
        enemyHPText.text    = $"{enemy.maxHP} / {enemy.maxHP}";

        // Configurar el sprite del enemigo
        if (enemyRenderer != null)
            enemyRenderer.sprite = enemy.sprite;

        // Si el enemigo tiene animator controller, asignarlo
        if (enemy.animatorController != null && enemyAnimator != null)
        {
            enemyAnimator.runtimeAnimatorController = enemy.animatorController;
            enemyAnimator.enabled = true;
        }

        playerHPBar.maxValue = playerMaxHP;
        playerHPBar.value    = playerHP;
        playerHPText.text    = $"{playerHP} / {playerMaxHP}";

        resultPanel.SetActive(false);
        feedbackText.text      = "";
        correctAnswerText.text = "";

        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(() =>
            BattleManager.Instance.SubmitAnswer(answerInput.text));

        // Enter también envía la respuesta
        answerInput.onSubmit.RemoveAllListeners();
        answerInput.onSubmit.AddListener(val =>
            BattleManager.Instance.SubmitAnswer(val));

        returnButton.onClick.RemoveAllListeners();
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ReturnToMap);
        }
    }

    public void ShowQuestion(string question, int timeLimit)
    {
        questionText.text  = question;
        answerInput.text   = "";
        feedbackText.text  = "";
        correctAnswerText.text = "";
        answerInput.interactable = true;
        submitButton.interactable = true;
        answerInput.ActivateInputField();

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunTimer(timeLimit));
    }

    IEnumerator RunTimer(int seconds)
    {
        int remaining = seconds;
        while (remaining > 0)
        {
            timerText.text = remaining.ToString() + "s";
            timerText.color = remaining <= 5 ? Color.red : Color.white;
            yield return new WaitForSeconds(1f);
            remaining--;
        }
        timerText.text = "0";
        answerInput.interactable = false;
        submitButton.interactable = false;
        BattleManager.Instance.TimeOut();
    }

    public void StopTimer()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        answerInput.interactable = false;
        submitButton.interactable = false;
    }

    public void ShowFeedback(bool correct, int correctAnswer, string explanation)
    {
        feedbackText.text  = correct ? "¡Correcto! ✓" : "¡Incorrecto! ✗";
        feedbackText.color = correct ? Color.green : Color.red;
        correctAnswerText.text = correct
            ? explanation
            : $"La respuesta era: {correctAnswer}\n{explanation}";
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
        if (sfxSource && enemyHitSound) sfxSource.PlayOneShot(enemyHitSound);
    }

    public void PlayPlayerHitAnim()
    {
        if (playerAnimator) playerAnimator.SetTrigger("Hit");
        if (sfxSource && playerHitSound) sfxSource.PlayOneShot(playerHitSound);
    }

    public void ShowResult(bool playerWon)
    {
        resultPanel.SetActive(true);
        resultText.text = playerWon ? "¡Victoria! " : "Derrota... :(";
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(ReturnToMap);
    }

    private void ReturnToMap()
    {
        StopTimer();
        GameManager.Instance.ReturnToMap();
    }
}
