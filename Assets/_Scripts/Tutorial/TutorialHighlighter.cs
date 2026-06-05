using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Creates a dark overlay with a "window" that highlights one or more UI elements.
/// Uses pre-existing overlay panel references from the scene UI.
/// </summary>
public class TutorialHighlighter : MonoBehaviour
{
    [Header("Referencias de UI en Escena")]
    public RectTransform overlayRoot;
    public Image topPanel;
    public Image bottomPanel;
    public Image rightPanel;
    public Image leftPanel;
    public RectTransform glowBorder;
    public Image glowImage;

    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private bool isInitialized = false;

    // ─── Animation ───
    private Coroutine transitionCoroutine;
    private Coroutine glowCoroutine;

    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.75f);
    private static readonly Color GlowColor = new Color(0.78f, 0.63f, 0.31f, 0.8f); // Dorado

    /// <summary>
    /// Initialize the highlighter with the active canvas.
    /// </summary>
    public void Initialize(Canvas canvas)
    {
        if (isInitialized) return;

        parentCanvas = canvas;
        canvasRect = canvas.GetComponent<RectTransform>();

        if (overlayRoot != null)
        {
            overlayRoot.gameObject.SetActive(false);
        }

        isInitialized = true;
    }

    /// <summary>
    /// Highlight one or more objects (UI or World). Pass null or empty to hide.
    /// </summary>
    public void Highlight(List<GameObject> targets)
    {
        if (!isInitialized) return;

        if (targets == null || targets.Count == 0)
        {
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(FadeOverlay(false));
            return;
        }

        if (overlayRoot != null)
        {
            overlayRoot.gameObject.SetActive(true);
            // Removed SetAsLastSibling() to avoid covering the dialogue panel
        }

        // Calculate combined bounding rect of all targets in Canvas space
        Rect bounds = GetCombinedBounds(targets);

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(AnimateHighlight(bounds));
    }

    /// <summary>
    /// Highlight a single object.
    /// </summary>
    public void Highlight(GameObject target)
    {
        if (target == null)
        {
            Highlight((List<GameObject>)null);
            return;
        }

        Highlight(new List<GameObject> { target });
    }

    /// <summary>
    /// Immediately hide the overlay.
    /// </summary>
    public void Hide()
    {
        if (!isInitialized) return;
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        if (glowCoroutine != null) StopCoroutine(glowCoroutine);
        if (overlayRoot != null) overlayRoot.gameObject.SetActive(false);
    }

    /// <summary>
    /// Make sure the overlay stays on top.
    /// </summary>
    public void BringToFront()
    {
        if (isInitialized && overlayRoot != null)
            overlayRoot.SetAsLastSibling();
    }

    // ═══════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════

    private Rect GetCombinedBounds(List<GameObject> targets)
    {
        float padding = 20f;
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        Camera cam = Camera.main;

        foreach (var target in targets)
        {
            if (target == null) continue;

            RectTransform rt = target.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Vector2 canvasPos = canvasRect.InverseTransformPoint(corner);
                    minX = Mathf.Min(minX, canvasPos.x);
                    maxX = Mathf.Max(maxX, canvasPos.x);
                    minY = Mathf.Min(minY, canvasPos.y);
                    maxY = Mathf.Max(maxY, canvasPos.y);
                }
            }
            else
            {
                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Bounds b = renderer.bounds;
                    Vector3[] worldCorners = new Vector3[8];
                    worldCorners[0] = b.center + new Vector3(-b.extents.x, -b.extents.y, -b.extents.z);
                    worldCorners[1] = b.center + new Vector3(b.extents.x, -b.extents.y, -b.extents.z);
                    worldCorners[2] = b.center + new Vector3(b.extents.x, b.extents.y, -b.extents.z);
                    worldCorners[3] = b.center + new Vector3(-b.extents.x, b.extents.y, -b.extents.z);
                    worldCorners[4] = b.center + new Vector3(-b.extents.x, -b.extents.y, b.extents.z);
                    worldCorners[5] = b.center + new Vector3(b.extents.x, -b.extents.y, b.extents.z);
                    worldCorners[6] = b.center + new Vector3(b.extents.x, b.extents.y, b.extents.z);
                    worldCorners[7] = b.center + new Vector3(-b.extents.x, b.extents.y, b.extents.z);

                    foreach (var wc in worldCorners)
                    {
                        Vector2 screenPos = cam.WorldToScreenPoint(wc);
                        Vector2 canvasPos;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam, out canvasPos);
                        
                        minX = Mathf.Min(minX, canvasPos.x);
                        maxX = Mathf.Max(maxX, canvasPos.x);
                        minY = Mathf.Min(minY, canvasPos.y);
                        maxY = Mathf.Max(maxY, canvasPos.y);
                    }
                }
            }
        }

        if (minX == float.MaxValue) return new Rect(0, 0, 100, 100);

        return new Rect(minX - padding, minY - padding,
                       (maxX - minX) + padding * 2,
                       (maxY - minY) + padding * 2);
    }

    private IEnumerator AnimateHighlight(Rect targetBounds)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        // Get current positions for smooth lerp
        Rect currentBounds = GetCurrentBounds();

        // Start glow pulse
        if (glowCoroutine != null) StopCoroutine(glowCoroutine);
        glowCoroutine = StartCoroutine(PulseGlow());

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            Rect lerpedBounds = new Rect(
                Mathf.Lerp(currentBounds.x, targetBounds.x, t),
                Mathf.Lerp(currentBounds.y, targetBounds.y, t),
                Mathf.Lerp(currentBounds.width, targetBounds.width, t),
                Mathf.Lerp(currentBounds.height, targetBounds.height, t)
            );

            ApplyBounds(lerpedBounds);
            yield return null;
        }

        ApplyBounds(targetBounds);
    }

    private void ApplyBounds(Rect bounds)
    {
        float cw = canvasRect.rect.width;
        float ch = canvasRect.rect.height;
        float halfW = cw / 2f;
        float halfH = ch / 2f;

        float left = bounds.xMin + halfW;
        float right = halfW - bounds.xMax;
        float bottom = bounds.yMin + halfH;
        float top = halfH - bounds.yMax;

        // Top panel
        if (topPanel != null)
            SetPanel(topPanel.rectTransform, 0, 1, 1, 1, 0, -top / 2f, 0, top);
        // Bottom panel
        if (bottomPanel != null)
            SetPanel(bottomPanel.rectTransform, 0, 0, 1, 0, 0, bottom / 2f, 0, bottom);
        // Left panel
        if (leftPanel != null)
            SetPanel(leftPanel.rectTransform, 0, 0, 0, 1, left / 2f, (bounds.yMin + bounds.yMax) / 2f, left, bounds.height);
        // Right panel
        if (rightPanel != null)
            SetPanel(rightPanel.rectTransform, 1, 0, 1, 1, -right / 2f, (bounds.yMin + bounds.yMax) / 2f, right, bounds.height);

        // Glow border
        if (glowBorder != null)
        {
            glowBorder.anchorMin = new Vector2(0.5f, 0.5f);
            glowBorder.anchorMax = new Vector2(0.5f, 0.5f);
            glowBorder.anchoredPosition = new Vector2(bounds.center.x, bounds.center.y);
            glowBorder.sizeDelta = new Vector2(bounds.width, bounds.height);
        }
    }

    private Rect GetCurrentBounds()
    {
        if (glowBorder == null || glowBorder.sizeDelta.magnitude < 1f)
        {
            // Not yet positioned, use center
            return new Rect(-50, -50, 100, 100);
        }

        return new Rect(
            glowBorder.anchoredPosition.x - glowBorder.sizeDelta.x / 2f,
            glowBorder.anchoredPosition.y - glowBorder.sizeDelta.y / 2f,
            glowBorder.sizeDelta.x,
            glowBorder.sizeDelta.y
        );
    }

    private void SetPanel(RectTransform rt, float ancMinX, float ancMinY, float ancMaxX, float ancMaxY,
                          float posX, float posY, float sizeX, float sizeY)
    {
        rt.anchorMin = new Vector2(ancMinX, ancMinY);
        rt.anchorMax = new Vector2(ancMaxX, ancMaxY);
        rt.anchoredPosition = new Vector2(posX, posY);
        rt.sizeDelta = new Vector2(sizeX, Mathf.Max(0, sizeY));
    }

    private IEnumerator PulseGlow()
    {
        if (glowImage == null) yield break;

        float speed = 2.5f;
        while (true)
        {
            float alpha = Mathf.Lerp(0.3f, 0.9f, (Mathf.Sin(Time.time * speed) + 1f) / 2f);
            glowImage.color = new Color(GlowColor.r, GlowColor.g, GlowColor.b, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeOverlay(bool fadeIn)
    {
        float duration = 0.25f;
        float elapsed = 0f;
        float startAlpha = fadeIn ? 0f : OverlayColor.a;
        float endAlpha = fadeIn ? OverlayColor.a : 0f;

        if (fadeIn && overlayRoot != null) overlayRoot.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            Color c = new Color(OverlayColor.r, OverlayColor.g, OverlayColor.b, alpha);
            if (topPanel != null) topPanel.color = c;
if (bottomPanel != null) bottomPanel.color = c;
            if (leftPanel != null) leftPanel.color = c;
            if (rightPanel != null) rightPanel.color = c;
            yield return null;
        }

        if (!fadeIn && overlayRoot != null) overlayRoot.gameObject.SetActive(false);
    }
}
