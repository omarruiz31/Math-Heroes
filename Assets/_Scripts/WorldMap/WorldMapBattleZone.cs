using UnityEngine;

public class WorldMapBattleZone : MonoBehaviour
{
    public EnemyData enemy;
    [SerializeField] private BattleEnvironmentData environment;
    [SerializeField] private string zoneName = "Zona";
    [SerializeField] private float entryDelay = 0.25f;
    [SerializeField] private float triggerRadius = 1.25f;

    [Header("Progresión (RF 05)")]
    [Tooltip("Si está vacío, la zona siempre está desbloqueada")]
    [SerializeField] private string requiredUnlockName = "";
    [Tooltip("Nombre de la zona que se desbloquea al ganar esta batalla")]
    [SerializeField] private string zoneToUnlockOnWin = "";

    [Header("Visual de zona bloqueada")]
    [SerializeField] private Color lockedTint = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    private bool enteringBattle;
    private bool isLocked;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        CircleCollider2D zoneCollider = GetComponent<CircleCollider2D>();
        if (zoneCollider == null)
            zoneCollider = gameObject.AddComponent<CircleCollider2D>();

        zoneCollider.isTrigger = true;
        zoneCollider.radius = triggerRadius;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateLockState();
    }

    /// <summary>
    /// Revisa si esta zona está bloqueada según el progreso del jugador.
    /// </summary>
    public void UpdateLockState()
    {
        // Si no tiene nombre de desbloqueo, siempre está abierta
        if (string.IsNullOrEmpty(requiredUnlockName))
        {
            isLocked = false;
        }
        else if (GameManager.Instance != null)
        {
            isLocked = !GameManager.Instance.IsZoneUnlocked(requiredUnlockName);
        }

        // Aplicar tinte visual
        if (spriteRenderer != null)
            spriteRenderer.color = isLocked ? lockedTint : Color.white;
    }

    /// <summary>
    /// Nombre de la zona que se desbloquea al ganar aquí.
    /// El BattleManager lo usa al llamar OnBattleWon().
    /// </summary>
    public string ZoneToUnlockOnWin => zoneToUnlockOnWin;

    public void Configure(EnemyData zoneEnemy, string displayName)
    {
        enemy = zoneEnemy;
        zoneName = displayName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enteringBattle || other.GetComponent<WorldMapPlayerController>() == null) return;

        if (isLocked)
        {
            Debug.Log($"La zona {zoneName} está bloqueada. ¡Derrota enemigos anteriores primero!");
            return;
        }

        enteringBattle = true;

        if (BattleEntranceUI.Instance != null)
        {
            BattleEntranceUI.Instance.ShowEntrance(zoneName, enemy, () =>
            {
                StartBattleDirectly();
            });
        }
        else
        {
            // Fallback si no está el modal en la escena
            Invoke(nameof(StartBattle), entryDelay);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<WorldMapPlayerController>() == null) return;

        enteringBattle = false;
        CancelInvoke(nameof(StartBattle));

        if (BattleEntranceUI.Instance != null)
        {
            BattleEntranceUI.Instance.Hide();
        }
    }

    private void StartBattle()
    {
        if (!enteringBattle) return;
        StartBattleDirectly();
    }

    private void StartBattleDirectly()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError($"No hay GameManager para entrar a {zoneName}.");
            return;
        }

        if (enemy == null)
        {
            Debug.LogWarning($"La zona {zoneName} no tiene EnemyData asignado.");
            return;
        }

        // Guardar referencia a la zona que se desbloquea al ganar
        GameManager.Instance.pendingZoneUnlock = zoneToUnlockOnWin;
        GameManager.Instance.StartBattle(enemy, environment);
    }
}
