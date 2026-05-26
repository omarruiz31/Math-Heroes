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
        // Safety check: if the object starts with 0 scale (e.g. for animation), 
        // we assume the original scale should be (1,1,1) or the value before animation.
        originalScale = transform.localScale;
        if (originalScale.magnitude < 0.01f)
        {
            originalScale = Vector3.one;
        }
        
        targetScale = originalScale;
        targetImage = GetComponent<UnityEngine.UI.Image>();
        if (targetImage != null)
        {
            originalColor = targetImage.color;
        }
    }

    void OnEnable()
    {
        // Sync targetScale with current scale to avoid snapping if enabled during animation
        targetScale = transform.localScale;
        if (targetScale.magnitude < 0.01f) targetScale = originalScale;
    }

    void OnDisable()
    {
        // Reset state when disabled to avoid getting stuck in hover scale
        // But only if we have a valid original scale
        if (originalScale.magnitude > 0.01f)
        {
            transform.localScale = originalScale;
            targetScale = originalScale;
        }
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
