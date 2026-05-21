using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuSystem : MonoBehaviour
{
    [Header("Panels")]
    public GameObject optionsMenu;
    public GameObject mainMenu;

    [Header("Audio")]
    public AudioSource backgroundMusic;

    [Header("User Validation")]
    public TMP_InputField playerNameInput;
    public TextMeshProUGUI errorText;

    [Header("Reporte de la Maestra")]
    [Tooltip("Componente ReportUI — si está vacío, se buscará automáticamente")]
    public ReportUI reportUI;
    [Tooltip("Botón para abrir el reporte — si está vacío, se creará automáticamente en el panel de opciones")]
    public Button reportButton;

    private void Start()
    {
        // Find music source if not assigned
        if (backgroundMusic == null)
        {
            var musicObj = GameObject.Find("Music");
            if (musicObj != null) backgroundMusic = musicObj.GetComponent<AudioSource>();
        }
        
        if (errorText != null) errorText.gameObject.SetActive(false);

        // Configurar ReportUI
        SetupReportUI();
    }

    /// <summary>
    /// Configura el sistema de reporte. Si no hay ReportUI asignado, lo crea.
    /// Si no hay botón de reporte, lo crea en el panel de opciones.
    /// </summary>
    private void SetupReportUI()
    {
        // Buscar o crear ReportUI
        if (reportUI == null)
        {
            reportUI = FindAnyObjectByType<ReportUI>();
        }

        if (reportUI == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

            if (canvas != null)
            {
                GameObject reportObj = new GameObject("ReportUI");
                reportObj.transform.SetParent(canvas.transform, false);
                reportUI = reportObj.AddComponent<ReportUI>();
            }
        }

        // Crear botón de reporte en opciones si no existe
        if (reportButton == null && optionsMenu != null)
        {
            CreateReportButton();
        }

        // Conectar el botón al reporte
        if (reportButton != null && reportUI != null)
        {
            reportButton.onClick.RemoveAllListeners();
            reportButton.onClick.AddListener(OpenReport);
        }
    }

    /// <summary>
    /// Crea programáticamente el botón "Reporte del Alumno" en el panel de opciones.
    /// </summary>
    private void CreateReportButton()
    {
        GameObject btnObj = new GameObject("ReportButton");
        btnObj.transform.SetParent(optionsMenu.transform, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.25f, 0.02f);
        rt.anchorMax = new Vector2(0.75f, 0.12f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = btnObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.5f, 0.8f, 1f);

        reportButton = btnObj.AddComponent<Button>();
        reportButton.targetGraphic = bg;

        // Color transitions
        ColorBlock colors = reportButton.colors;
        colors.normalColor = new Color(0.2f, 0.5f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.6f, 0.9f, 1f);
        colors.pressedColor = new Color(0.15f, 0.4f, 0.7f, 1f);
        reportButton.colors = colors;

        // Texto del botón
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "📊 Reporte del Alumno";
        btnText.fontSize = 22;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontStyle = FontStyles.Bold;
    }

    /// <summary>
    /// Abre el panel de reporte.
    /// </summary>
    public void OpenReport()
    {
        if (reportUI != null)
        {
            reportUI.ShowReport();
        }
        else
        {
            Debug.LogWarning("[MenuSystem] ReportUI no disponible.");
        }
    }

    /// <summary>
    /// Cierra el panel de reporte.
    /// </summary>
    public void CloseReport()
    {
        if (reportUI != null)
            reportUI.HideReport();
    }

    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void OpenMainMenuPanel()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void Jugar()
    {
        if (playerNameInput != null && string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            if (errorText != null)
            {
                errorText.text = "¡Por favor, ingresa tu nombre de héroe!";
                errorText.color = Color.red;
                errorText.gameObject.SetActive(true);
            }
            Debug.LogWarning("Intento de jugar sin nombre");
            return;
        }

        // Save the profile name
        if (playerNameInput != null)
        {
            PlayerPrefs.SetString("CurrentPlayer", playerNameInput.text.Trim());
            PlayerPrefs.Save();
            Debug.Log("Jugador registrado: " + playerNameInput.text);
        }

        SceneManager.LoadScene("WorldMap");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
    }

    // === Options Functionality ===

    public void SetMusicVolume(float volume)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = volume;
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        // En un futuro podrías aplicar esto a un AudioMixer
        Debug.Log("SFX Volume set to: " + volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log("Fullscreen set to: " + isFullscreen);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        Debug.Log("Quality set to: " + QualitySettings.names[qualityIndex]);
    }

    public void SetQualityFloat(float qualityIndex)
    {
        SetQuality(Mathf.RoundToInt(qualityIndex));
    }
}

