using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel de reporte para la maestra. Muestra estadísticas detalladas
/// del desempeño del alumno: porcentaje de acierto por operación,
/// historial de batallas, y preguntas recientes.
///
/// Se crea programáticamente — no necesita configuración manual en la escena.
/// Solo necesita una referencia al Canvas padre.
/// </summary>
public class ReportUI : MonoBehaviour
{
    [Header("Canvas padre (se asigna automáticamente si está vacío)")]
    [SerializeField] private Canvas parentCanvas;

    // ─── Referencias internas (se crean programáticamente) ───
    private GameObject reportPanel;
    private ScrollRect scrollRect;
    private TextMeshProUGUI reportContent;
    private Button closeButton;
    private bool isBuilt = false;

    private void Awake()
    {
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Abre el panel de reporte y genera el contenido.
    /// </summary>
    public void ShowReport()
    {
        if (!isBuilt) BuildReportPanel();

        var gm = GameManager.Instance;
        if (gm == null || gm.playerData == null)
        {
            reportContent.text = "<color=#FF6B6B>No hay datos de jugador cargados.</color>\n\nSelecciona un perfil primero.";
            reportPanel.SetActive(true);
            return;
        }

        reportContent.text = GenerateReportText(gm.playerData);
        reportPanel.SetActive(true);

        // Scroll hasta arriba
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// Cierra el panel de reporte.
    /// </summary>
    public void HideReport()
    {
        if (reportPanel != null)
            reportPanel.SetActive(false);
    }

    // ═══════════════════════════════════════════
    //  GENERACIÓN DEL TEXTO DEL REPORTE
    // ═══════════════════════════════════════════

    private string GenerateReportText(PlayerData data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // ─── Encabezado ───
        sb.AppendLine("<size=28><color=#FFD700>📊 REPORTE DE DESEMPEÑO</color></size>");
        sb.AppendLine("<color=#AAAAAA>─────────────────────────────────────</color>");
        sb.AppendLine();

        // ─── Datos del alumno ───
        sb.AppendLine("<size=22><color=#4FC3F7>👤 DATOS DEL ALUMNO</color></size>");

        // Nombre con fallback a PlayerPrefs si está vacío
        string displayName = data.playerName;
        if (string.IsNullOrEmpty(displayName))
            displayName = PlayerPrefs.GetString("CurrentPlayer", "Sin nombre");

        sb.AppendLine($"  Nombre:   <color=#FFFFFF>{displayName}</color>");
        sb.AppendLine($"  Nivel:    <color=#FFFFFF>{data.level}</color>");
        sb.AppendLine($"  XP:       <color=#FFFFFF>{data.currentXP} / {data.XPToNextLevel()}</color>");
        sb.AppendLine($"  HP Máx:   <color=#FFFFFF>{data.maxHP}</color>");
        sb.AppendLine();

        // ─── Resumen general ───
        sb.AppendLine("<size=22><color=#4FC3F7>📈 RESUMEN GENERAL</color></size>");

        int totalQuestions = data.GetTotalQuestionsAnswered();
        float overallAccuracy = data.GetOverallAccuracy();
        float avgTime = data.GetAverageResponseTime();

        sb.AppendLine($"  Total de preguntas:     <color=#FFFFFF>{totalQuestions}</color>");

        if (overallAccuracy >= 0)
        {
            string accuracyColor = GetAccuracyColor(overallAccuracy);
            sb.AppendLine($"  Acierto global:         <color={accuracyColor}>{overallAccuracy:F1}%</color>");
        }
        else
        {
            sb.AppendLine("  Acierto global:         <color=#888888>Sin datos</color>");
        }

        sb.AppendLine($"  Respuestas correctas:   <color=#66BB6A>{data.totalCorrectAnswers}</color>");
        sb.AppendLine($"  Respuestas incorrectas: <color=#EF5350>{data.totalWrongAnswers}</color>");

        if (avgTime >= 0)
            sb.AppendLine($"  Tiempo promedio:        <color=#FFFFFF>{avgTime:F1}s</color>");

        sb.AppendLine($"  Batallas ganadas:       <color=#66BB6A>{data.totalBattlesWon}</color>");
        sb.AppendLine($"  Batallas perdidas:      <color=#EF5350>{data.totalBattlesLost}</color>");
        sb.AppendLine();

        // ─── Desglose por operación ───
        sb.AppendLine("<size=22><color=#4FC3F7>🧮 DESGLOSE POR OPERACIÓN</color></size>");
        sb.AppendLine();

        string[] operations = { "+", "-", "×", "÷" };
        string[] opNames = { "Suma (+)", "Resta (-)", "Multiplicación (×)", "División (÷)" };

        for (int i = 0; i < operations.Length; i++)
        {
            float accuracy = data.GetAccuracyByOperation(operations[i]);
            int count = data.GetQuestionsCountByOperation(operations[i]);

            if (count > 0)
            {
                string color = GetAccuracyColor(accuracy);
                string bar = GenerateProgressBar(accuracy);
                sb.AppendLine($"  <color=#FFFFFF>{opNames[i]}</color>");
                sb.AppendLine($"    {bar}  <color={color}>{accuracy:F1}%</color>  ({count} preguntas)");
            }
            else
            {
                sb.AppendLine($"  <color=#888888>{opNames[i]}: Sin datos</color>");
            }
        }

        sb.AppendLine();

        // ─── Historial de batallas ───
        sb.AppendLine("<size=22><color=#4FC3F7>⚔️ HISTORIAL DE BATALLAS</color></size>");

        if (data.battleHistory.Count == 0)
        {
            sb.AppendLine("  <color=#888888>No hay batallas registradas.</color>");
        }
        else
        {
            // Mostrar las últimas 15 batallas (más recientes primero)
            int start = Mathf.Max(0, data.battleHistory.Count - 15);
            for (int i = data.battleHistory.Count - 1; i >= start; i--)
            {
                BattleRecord b = data.battleHistory[i];
                string result = b.won
                    ? "<color=#66BB6A>✓ Victoria</color>"
                    : "<color=#EF5350>✗ Derrota</color>";

                int totalAnswers = b.correctAnswers + b.wrongAnswers;
                float battleAccuracy = totalAnswers > 0
                    ? (b.correctAnswers / (float)totalAnswers) * 100f
                    : 0f;

                string timeStr = FormatTime(b.timeSeconds);

                sb.AppendLine($"  <color=#AAAAAA>{b.date}</color> — {result}");
                sb.AppendLine($"    vs <color=#FFFFFF>{b.enemyName}</color> | " +
                              $"Aciertos: {b.correctAnswers}/{totalAnswers} ({battleAccuracy:F0}%) | " +
                              $"Tiempo: {timeStr}");
            }
        }

        sb.AppendLine();

        // ─── Últimas 20 preguntas ───
        sb.AppendLine("<size=22><color=#4FC3F7>📝 ÚLTIMAS 20 PREGUNTAS</color></size>");

        List<QuestionRecord> recent = data.GetRecentQuestions(20);
        if (recent.Count == 0)
        {
            sb.AppendLine("  <color=#888888>No hay preguntas registradas.</color>");
        }
        else
        {
            // Mostrar más recientes primero
            for (int i = recent.Count - 1; i >= 0; i--)
            {
                QuestionRecord q = recent[i];
                string icon = q.wasCorrect ? "<color=#66BB6A>✓</color>" : "<color=#EF5350>✗</color>";
                string answerStr = q.playerAnswer == -999
                    ? "<color=#FFA726>Sin respuesta</color>"
                    : $"Respondió: <color=#FFFFFF>{q.playerAnswer}</color>";

                sb.AppendLine($"  {icon} {q.numberA} {q.operation} {q.numberB} = {q.correctAnswer}" +
                              $"  |  {answerStr}  |  {q.timeUsed:F1}s" +
                              $"  <color=#AAAAAA>({q.date})</color>");
            }
        }

        sb.AppendLine();
        sb.AppendLine("<color=#AAAAAA>─────────────────────────────────────</color>");
        sb.AppendLine("<color=#888888>Reporte generado automáticamente por Math Heroes</color>");

        return sb.ToString();
    }

    // ═══════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════

    private string GetAccuracyColor(float accuracy)
    {
        if (accuracy >= 80f) return "#66BB6A";  // Verde
        if (accuracy >= 60f) return "#FFA726";  // Naranja
        return "#EF5350";                       // Rojo
    }

    private string GenerateProgressBar(float percentage)
    {
        int filled = Mathf.RoundToInt(percentage / 10f);
        int empty = 10 - filled;

        string bar = "<color=#66BB6A>";
        for (int i = 0; i < filled; i++) bar += "█";
        bar += "</color><color=#333333>";
        for (int i = 0; i < empty; i++) bar += "█";
        bar += "</color>";

        return bar;
    }

    private string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return mins > 0 ? $"{mins}m {secs}s" : $"{secs}s";
    }

    // ═══════════════════════════════════════════
    //  CONSTRUCCIÓN PROGRAMÁTICA DEL PANEL
    // ═══════════════════════════════════════════

    private void BuildReportPanel()
    {
        if (parentCanvas == null)
        {
            Debug.LogError("[ReportUI] No se encontró Canvas padre.");
            return;
        }

        // ─── Panel principal (fondo oscuro semitransparente) ───
        reportPanel = new GameObject("ReportPanel");
        reportPanel.transform.SetParent(parentCanvas.transform, false);

        RectTransform panelRT = reportPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelBg = reportPanel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.12f, 0.97f);

        // ─── Contenedor interior con margen ───
        GameObject innerContainer = new GameObject("InnerContainer");
        innerContainer.transform.SetParent(reportPanel.transform, false);

        RectTransform innerRT = innerContainer.AddComponent<RectTransform>();
        innerRT.anchorMin = new Vector2(0.05f, 0.05f);
        innerRT.anchorMax = new Vector2(0.95f, 0.95f);
        innerRT.offsetMin = Vector2.zero;
        innerRT.offsetMax = Vector2.zero;

        // ─── Título ───
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(innerContainer.transform, false);

        RectTransform titleRT = titleObj.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.92f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "📊 Reporte del Alumno";
        titleText.fontSize = 32;
        titleText.color = new Color(1f, 0.84f, 0f); // Gold
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;

        // ─── Botón cerrar ───
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(innerContainer.transform, false);

        RectTransform closeRT = closeObj.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.85f, 0.92f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.offsetMin = Vector2.zero;
        closeRT.offsetMax = Vector2.zero;

        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(0.9f, 0.3f, 0.3f, 1f);

        closeButton = closeObj.AddComponent<Button>();
        closeButton.targetGraphic = closeBg;
        closeButton.onClick.AddListener(HideReport);

        // Texto del botón cerrar
        GameObject closeTextObj = new GameObject("CloseText");
        closeTextObj.transform.SetParent(closeObj.transform, false);

        RectTransform closeTextRT = closeTextObj.AddComponent<RectTransform>();
        closeTextRT.anchorMin = Vector2.zero;
        closeTextRT.anchorMax = Vector2.one;
        closeTextRT.offsetMin = Vector2.zero;
        closeTextRT.offsetMax = Vector2.zero;

        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "✕ Cerrar";
        closeText.fontSize = 20;
        closeText.color = Color.white;
        closeText.alignment = TextAlignmentOptions.Center;

        // ─── Scroll View ───
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(innerContainer.transform, false);

        RectTransform scrollRT = scrollObj.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 0.90f);
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = Vector2.zero;

        scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30f;

        // Máscara
        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = new Color(0.08f, 0.08f, 0.15f, 1f);
        Mask mask = scrollObj.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollObj.transform, false);

        RectTransform viewportRT = viewportObj.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = new Vector2(10f, 10f);
        viewportRT.offsetMax = new Vector2(-10f, -10f);

        // Content (el texto scrolleable)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);

        RectTransform contentRT = contentObj.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        reportContent = contentObj.AddComponent<TextMeshProUGUI>();
        reportContent.fontSize = 16;
        reportContent.color = new Color(0.85f, 0.85f, 0.9f);
        reportContent.alignment = TextAlignmentOptions.TopLeft;
        reportContent.enableWordWrapping = true;
        reportContent.richText = true;
        reportContent.overflowMode = TextOverflowModes.Overflow;
        reportContent.lineSpacing = 5f;

        // Configurar scroll rect
        scrollRect.content = contentRT;
        scrollRect.viewport = viewportRT;

        // ─── Scrollbar vertical ───
        GameObject scrollbarObj = new GameObject("Scrollbar");
        scrollbarObj.transform.SetParent(scrollObj.transform, false);

        RectTransform scrollbarRT = scrollbarObj.AddComponent<RectTransform>();
        scrollbarRT.anchorMin = new Vector2(1f, 0f);
        scrollbarRT.anchorMax = new Vector2(1f, 1f);
        scrollbarRT.pivot = new Vector2(1f, 0.5f);
        scrollbarRT.sizeDelta = new Vector2(12f, 0f);
        scrollbarRT.offsetMin = new Vector2(-12f, 0f);
        scrollbarRT.offsetMax = new Vector2(0f, 0f);

        Image scrollbarBg = scrollbarObj.AddComponent<Image>();
        scrollbarBg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        // Handle del scrollbar
        GameObject handleArea = new GameObject("HandleArea");
        handleArea.transform.SetParent(scrollbarObj.transform, false);

        RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = Vector2.zero;
        handleAreaRT.offsetMax = Vector2.zero;

        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleArea.transform, false);

        RectTransform handleRT = handleObj.AddComponent<RectTransform>();
        handleRT.anchorMin = Vector2.zero;
        handleRT.anchorMax = Vector2.one;
        handleRT.offsetMin = Vector2.zero;
        handleRT.offsetMax = Vector2.zero;

        Image handleImg = handleObj.AddComponent<Image>();
        handleImg.color = new Color(0.4f, 0.4f, 0.6f, 1f);

        scrollbar.handleRect = handleRT;
        scrollbar.targetGraphic = handleImg;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        reportPanel.SetActive(false);
        isBuilt = true;

        Debug.Log("[ReportUI] Panel de reporte construido exitosamente.");
    }
}
