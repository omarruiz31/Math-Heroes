using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string BattleSceneName = "BattleScene";
    private const string MapSceneName = "WorldMap";

    private AudioSource audioSource;

    public static GameManager Instance { get; private set; }

    [Header("Estado actual")]
    public EnemyData currentEnemy;
    public int playerMaxHP = 100;
    public int playerCurrentHP = 100;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerCurrentHP = Mathf.Clamp(playerCurrentHP, 0, playerMaxHP);

        // Inicia la música solo una vez
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    public void StartBattle(EnemyData enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("No se puede iniciar una batalla sin EnemyData.");
            return;
        }

        currentEnemy = enemy;
        SceneManager.LoadScene(BattleSceneName);
    }

    public void ReturnToMap()
    {
        if (playerCurrentHP <= 0)
            HealPlayerToFull();

        SceneManager.LoadScene(MapSceneName);
    }

    public void HealPlayerToFull()
    {
        playerCurrentHP = playerMaxHP;
    }
}
