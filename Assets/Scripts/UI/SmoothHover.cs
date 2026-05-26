using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmoothHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    public float scaleFactor = 1.08f;
    public float clickScale = 0.94f;
    public float transitionSpeed = 12f;

    [Header("Color Settings")]
    public Color hoverColor = new Color(1.1f, 1.1f, 1.1f, 1f); // Slightly brighter
    
    private Vector3 targetScale;
    private Vector3 originalScale;
    private UnityEngine.UI.Image targetImage;
    private Color originalColor;
    private bool isHovered = false;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        targetImage = GetComponent<UnityEngine.UI.Image>();
        if (targetImage != null)
        {
            originalColor = targetImage.color;
        }
    }

    void OnDisable()
    {
        // Reset state when disabled to avoid getting stuck in hover scale
        transform.localScale = originalScale;
        targetScale = originalScale;
        isHovered = false;
        if (targetImage != null) targetImage.color = originalColor;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
        if (targetImage != null)
        {
            targetImage.color = Color.Lerp(targetImage.color, isHovered ? hoverColor : originalColor, Time.deltaTime * transitionSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleFactor;
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        isHovered = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = isHovered ? originalScale * scaleFactor : originalScale;
    }
}
