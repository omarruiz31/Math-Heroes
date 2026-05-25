using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageAnimateHelper : MonoBehaviour
{
    private Image image;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        image = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.enabled = false; // Desactivar renderizado en el mundo
        }
    }

    private void LateUpdate()
    {
        if (spriteRenderer != null && image != null)
        {
            image.sprite = spriteRenderer.sprite;
        }
    }
}
