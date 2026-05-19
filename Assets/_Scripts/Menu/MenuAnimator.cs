using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Handles menu entrance animations and button hover effects for the main menu.
/// Attach to the Canvas or a root menu object. Assign buttons in the inspector.
/// </summary>
public class MenuAnimator : MonoBehaviour
{
    [Header("=== Panels ===")]
    [SerializeField] private RectTransform logoTransform;
    [SerializeField] private RectTransform buttonsContainer;
    [SerializeField] private RectTransform inputFieldTransform;
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;

    [Header("=== Animation Settings ===")]
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float logoDropDuration = 0.6f;
    [SerializeField] private float buttonStaggerDelay = 0.15f;
    [SerializeField] private float buttonSlideDuration = 0.4f;

    [Header("=== Button Hover ===")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverScaleSpeed = 8f;

    [Header("=== Floating Logo ===")]
    [SerializeField] private float floatAmplitude = 8f;
    [SerializeField] private float floatSpeed = 1.5f;

    [Header("=== Particle Stars (Optional) ===")]
    [SerializeField] private bool enableStarParticles = true;

    private Vector2 logoStartPos;
    private bool animationComplete;
    private Button[] menuButtons;

    private void Awake()
    {
        // Cache buttons
        if (buttonsContainer != null)
        {
            menuButtons = buttonsContainer.GetComponentsInChildren<Button>(true);
        }
        if (logoTransform != null)
        {
            logoStartPos = logoTransform.anchoredPosition;
        }
    }

    private static bool hasPlayedEntrance = false;

    private void OnEnable()
    {
        if (!hasPlayedEntrance)
        {
            StartCoroutine(PlayEntranceAnimation());
            hasPlayedEntrance = true;
        }
        else
        {
            // Just ensure everything is visible
            if (mainMenuCanvasGroup != null) mainMenuCanvasGroup.alpha = 1f;
            if (logoTransform != null) logoTransform.anchoredPosition = logoStartPos; // Use original pos
            if (menuButtons != null) 
            {
                foreach (Button btn in menuButtons)
                {
                    if (btn != null) btn.GetComponent<RectTransform>().localScale = Vector3.one;
                }
            }
            animationComplete = true;
        }
    }

    private IEnumerator PlayEntranceAnimation()
    {
        animationComplete = false;

        // Setup: hide everything
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
        }

        if (logoTransform != null)
        {
            logoTransform.anchoredPosition = logoStartPos + Vector2.up * 120f;
        }

        // Hide buttons by scaling to zero instead of sliding
        if (menuButtons != null)
        {
            foreach (Button btn in menuButtons)
            {
                RectTransform rt = btn.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.zero;
                }
            }
        }

        // Hide input field
        if (inputFieldTransform != null)
        {
            CanvasGroup inputCG = inputFieldTransform.GetComponent<CanvasGroup>();
            if (inputCG == null) inputCG = inputFieldTransform.gameObject.AddComponent<CanvasGroup>();
            inputCG.alpha = 0f;
        }

        yield return null;

        // Phase 1: Fade in the whole panel
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            float eased = EaseOutCubic(t);
            if (mainMenuCanvasGroup != null) mainMenuCanvasGroup.alpha = eased;
            yield return null;
        }
        if (mainMenuCanvasGroup != null) mainMenuCanvasGroup.alpha = 1f;

        // Phase 2: Drop logo into place
        if (logoTransform != null)
        {
            elapsed = 0f;
            Vector2 from = logoTransform.anchoredPosition;
            while (elapsed < logoDropDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / logoDropDuration);
                float eased = EaseOutBack(t);
                logoTransform.anchoredPosition = Vector2.Lerp(from, logoStartPos, eased);
                yield return null;
            }
            logoTransform.anchoredPosition = logoStartPos;
        }

        // Phase 3: Stagger buttons in from the left
        if (menuButtons != null)
        {
            for (int i = 0; i < menuButtons.Length; i++)
            {
                RectTransform rt = menuButtons[i].GetComponent<RectTransform>();
                if (rt == null) continue;
                StartCoroutine(PopButtonIn(rt));
                yield return new WaitForSecondsRealtime(buttonStaggerDelay);
            }
            yield return new WaitForSecondsRealtime(buttonSlideDuration);
        }

        // Phase 4: Fade in input field
        if (inputFieldTransform != null)
        {
            CanvasGroup inputCG = inputFieldTransform.GetComponent<CanvasGroup>();
            if (inputCG != null)
            {
                elapsed = 0f;
                while (elapsed < 0.4f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    inputCG.alpha = Mathf.Clamp01(elapsed / 0.4f);
                    yield return null;
                }
                inputCG.alpha = 1f;
            }
        }

        animationComplete = true;
    }

    private IEnumerator PopButtonIn(RectTransform rt)
    {
        float elapsed = 0f;
        while (elapsed < buttonSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / buttonSlideDuration);
            float eased = EaseOutBack(t);
            rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, eased);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    private void Update()
    {
        // Floating logo effect
        if (animationComplete && logoTransform != null)
        {
            float offset = Mathf.Sin(Time.unscaledTime * floatSpeed) * floatAmplitude;
            logoTransform.anchoredPosition = logoStartPos + Vector2.up * offset;
        }

        // Button hover scale
        if (menuButtons != null)
        {
            foreach (Button btn in menuButtons)
            {
                if (btn == null) continue;
                RectTransform rt = btn.GetComponent<RectTransform>();
                bool isHovered = EventSystem.current != null &&
                                 EventSystem.current.currentSelectedGameObject == btn.gameObject;

                // Also check pointer hover via raycasting
                if (!isHovered)
                {
                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                    pointerData.position = Input.mousePosition;
                    var results = new System.Collections.Generic.List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointerData, results);
                    foreach (var result in results)
                    {
                        if (result.gameObject == btn.gameObject)
                        {
                            isHovered = true;
                            break;
                        }
                    }
                }

                float targetScale = isHovered ? hoverScale : 1f;
                float currentScale = rt.localScale.x;
                float newScale = Mathf.Lerp(currentScale, targetScale, Time.unscaledDeltaTime * hoverScaleSpeed);
                rt.localScale = Vector3.one * newScale;
            }
        }
    }

    // --- Easing Functions ---
    private float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
