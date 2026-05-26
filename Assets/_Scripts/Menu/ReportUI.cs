using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel de reporte para la maestra. Muestra estadísticas detalladas
/// del desempeño del alumno: porcentaje de acierto por operación,
/// historial de batallas, y preguntas recientes.
/// </summary>
public class ReportUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject reportPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI reportContent;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Dropdown profileDropdown;

    [SerializeField] private Button deleteProfileButton;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HideReport);
        }

        if (profileDropdown != null)
        {
            profileDropdown.onValueChanged.RemoveAllListeners();
            profileDropdown.onValueChanged.AddListener(OnProfileChanged);
        }

        if (deleteProfileButton != null)
        {
            deleteProfileButton.onClick.RemoveAllListeners();
            deleteProfileButton.onClick.AddListener(DeleteCurrentProfile);
        }
    }

    /// <summary>
    /// Elimina el perfil seleccionado actualmente en el dropdown.
    /// </summary>
    public void DeleteCurrentProfile()
    {
        if (profileDropdown == null || profileDropdown.options.Count == 0) return;

        string profileName = profileDropdown.options[profileDropdown.value].text;
        
        // Confirmación simple (opcional, pero recomendada)
        // Aquí lo borramos directamente como se pidió limpiar el ejecutable
        SaveSystem.DeleteProfile(profileName);
        
        // Si el perfil borrado era el activo en el GameManager, resetearlo
        if (GameManager.Instance != null && GameManager.Instance.playerData != null && 
            GameManager.Instance.playerData.playerName == profileName)
        {
            GameManager.Instance.playerData = new PlayerData();
            PlayerPrefs.DeleteKey("CurrentPlayer");
        }

        // Refrescar la lista
        ShowReport();
        
        Debug.Log($"Perfil '{profileName}' eliminado desde el reporte.");
    }

    /// <summary>
    /// Abre el panel de reporte y genera el contenido.
    /// </summary>
    public void ShowReport()
    {
        var gm = GameManager.Instance;

        // Configurar el dropdown con todos los perfiles guardados
        if (profileDropdown != null)
        {
            profileDropdown.onValueChanged.RemoveAllListeners(); // Evitar disparar eventos durante la inicialización
            profileDropdown.ClearOptions();

            List<string> profiles = SaveSystem.GetAllProfileNames();

            // Si la lista está vacía, pero hay un jugador actual, lo añadimos
            if (gm != null && gm.playerData != null && !profiles.Contains(gm.playerData.playerName))
            {
                profiles.Add(gm.playerData.playerName);
            }

            profileDropdown.AddOptions(profiles);

            // Seleccionar por defecto el perfil activo
            if (gm != null && gm.playerData != null)
            {
                int currentIndex = profiles.IndexOf(gm.playerData.playerName);
                if (currentIndex >= 0)
                {
                    profileDropdown.value = currentIndex;
                }
            }

            profileDropdown.onValueChanged.AddListener(OnProfileChanged);
        }

        // Obtener los datos del perfil a mostrar
        PlayerData activeData = null;
        if (gm != null && gm.playerData != null)
        {
            activeData = gm.playerData;
        }
        else if (profileDropdown != null && profileDropdown.options.Count > 0)
        {
            string firstProfile = profileDropdown.options[0].text;
            activeData = SaveSystem.Load(firstProfile);
        }

        if (activeData == null)
        {
            if (reportContent != null)
                reportContent.text = "<color=#FF6B6B>No hay datos de jugador cargados.</color>\n\nSelecciona un perfil primero.";
            if (reportPanel != null)
                reportPanel.SetActive(true);
            return;
        }

        if (reportContent != null)
            reportContent.text = GenerateReportText(activeData);
        
        if (reportPanel != null)
            reportPanel.SetActive(true);

        // Scroll hasta arriba
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// Se llama cuando se selecciona un perfil diferente en la lista desplegable.
    /// </summary>
    private void OnProfileChanged(int index)
    {
        if (profileDropdown == null || index < 0 || index >= profileDropdown.options.Count) return;

        string profileName = profileDropdown.options[index].text;
        PlayerData selectedData = SaveSystem.Load(profileName);

        if (selectedData != null)
        {
            if (reportContent != null)
                reportContent.text = GenerateReportText(selectedData);

            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;
        }
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
        sb.AppendLine("<size=28><color=#FFD700>REPORTE DE DESEMPEÑO</color></size>");
        sb.AppendLine("<color=#AAAAAA>─────────────────────────────────────</color>");
        sb.AppendLine();

        // ─── Datos del alumno ───
        sb.AppendLine("<size=22><color=#4FC3F7>DATOS DEL ALUMNO</color></size>");

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
        sb.AppendLine("<size=22><color=#4FC3F7>RESUMEN GENERAL</color></size>");

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
        sb.AppendLine("<size=22><color=#4FC3F7>DESGLOSE POR OPERACIÓN</color></size>");
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
        sb.AppendLine("<size=22><color=#4FC3F7>HISTORIAL DE BATALLAS</color></size>");

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
                    ? "<color=#66BB6A>Victoria</color>"
                    : "<color=#EF5350>Derrota</color>";

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
        sb.AppendLine("<color=#AAAAAA>─────────────────────────────────────</color>");
        sb.AppendLine();

        // ─── Modo Frenesí ───
        sb.AppendLine("<size=22><color=#FF5252>ESTADÍSTICAS MODO FRENESÍ</color></size>");

        if (data.totalFrenzySessions == 0)
        {
            sb.AppendLine("  <color=#888888>Aún no has participado en el modo frenesí.</color>");
        }
        else
        {
            float avgEnemies = (float)data.totalFrenzyEnemiesDefeated / data.totalFrenzySessions;
            sb.AppendLine($"  Récord máximo:        <color=#FFD700>{data.frenzyHighscore} enemigos</color>");
            sb.AppendLine($"  Enemigos totales:     <color=#FFFFFF>{data.totalFrenzyEnemiesDefeated}</color>");
            sb.AppendLine($"  Promedio por sesión:  <color=#FFFFFF>{avgEnemies:F1}</color>");
            sb.AppendLine($"  Sesiones jugadas:     <color=#FFFFFF>{data.totalFrenzySessions}</color>");
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
}
