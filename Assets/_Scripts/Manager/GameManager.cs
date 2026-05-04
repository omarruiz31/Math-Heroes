using UnityEngine;

public class GameManager : MonoBehaviour
{
    private AudioSource audioSource;

    public static GameManager Instance { get; private set; }

    [Header("Estado actual")]
    public EnemyData currentEnemy;
    public int playerMaxHP = 100;
    public int playerCurrentHP = 100;

    void Awake()
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
}

    public void StartBattle(EnemyData enemy)
    {
        currentEnemy = enemy;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
    }

    public void ReturnToMap()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Mapa");
    }
}