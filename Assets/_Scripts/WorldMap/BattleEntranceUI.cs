using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleEntranceUI : MonoBehaviour
{
    public static BattleEntranceUI Instance { get; private set; }

    [Header("UI Panel References")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI operationsText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private Button enterButton;
    [SerializeField] private Button cancelButton;

    private System.Action onEnterCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Muestra la pantalla de confirmación antes de la batalla.
    /// </summary>
    public void ShowEntrance(string battleZoneName, EnemyData enemy, System.Action onConfirm)
    {
        if (modalPanel == null)
        {
            Debug.LogError("[BattleEntranceUI] Modal Panel is not assigned!");
            return;
        }

        onEnterCallback = onConfirm;

        // Asegurar que los botones tengan los Listeners vinculados
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(Hide);
        }

        if (enterButton != null)
        {
            enterButton.onClick.RemoveAllListeners();
            enterButton.onClick.AddListener(ConfirmEnter);
        }

        // Configurar textos
        if (titleText != null)
        {
            titleText.text = battleZoneName;
        }

        // Obtener operaciones
        if (operationsText != null)
        {
            List<string> ops = new List<string>();
            if (enemy.useAddition) ops.Add("<color=#66BB6A>Suma (+)</color>");
            if (enemy.useSubtraction) ops.Add("<color=#EF5350>Resta (-)</color>");
            if (enemy.useMultiplication) ops.Add("<color=#4FC3F7>Multiplicación (×)</color>");
            if (enemy.useDivision) ops.Add("<color=#FFCA28>División (÷)</color>");

            if (ops.Count == 0) ops.Add("Suma (+)");
            operationsText.text = "<b>Operaciones:</b> " + string.Join(", ", ops);
        }

        // Calcular XP aproximado
        if (xpText != null)
        {
            // Daño estimado por respuesta correcta: 24 (rango 18-30)
            int estimatedCorrectAnswers = Mathf.CeilToInt((float)enemy.maxHP / 24f);
            int xpEstimate = 20 + (estimatedCorrectAnswers * 5);
            xpText.text = $"<b>Recompensa:</b> ~{xpEstimate} XP (20 base + 5 por acierto)";
        }

        modalPanel.SetActive(true);

        // Desactivar el movimiento del jugador mientras esté el modal
        SetPlayerMovement(false);
    }

    public void Hide()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }
        onEnterCallback = null;

        // Reactivar el movimiento del jugador
        SetPlayerMovement(true);
    }

    private void ConfirmEnter()
    {
        onEnterCallback?.Invoke();
        Hide();
    }

    private void SetPlayerMovement(bool enable)
    {
        var player = FindAnyObjectByType<WorldMapPlayerController>();
        if (player != null)
        {
            player.enabled = enable;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null && !enable)
            {
                rb.linearVelocity = Vector2.zero; // Detener completamente el deslizamiento
            }
        }
    }
}
