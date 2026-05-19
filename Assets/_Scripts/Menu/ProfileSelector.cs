using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Selector de perfiles para múltiples jugadores.
/// Muestra una lista de perfiles guardados y permite crear uno nuevo.
///
/// === SETUP EN UNITY ===
/// En la escena "menu", crea un Panel "ProfileSelectPanel" con:
///   - Un ScrollView (o un contenedor vertical) → asignar a profileListParent
///   - Un botón prefab con TextMeshProUGUI → asignar a profileButtonPrefab
///   - Un botón "Nuevo Jugador" → asignar a newProfileButton
///   - Un panel para crear perfil con InputField y botón confirmar
/// </summary>
public class ProfileSelector : MonoBehaviour
{
    [Header("Panel de selección de perfil")]
    [SerializeField] private GameObject profileSelectPanel;
    [SerializeField] private Transform profileListParent;
    [SerializeField] private GameObject profileButtonPrefab;
    [SerializeField] private Button newProfileButton;

    [Header("Panel de crear perfil nuevo")]
    [SerializeField] private GameObject createProfilePanel;
    [SerializeField] private TMP_InputField newNameInputField;
    [SerializeField] private Button confirmCreateButton;
    [SerializeField] private TextMeshProUGUI createErrorText;

    [Header("Panel del menú principal")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Texto de bienvenida (opcional)")]
    [Tooltip("Un texto en el menú principal que muestre '¡Hola, Omar!'")]
    [SerializeField] private TextMeshProUGUI welcomeText;

    private void Start()
    {
        // Mostrar selector de perfiles al inicio
        ShowProfileSelect();
    }

    /// <summary>
    /// Muestra la pantalla de selección de perfiles.
    /// </summary>
    public void ShowProfileSelect()
    {
        profileSelectPanel.SetActive(true);
        createProfilePanel.SetActive(false);
        mainMenuPanel.SetActive(false);

        // Limpiar botones anteriores
        foreach (Transform child in profileListParent)
            Destroy(child.gameObject);

        // Crear un botón por cada perfil guardado
        List<string> profiles = SaveSystem.GetAllProfileNames();

        foreach (string profileName in profiles)
        {
            GameObject buttonObj = Instantiate(profileButtonPrefab, profileListParent);
            buttonObj.SetActive(true);

            // Cargar datos para mostrar info
            PlayerData data = SaveSystem.Load(profileName);

            // Configurar texto del botón
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null && data != null)
                buttonText.text = $"{data.playerName}  —  Nivel {data.level}";
            else if (buttonText != null)
                buttonText.text = profileName;

            // Al hacer clic, cargar este perfil
            Button btn = buttonObj.GetComponent<Button>();
            string nameCopy = profileName; // Captura para el closure
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectProfile(nameCopy));
        }

        // Botón de nuevo perfil
        newProfileButton.onClick.RemoveAllListeners();
        newProfileButton.onClick.AddListener(ShowCreateProfile);
    }

    /// <summary>
    /// Muestra el panel para crear un nuevo perfil.
    /// </summary>
    private void ShowCreateProfile()
    {
        profileSelectPanel.SetActive(false);
        createProfilePanel.SetActive(true);

        newNameInputField.text = "";
        if (createErrorText != null)
            createErrorText.text = "";

        newNameInputField.ActivateInputField();

        confirmCreateButton.onClick.RemoveAllListeners();
        confirmCreateButton.onClick.AddListener(ConfirmCreateProfile);

        newNameInputField.onSubmit.RemoveAllListeners();
        newNameInputField.onSubmit.AddListener(_ => ConfirmCreateProfile());
    }

    /// <summary>
    /// Confirma la creación del nuevo perfil.
    /// </summary>
    private void ConfirmCreateProfile()
    {
        string playerName = newNameInputField.text.Trim();

        // Validar longitud
        if (playerName.Length < 2)
        {
            ShowError("Escribe al menos 2 letras");
            return;
        }

        // Validar que no exista ya
        if (SaveSystem.ProfileExists(playerName))
        {
            ShowError($"Ya existe un jugador llamado \"{playerName}\"");
            return;
        }

        // Crear perfil y entrar al menú
        GameManager.Instance.CreateProfile(playerName);
        EnterMainMenu(playerName);
    }

    /// <summary>
    /// Selecciona un perfil existente y entra al menú.
    /// </summary>
    private void SelectProfile(string profileName)
    {
        GameManager.Instance.LoadProfile(profileName);
        EnterMainMenu(profileName);
    }

    /// <summary>
    /// Transición al menú principal con el perfil cargado.
    /// </summary>
    private void EnterMainMenu(string playerName)
    {
        profileSelectPanel.SetActive(false);
        createProfilePanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        if (welcomeText != null)
            welcomeText.text = $"¡Hola, {playerName}!";
    }

    /// <summary>
    /// Volver a la selección de perfiles (para cambiar de jugador).
    /// </summary>
    public void BackToProfileSelect()
    {
        ShowProfileSelect();
    }

    private void ShowError(string message)
    {
        if (createErrorText != null)
        {
            createErrorText.text = message;
            createErrorText.color = Color.red;
        }
    }
}
