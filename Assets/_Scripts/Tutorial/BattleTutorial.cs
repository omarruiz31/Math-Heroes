using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tutorial interactivo y visual para la BattleScene.
/// Controla el flujo del tutorial utilizando la UI pre-configurada en la escena.
/// Solo se muestra si PlayerData.battleTutorialCompleted == false.
/// 
/// Pasos:
/// 1. Bienvenida general
/// 2. Enemigo + HP del enemigo
/// 3. Jugador + HP del jugador
/// 4. Pregunta de matemáticas
/// 5. Input de respuesta + botón enviar
/// 6. Temporizador + despedida
/// </summary>
public class BattleTutorial : MonoBehaviour
{
    [Header("Referencias de UI en Escena")]
    public GameObject tutorialRoot;
    public CanvasGroup dialogueGroup;
    public TextMeshProUGUI messageText;
    public Button nextButton;
    public TextMeshProUGUI buttonText;
    public Transform dotsContainer;

    // ─── Internal state ───
    private int currentStep = 0;
    private BattleUI battleUI;
    private TutorialHighlighter highlighter;
    private Canvas mainCanvas;

    // ─── Step definitions ───
    private List<TutorialStepData> steps;

    // ─── Constants: Medieval palette for dots ───
    private static readonly Color BORDER_GOLD = new Color(0.78f, 0.63f, 0.31f, 1f);
    private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);

    private struct TutorialStepData
    {
        public string message;
        public string[] targetNames;  // Names of GameObjects to highlight
        public string buttonLabel;
    }

    public void StartTutorial(BattleUI ui)
    {
        battleUI = ui;

        // Find the main UI canvas reliably
        mainCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            mainCanvas = ui.GetComponentInChildren<Canvas>();
            if (mainCanvas == null)
                mainCanvas = ui.GetComponentInParent<Canvas>();
        }

        highlighter = GetComponent<TutorialHighlighter>();
        if (highlighter == null)
            highlighter = gameObject.AddComponent<TutorialHighlighter>();

        highlighter.Initialize(mainCanvas);

        if (tutorialRoot != null)
        {
            tutorialRoot.SetActive(true);
        }

        // Define steps
        steps = new List<TutorialStepData>
        {
            new TutorialStepData
            {
                message = "¡Bienvenido a tu primera batalla, héroe!\n\nAquí lucharás contra enemigos resolviendo problemas de matemáticas. ¡Cada respuesta correcta es un ataque poderoso!",
                targetNames = null,
                buttonLabel = "¡Entendido!"
            },
            new TutorialStepData
            {
                message = "Este es tu enemigo. ¡Observa su barra de vida arriba!\n\nDebes reducir su vida a cero para ganar la batalla. ¡Cada respuesta correcta le hará daño!",
                targetNames = new[] { "EnemySprite", "EnemyHPBar", "EnemyNameText", "EnemyHPText" },
                buttonLabel = "Siguiente >"
            },
            new TutorialStepData
            {
                message = "¡Y esta es tu vida!\n\nSi tu barra de vida llega a cero, perderás. Cada respuesta incorrecta o tiempo agotado permite al enemigo atacarte. ¡Protégela!",
                targetNames = new[] { "PlayerSprite", "PlayerHPBar", "PlayerHPText" },
                buttonLabel = "Siguiente >"
            },
            new TutorialStepData
            {
                message = "Aquí aparecerán las preguntas de matemáticas.\n\n¡Lee bien la operación antes de responder! Pueden ser sumas, restas, multiplicaciones o divisiones.",
                targetNames = new[] { "QuestionText" },
                buttonLabel = "Siguiente >"
            },
            new TutorialStepData
            {
                message = "Escribe tu respuesta en este campo y presiona el botón para enviarla.\n\n¡Mientras más rápido respondas, mejor!",
                targetNames = new[] { "AnswerInput", "SubmitButton" },
                buttonLabel = "Siguiente >"
            },
            new TutorialStepData
            {
                message = "¡Cuidado con el temporizador!\n\nSi se acaba el tiempo sin responder, el enemigo te atacará. ¡Responde rápido y bien!\n\n¡Buena suerte, héroe!",
                targetNames = new[] { "TimerText" },
                buttonLabel = "¡A luchar!"
            }
        };

        // Connect button click
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClicked);
        }

        currentStep = 0;
        ShowStep();
    }

    // ═══════════════════════════════════════════
    //  STEP MANAGEMENT
    // ═══════════════════════════════════════════

    private void ShowStep()
    {
        if (steps == null || currentStep >= steps.Count)
        {
            EndTutorial();
            return;
        }

        TutorialStepData step = steps[currentStep];

        // Update button text
        if (buttonText != null)
        {
            buttonText.text = step.buttonLabel;
        }

        // Update step dots
        UpdateStepDots();

        // Find and highlight targets
        List<GameObject> targets = FindTargets(step.targetNames);
        highlighter.Highlight(targets);

        // Make sure tutorial UI stays on top of overlay
        if (dialogueGroup != null)
        {
            dialogueGroup.transform.SetAsLastSibling();
        }
        else if (tutorialRoot != null)
        {
            tutorialRoot.transform.SetAsLastSibling();
        }

        // Animate text
        StopAllCoroutines();
        StartCoroutine(AnimateStep(step.message));
    }

    private void UpdateStepDots()
    {
        if (dotsContainer == null) return;

        for (int i = 0; i < dotsContainer.childCount && i < steps.Count; i++)
        {
            Image dotImg = dotsContainer.GetChild(i).GetComponent<Image>();
            if (dotImg != null)
            {
                if (i == currentStep)
                    dotImg.color = TEXT_GOLD;
                else if (i < currentStep)
                    dotImg.color = BORDER_GOLD;
                else
                    dotImg.color = new Color(BORDER_GOLD.r, BORDER_GOLD.g, BORDER_GOLD.b, 0.3f);
            }
        }
    }

    private List<GameObject> FindTargets(string[] names)
    {
        if (names == null || names.Length == 0) return null;

        List<GameObject> targets = new List<GameObject>();

        foreach (string name in names)
        {
            // Search in Canvas children (and deeper)
            GameObject found = FindInCanvas(name);
            if (found != null)
            {
                targets.Add(found);
            }
            else
            {
                // Also check scene root objects
                GameObject go = GameObject.Find(name);
                if (go != null)
                {
                    targets.Add(go);
                }
            }
        }

        return targets.Count > 0 ? targets : null;
    }

    private GameObject FindInCanvas(string objectName)
    {
        if (mainCanvas == null) return null;

        // Recursive search through all canvas children
        Transform found = FindChildRecursive(mainCanvas.transform, objectName);
        return found != null ? found.gameObject : null;
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform found = FindChildRecursive(child, childName);
            if (found != null) return found;
        }

        return null;
    }

    // ═══════════════════════════════════════════
    //  ANIMATIONS
    // ═══════════════════════════════════════════

    private IEnumerator AnimateStep(string message)
    {
        if (dialogueGroup == null || messageText == null || nextButton == null) yield break;

        // Disable button during animation
        nextButton.interactable = false;

        // Fade in dialogue
        float fadeTime = 0.25f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            dialogueGroup.alpha = Mathf.Lerp(dialogueGroup.alpha, 1f, elapsed / fadeTime);
            yield return null;
        }
        dialogueGroup.alpha = 1f;

        // Typewriter effect
        messageText.text = "";
        bool isTag = false;
        string currentText = "";

        foreach (char c in message)
        {
            if (c == '<') isTag = true;
            if (isTag)
            {
                currentText += c;
                if (c == '>')
                {
                    isTag = false;
                    messageText.text = currentText;
                }
                continue;
            }

            currentText += c;
            messageText.text = currentText;

            float delay = (c == ' ' || c == '\n') ? 0.008f : 0.018f;
            yield return new WaitForSeconds(delay);
        }

        // Enable button with a pulse animation
        nextButton.interactable = true;
        StartCoroutine(PulseButton());
    }

    private IEnumerator PulseButton()
    {
        if (nextButton == null) yield break;

        RectTransform btnRT = nextButton.GetComponent<RectTransform>();
        if (btnRT == null) yield break;

        Vector3 originalScale = Vector3.one;

        while (nextButton != null && nextButton.interactable)
        {
            float scale = 1f + Mathf.Sin(Time.time * 3f) * 0.04f;
            btnRT.localScale = originalScale * scale;
            yield return null;
        }

        if (btnRT != null)
            btnRT.localScale = originalScale;
    }

    // ═══════════════════════════════════════════
    //  EVENT HANDLERS
    // ═══════════════════════════════════════════

    private void OnNextClicked()
    {
        currentStep++;

        if (steps == null || currentStep >= steps.Count)
        {
            EndTutorial();
        }
        else
        {
            // Brief fade out, then show next step
            StartCoroutine(TransitionToNextStep());
        }
    }

    private IEnumerator TransitionToNextStep()
    {
        if (nextButton != null)
            nextButton.interactable = false;

        if (dialogueGroup != null)
        {
            float fadeTime = 0.15f;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                dialogueGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                yield return null;
            }
            dialogueGroup.alpha = 0f;
        }

        ShowStep();
    }

    private void EndTutorial()
    {
        StartCoroutine(EndTutorialSequence());
    }

    private IEnumerator EndTutorialSequence()
    {
        if (dialogueGroup != null)
        {
            float fadeTime = 0.3f;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                dialogueGroup.alpha = Mathf.Lerp(dialogueGroup.alpha, 0f, elapsed / fadeTime);
                yield return null;
            }
            dialogueGroup.alpha = 0f;
        }

        if (highlighter != null)
        {
            highlighter.Hide();
        }

        if (tutorialRoot != null)
        {
            tutorialRoot.SetActive(false);
        }

        // Mark tutorial as complete
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.battleTutorialCompleted = true;
            SaveSystem.Save(GameManager.Instance.playerData);
        }

        // Start the battle
        if (BattleManager.Instance != null)
            BattleManager.Instance.StartBattleAfterTutorial();
    }
}
