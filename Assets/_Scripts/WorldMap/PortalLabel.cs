using UnityEngine;
using TMPro;

/// <summary>
/// Agrega un texto flotante encima de un portal/zona para indicar
/// visualmente su nombre y estado (desbloqueado/bloqueado).
///
/// === USO ===
/// Agregar este componente al mismo GameObject que tiene WorldMapBattleZone.
/// El texto se crea automáticamente — no necesitas configurar nada en la escena.
/// Solo ajusta los valores en el Inspector si quieres personalizar.
/// </summary>
[RequireComponent(typeof(WorldMapBattleZone))]
public class PortalLabel : MonoBehaviour
{
    [Header("Texto")]
    [Tooltip("Nombre que se muestra sobre el portal. Si está vacío, usa el nombre del GameObject.")]
    [SerializeField] private string displayName = "";

    [Header("Apariencia")]
    [SerializeField] private float fontSize = 4f;
    [SerializeField] private Color unlockedColor = new Color(1f, 0.92f, 0.6f);  // Dorado claro
    [SerializeField] private Color lockedColor = new Color(0.6f, 0.6f, 0.6f);   // Gris
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -1f);        // Posición arriba/enfrente del portal

    [Header("Renderizado (Sorting)")]
    [Tooltip("El nombre de la Sorting Layer donde quieres renderizar el texto (ej: Default, Foreground, UI).")]
    [SerializeField] private string sortingLayerName = "Default";
    [Tooltip("Orden de renderizado dentro de la capa. Números mayores se dibujan encima.")]
    [SerializeField] private int sortingOrder = 100;

    [Header("Efecto de flotación")]
    [SerializeField] private bool enableFloat = true;
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Icono de candado")]
    [SerializeField] private bool showLockIcon = true;

    // ─── Referencias internas ───
    private TextMeshPro label;
    private Vector3 basePosition;
    private WorldMapBattleZone battleZone;
    private bool wasLocked;

    private void Start()
    {
        battleZone = GetComponent<WorldMapBattleZone>();
        CreateLabel();
        UpdateLabelState();
    }

    private void Update()
    {
        // Efecto de flotación suave
        if (enableFloat && label != null)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            label.transform.localPosition = basePosition + Vector3.up * yOffset;
        }
    }

    /// <summary>
    /// Actualiza el texto y color según si el portal está bloqueado o no.
    /// Llamar después de que cambie el estado de desbloqueo.
    /// </summary>
    public void UpdateLabelState()
    {
        if (label == null) return;

        // Detectar si está bloqueado leyendo el tint del SpriteRenderer
        // (WorldMapBattleZone cambia el color cuando está locked)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        bool isLocked = sr != null && sr.color.r < 0.5f && sr.color.g < 0.5f && sr.color.b < 0.5f;

        string name = GetDisplayName();

        if (isLocked)
        {
            label.text = showLockIcon ? $"🔒 {name}" : name;
            label.color = lockedColor;
        }
        else
        {
            label.text = name;
            label.color = unlockedColor;
        }

        wasLocked = isLocked;
    }

    /// <summary>
    /// Cambia el nombre mostrado en runtime.
    /// </summary>
    public void SetDisplayName(string newName)
    {
        displayName = newName;
        UpdateLabelState();
    }

    // ═══════════════════════════════════════════
    //  CREACIÓN DEL LABEL
    // ═══════════════════════════════════════════

    private void CreateLabel()
    {
        // Crear un hijo con TextMeshPro (World Space)
        GameObject labelObj = new GameObject("PortalLabel");
        labelObj.transform.SetParent(transform, false);

        basePosition = offset;
        labelObj.transform.localPosition = basePosition;

        label = labelObj.AddComponent<TextMeshPro>();
        label.text = GetDisplayName();
        label.fontSize = fontSize;
        label.color = unlockedColor;
        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Bold;

        // Configuración de rendering para 2D (Sorting Layer y Sorting Order)
        MeshRenderer meshRenderer = label.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = sortingLayerName;
            meshRenderer.sortingOrder = sortingOrder;
        }
        label.enableWordWrapping = false;

        // Tamaño del rectángulo de texto
        RectTransform rt = label.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(10f, 2f);
    }

    private string GetDisplayName()
    {
        if (!string.IsNullOrEmpty(displayName))
            return displayName;

        // Fallback: usar el nombre del GameObject limpio
        return gameObject.name.Replace("_", " ").Replace("Portal", "").Trim();
    }
}
