using UnityEngine;

public class TestBattleStarter : MonoBehaviour
{
    public EnemyData testEnemy;

    private void Awake()
    {
        // Simula que el GameManager ya tiene un enemigo cargado
        if (GameManager.Instance != null && testEnemy != null)
            GameManager.Instance.currentEnemy = testEnemy;
    }
}
