using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
private const string BattleSceneName = "BattleScene";
    private string lastMapSceneName = "WorldMap";

    private AudioSource audioSource;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager (Auto-Created)");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    private static GameManager instance;

    [Header("Estado actual")]
    public EnemyData currentEnemy;
    public BattleEnvironmentData currentEnvironment;
    public int playerMaxHP = 100;
    public int playerCurrentHP = 100;

    // ─── Datos persistentes del jugador ───
    [HideInInspector] 
    public PlayerData playerData
    {
        get
        {
            if (playerDataInstance == null)
            {
                string lastPlayer = PlayerPrefs.GetString("CurrentPlayer", "");
                if (!string.IsNullOrEmpty(lastPlayer) && SaveSystem.ProfileExists(lastPlayer))
                {
                    playerDataInstance = SaveSystem.Load(lastPlayer);
                    profileLoaded = true;
                }
                else
                {
                    playerDataInstance = new PlayerData();
                    playerDataInstance.playerName = string.IsNullOrEmpty(lastPlayer) ? "Héroe de Prueba" : lastPlayer;
                    profileLoaded = true;
                    if (!string.IsNullOrEmpty(lastPlayer))
                    {
                        SaveSystem.Save(playerDataInstance);
                    }
                }
            }
            return playerDataInstance;
        }
        set
        {
            playerDataInstance = value;
        }
    }
    private PlayerData playerDataInstance;
    [HideInInspector] public bool profileLoaded = false;

    // ─── Estadísticas de la batalla actual (RF 22) ───
    [HideInInspector] public int battleCorrectAnswers = 0;
    [HideInInspector] public int battleWrongAnswers = 0;
    [HideInInspector] public float battleStartTime = 0f;
    [HideInInspector] public string pendingZoneUnlock = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Inicia la música solo una vez
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying)
            audioSource.Play();

        // Aplica el volumen guardado por el menú de opciones
        AudioListener.volume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
    }

    // ═══════════════════════════════════════════
    //  GESTIÓN DE PERFILES
    // ═══════════════════════════════════════════

    /// <summary>
    /// Carga un perfil existente por nombre.
    /// </summary>
    public void LoadProfile(string profileName)
    {
        playerData = SaveSystem.Load(profileName);
        if (playerData == null)
        {
            Debug.LogError($"No se pudo cargar el perfil: {profileName}");
            return;
        }

        ApplyPlayerData();
        Debug.Log($"Perfil cargado: {playerData.playerName} — Nivel {playerData.level}");
    }

    /// <summary>
    /// Crea un perfil nuevo con el nombre dado.
    /// </summary>
    public void CreateProfile(string playerName)
    {
        playerData = new PlayerData();
        playerData.playerName = playerName;
        SaveSystem.Save(playerData);

        ApplyPlayerData();
        Debug.Log($"Perfil creado: {playerName}");
    }

    /// <summary>
    /// Aplica los datos del PlayerData al estado del juego.
    /// </summary>
    private void ApplyPlayerData()
    {
        playerMaxHP = playerData.maxHP;
        playerCurrentHP = playerMaxHP;
        profileLoaded = true;
    }

    /// <summary>
    /// Establece el nombre del jugador y guarda.
    /// </summary>
    public void SetPlayerName(string name)
    {
        playerData.playerName = name;
        SaveSystem.Save(playerData);
    }

    // ═══════════════════════════════════════════
    //  BATALLAS
    // ═══════════════════════════════════════════

    [Header("Transition")]
    [SerializeField] private GameObject sceneTransitionPrefab;

    public void StartBattle(EnemyData enemy, BattleEnvironmentData environment = null)
    {
        if (enemy == null)
        {
            Debug.LogWarning("No se puede iniciar una batalla sin EnemyData.");
            return;
        }

        StartCoroutine(TransitionToScene(BattleSceneName, () => {
            currentEnemy = enemy;
            currentEnvironment = environment;

            // Resetear contadores de la batalla actual
            battleCorrectAnswers = 0;
            battleWrongAnswers = 0;
            battleStartTime = Time.time;

            // Guardar la escena actual antes de entrar a la batalla
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene != BattleSceneName)
            {
                lastMapSceneName = currentScene;
            }
        }));
    }

    private IEnumerator TransitionToScene(string sceneName, System.Action beforeLoad = null)
    {
        if (SceneTransitionManager.Instance == null && sceneTransitionPrefab != null)
        {
            Instantiate(sceneTransitionPrefab);
        }

        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.FadeOut();
        }
        
        beforeLoad?.Invoke();
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Llamar cuando el jugador gana una batalla.
    /// Otorga XP, registra historial y guarda la partida.
    /// </summary>
    public void OnBattleWon(string enemyName)
    {
        float battleTime = Time.time - battleStartTime;

        // Calcular XP ganado (base 20 + 5 por respuesta correcta)
        int xpGained = 20 + (battleCorrectAnswers * 5);

        // Registrar en historial (RF 10)
        playerData.RecordBattle(enemyName, true, battleCorrectAnswers,
                                 battleWrongAnswers, battleTime);

        // Otorgar XP (RF 11)
        bool leveledUp = playerData.AddXP(xpGained);

        if (leveledUp)
        {
            playerMaxHP = playerData.maxHP;
            playerCurrentHP = playerMaxHP;
            Debug.Log($"¡Subiste al nivel {playerData.level}! HP máximo: {playerMaxHP}");
        }

        // Desbloquear zona si corresponde (RF 05)
        if (!string.IsNullOrEmpty(pendingZoneUnlock))
            playerData.UnlockZone(pendingZoneUnlock);

        // Guardar partida automáticamente
        SaveSystem.Save(playerData);

        Debug.Log($"Batalla ganada — XP: +{xpGained} | Total: {playerData.currentXP}/{playerData.XPToNextLevel()}");
    }

    /// <summary>
    /// Llamar cuando el jugador pierde una batalla.
    /// </summary>
    public void OnBattleLost(string enemyName)
    {
        float battleTime = Time.time - battleStartTime;

        playerData.RecordBattle(enemyName, false, battleCorrectAnswers,
                                 battleWrongAnswers, battleTime);

        SaveSystem.Save(playerData);
    }

    public void ReturnToMap()
    {
        if (playerCurrentHP <= 0)
            HealPlayerToFull();

        StartCoroutine(TransitionToScene(lastMapSceneName));
    }

    /// <summary>
    /// Reinicia la batalla actual con el mismo enemigo y ambiente.
    /// </summary>
    public void RetryBattle()
    {
        if (playerCurrentHP <= 0)
            HealPlayerToFull();

        StartCoroutine(TransitionToScene(BattleSceneName, () => {
            // Resetear contadores de la batalla
            battleCorrectAnswers = 0;
            battleWrongAnswers = 0;
            battleStartTime = Time.time;
        }));
    }

    public void HealPlayerToFull()
    {
        playerCurrentHP = playerMaxHP;
    }

    /// <summary>
    /// Verifica si una zona del mapa está desbloqueada (RF 05).
    /// </summary>
    public bool IsZoneUnlocked(string zoneName)
    {
        return playerData.IsZoneUnlocked(zoneName);
    }
}
