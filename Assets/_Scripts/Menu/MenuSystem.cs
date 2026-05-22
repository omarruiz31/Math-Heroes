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
    /// Configura el sistema de reporte.
    /// </summary>
    private void SetupReportUI()
    {
        // Buscar ReportUI si no está asignado
        if (reportUI == null)
        {
            reportUI = FindAnyObjectByType<ReportUI>();
        }

        // Conectar el botón al reporte
        if (reportButton != null && reportUI != null)
        {
            reportButton.onClick.RemoveAllListeners();
            reportButton.onClick.AddListener(OpenReport);
        }
    }

    /// <summary>
    /// Abre el panel de reporte.
    /// </summary>
    public void OpenReport()
    {
        if (playerNameInput != null && !string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            string name = playerNameInput.text.Trim();
            PlayerPrefs.SetString("CurrentPlayer", name);
            PlayerPrefs.Save();

            if (GameManager.Instance != null)
            {
                if (SaveSystem.ProfileExists(name))
                {
                    GameManager.Instance.LoadProfile(name);
                }
                else
                {
                    GameManager.Instance.CreateProfile(name);
                }
            }
        }

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
            string name = playerNameInput.text.Trim();
            PlayerPrefs.SetString("CurrentPlayer", name);
            PlayerPrefs.Save();
            Debug.Log("Jugador registrado: " + name);

            if (GameManager.Instance != null)
            {
                if (SaveSystem.ProfileExists(name))
                {
                    GameManager.Instance.LoadProfile(name);
                }
                else
                {
                    GameManager.Instance.CreateProfile(name);
                }
            }
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

