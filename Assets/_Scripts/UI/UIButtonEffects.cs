using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    public Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1.05f);
    public Vector3 pressScale = new Vector3(0.95f, 0.95f, 0.95f);
    public float scaleSpeed = 12f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovered = false;

    private void Awake()
    {
        originalScale = transform.localScale;
        if (originalScale == Vector3.zero) originalScale = Vector3.one;
        targetScale = originalScale;
    }

    private void Start()
    {
        // Fallback en caso de que cambie la escala antes de Awake
        if (originalScale == Vector3.one && transform.localScale != Vector3.one)
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
        }
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetScale = Vector3.Scale(originalScale, hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = Vector3.Scale(originalScale, pressScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isHovered)
        {
            targetScale = Vector3.Scale(originalScale, hoverScale);
        }
        else
        {
            targetScale = originalScale;
        }
    }

    private void OnDisable()
    {
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
            targetScale = originalScale;
        }
        isHovered = false;
    }
}
