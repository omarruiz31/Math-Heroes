using UnityEngine;

public class WorldMapBattleZone : MonoBehaviour
{
    [SerializeField] private EnemyData enemy;
    [SerializeField] private BattleEnvironmentData environment;
    [SerializeField] private string zoneName = "Zona";
    [SerializeField] private float entryDelay = 0.25f;
    [SerializeField] private float triggerRadius = 1.25f;

    private bool enteringBattle;

    private void Awake()
    {
        CircleCollider2D zoneCollider = GetComponent<CircleCollider2D>();
        if (zoneCollider == null)
            zoneCollider = gameObject.AddComponent<CircleCollider2D>();

        zoneCollider.isTrigger = true;
        zoneCollider.radius = triggerRadius;
    }

    public void Configure(EnemyData zoneEnemy, string displayName)
    {
        enemy = zoneEnemy;
        zoneName = displayName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enteringBattle || other.GetComponent<WorldMapPlayerController>() == null) return;

        enteringBattle = true;
        Invoke(nameof(StartBattle), entryDelay);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<WorldMapPlayerController>() == null) return;

        enteringBattle = false;
        CancelInvoke(nameof(StartBattle));
    }

    private void StartBattle()
    {
        if (!enteringBattle) return;

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

        GameManager.Instance.StartBattle(enemy, environment);
    }
}
