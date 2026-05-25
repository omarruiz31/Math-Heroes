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

    [Header("Resultado final (Modal Estático)")]
    public Image modalBorderImage;
    public Image titleBarImage;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultSubtitleText;
    public TextMeshProUGUI correctCountText;
    public TextMeshProUGUI incorrectCountText;
    public TextMeshProUGUI accuracyText;
    public Image accuracyBarFill;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI xpGainedText;
    public TextMeshProUGUI levelText;
    public Button retryButton;

    [Header("Animaciones")]
    public Animator enemyAnimator;
    public Animator playerAnimator;

    [Header("Sonidos")]
    public AudioSource sfxSource;
    public AudioClip playerHitSound;
    public AudioClip enemyHitSound;
    public AudioClip correctAnswerSound;

    [Header("Efectos Visuales")]
    public Sprite projectileSprite;
    public Sprite explosionSprite;

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

        // Configurar animación idle del jugador mirando hacia la derecha (Horizontal = 1)
        if (playerAnimator != null && playerAnimator.isActiveAndEnabled)
        {
            playerAnimator.SetFloat("Horizontal", 1f);
            playerAnimator.SetFloat("Vertical", 0f);
            playerAnimator.SetFloat("Speed", 0f);
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

        if (correct)
        {
            if (sfxSource && correctAnswerSound)
                sfxSource.PlayOneShot(correctAnswerSound);
        }
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
        StartCoroutine(FlashRedEnemy());
    }

    private IEnumerator FlashRedEnemy()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            enemyRenderer.color = Color.white;
        }
    }

    public void PlayPlayerHitAnim()
    {
        if (playerAnimator) playerAnimator.SetTrigger("Hit");
        if (sfxSource && playerHitSound) sfxSource.PlayOneShot(playerHitSound);
        StartCoroutine(FlashRedPlayer());
    }

    private IEnumerator FlashRedPlayer()
    {
        if (playerAnimator != null)
        {
            Image playerImage = playerAnimator.GetComponent<Image>();
            if (playerImage != null)
            {
                playerImage.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                playerImage.color = Color.white;
            }
        }
    }

    public void ShowResult(bool playerWon)
    {
        resultPanel.SetActive(true);

        var gm = GameManager.Instance;
        float battleTime = Time.time - gm.battleStartTime;
        int totalQuestions = gm.battleCorrectAnswers + gm.battleWrongAnswers;
        float accuracy = totalQuestions > 0
            ? (gm.battleCorrectAnswers / (float)totalQuestions) * 100f
            : 0f;
        int xpGained = playerWon ? 20 + (gm.battleCorrectAnswers * 5) : 0;

        // Cambiar colores del Modal y TitleBar
        if (modalBorderImage != null)
        {
            modalBorderImage.color = playerWon
                ? new Color(0.3f, 0.75f, 0.45f, 1f)
                : new Color(0.85f, 0.3f, 0.3f, 1f);
        }
        if (titleBarImage != null)
        {
            titleBarImage.color = playerWon
                ? new Color(0.15f, 0.5f, 0.3f, 1f)
                : new Color(0.6f, 0.15f, 0.15f, 1f);
        }

        // Título y Subtítulo
        if (resultTitleText != null)
        {
            resultTitleText.text = playerWon ? "VICTORIA!" : "DERROTA";
        }
        if (resultSubtitleText != null)
        {
            string enemyName = gm.currentEnemy != null ? gm.currentEnemy.enemyName : "Enemigo";
            resultSubtitleText.text = $"vs  {enemyName}";
        }

        // Estadísticas
        if (correctCountText != null)
        {
            correctCountText.text = gm.battleCorrectAnswers.ToString();
        }
        if (incorrectCountText != null)
        {
            incorrectCountText.text = gm.battleWrongAnswers.ToString();
        }
        if (accuracyText != null)
        {
            accuracyText.text = $"{accuracy:F0}%";
            accuracyText.color = GetAccuracyColorRGB(accuracy);
        }
        if (accuracyBarFill != null)
        {
            accuracyBarFill.color = GetAccuracyColorRGB(accuracy);
            RectTransform fillRT = accuracyBarFill.rectTransform;
            if (fillRT != null)
            {
                fillRT.anchorMax = new Vector2(Mathf.Clamp01(accuracy / 100f), 1f);
            }
        }

        int mins = Mathf.FloorToInt(battleTime / 60f);
        int secs = Mathf.FloorToInt(battleTime % 60f);
        string timeStr = mins > 0 ? $"{mins}m {secs}s" : $"{secs}s";
        if (timeText != null)
        {
            timeText.text = timeStr;
        }

        // Info extra (XP y Nivel o Motivación)
        if (playerWon)
        {
            if (xpGainedText != null)
            {
                xpGainedText.text = $"+{xpGained} XP";
                xpGainedText.color = new Color(1f, 0.84f, 0f);
            }
            if (levelText != null)
            {
                levelText.text = gm.playerData != null ? $"Nivel actual: {gm.playerData.level}" : "";
                levelText.color = new Color(0.7f, 0.7f, 0.8f);
            }
        }
        else
        {
            if (xpGainedText != null)
            {
                xpGainedText.text = "No te rindas, ¡inténtalo de nuevo!";
                xpGainedText.color = new Color(1f, 0.65f, 0.15f);
            }
            if (levelText != null)
            {
                levelText.text = "";
            }
        }

        // Botones
        if (returnButton != null)
        {
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(() => ReturnToMap());
        }
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(() => RetryBattle());
        }
    }

    private Color GetAccuracyColorRGB(float accuracy)
    {
        if (accuracy >= 80f) return new Color(0.4f, 0.73f, 0.42f);
        if (accuracy >= 60f) return new Color(1f, 0.65f, 0.15f);
        return new Color(0.94f, 0.33f, 0.31f);
    }

    private void ReturnToMap()
    {
        StopTimer();
        GameManager.Instance.ReturnToMap();
    }

    private void RetryBattle()
    {
        StopTimer();
        GameManager.Instance.RetryBattle();
    }

    // Método para disparar la animación de ataque (proyectil y explosión)
    public void TriggerPlayerAttack(Vector3 enemyWorldPos, bool isHit)
    {
        if (playerAnimator == null || enemyRenderer == null) return;

        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) canvas = playerAnimator.GetComponentInParent<Canvas>();

        Vector2 startPos = ((RectTransform)playerAnimator.transform).anchoredPosition;
        // Ajustar punto de salida para que salga aproximadamente desde la mano del personaje (mirando a la derecha)
        startPos += new Vector2(65f, -10f);
        Vector2 endPos = Vector2.zero;

        if (canvas != null)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(enemyWorldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                screenPos,
                canvas.worldCamera,
                out endPos
            );
        }
        else
        {
            endPos = new Vector2(500f, 0f);
        }

        StartCoroutine(AnimateProjectile(startPos, endPos, isHit));
    }

    private IEnumerator AnimateProjectile(Vector2 startPos, Vector2 endPos, bool isHit)
    {
        Sprite spriteToUse = projectileSprite;
        if (spriteToUse == null)
        {
#if UNITY_EDITOR
            spriteToUse = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SunnyLand Artwork/Sprites/Items/gem/gem-1.png");
#endif
        }

        if (spriteToUse == null) yield break;

        GameObject proj = new GameObject("Projectile");
        proj.transform.SetParent(playerAnimator.transform.parent, false);

        Image img = proj.AddComponent<Image>();
        img.sprite = spriteToUse;
        img.preserveAspect = true;

        RectTransform rect = proj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120f, 120f);
        rect.anchoredPosition = startPos;

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            rect.Rotate(0f, 0f, 500f * Time.deltaTime);
            yield return null;
        }

        Destroy(proj);

        if (isHit)
        {
            StartCoroutine(AnimateHitExplosion(endPos));
            PlayEnemyHitAnim();
        }
    }

    private IEnumerator AnimateHitExplosion(Vector2 pos)
    {
        GameObject exp = new GameObject("Explosion");
        exp.transform.SetParent(playerAnimator.transform.parent, false);

        Image img = exp.AddComponent<Image>();
        Sprite spriteToUse = explosionSprite;
        if (spriteToUse == null)
        {
#if UNITY_EDITOR
            spriteToUse = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SunnyLand Artwork/Sprites/Fx/item-feedback/item-feedback-1.png");
#endif
        }
        img.sprite = spriteToUse;
        img.preserveAspect = true;
        img.color = new Color(1f, 0.9f, 0.4f, 1f); // Tinte dorado

        RectTransform rect = exp.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(30f, 30f);

        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(1f, 7f, t);
            rect.sizeDelta = new Vector2(30f * scale, 30f * scale);
            img.color = new Color(1f, 0.9f, 0.4f, 1f - t);
            yield return null;
        }

        Destroy(exp);
    }

    // Método para disparar la animación de ataque del enemigo hacia el jugador
    public void TriggerEnemyAttack(bool isHit)
    {
        if (playerAnimator == null || enemyRenderer == null) return;

        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) canvas = playerAnimator.GetComponentInParent<Canvas>();

        Vector2 endPos = ((RectTransform)playerAnimator.transform).anchoredPosition;
        // Ajustar punto de impacto de la mano del jugador
        endPos += new Vector2(65f, -10f);
        Vector2 startPos = Vector2.zero;

        if (canvas != null)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(enemyRenderer.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                screenPos,
                canvas.worldCamera,
                out startPos
            );
        }
        else
        {
            startPos = new Vector2(500f, 0f);
        }

        StartCoroutine(AnimateEnemyProjectile(startPos, endPos, isHit));
    }

    private IEnumerator AnimateEnemyProjectile(Vector2 startPos, Vector2 endPos, bool isHit)
    {
        Sprite spriteToUse = projectileSprite;
        if (spriteToUse == null)
        {
#if UNITY_EDITOR
            spriteToUse = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SunnyLand Artwork/Sprites/Items/gem/gem-1.png");
#endif
        }

        if (spriteToUse == null) yield break;

        GameObject proj = new GameObject("EnemyProjectile");
        proj.transform.SetParent(playerAnimator.transform.parent, false);

        Image img = proj.AddComponent<Image>();
        img.sprite = spriteToUse;
        img.preserveAspect = true;

        RectTransform rect = proj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120f, 120f);
        rect.anchoredPosition = startPos;

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            rect.Rotate(0f, 0f, -500f * Time.deltaTime); // Rotar en sentido opuesto
            yield return null;
        }

        Destroy(proj);

        if (isHit)
        {
            StartCoroutine(AnimateHitExplosion(endPos));
            PlayPlayerHitAnim();
        }
    }
}
