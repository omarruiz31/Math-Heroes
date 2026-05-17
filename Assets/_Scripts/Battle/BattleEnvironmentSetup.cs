using UnityEngine;

public class BattleEnvironmentSetup : MonoBehaviour
{
    [Header("Punto de aparición del ambiente")]
    [SerializeField] private Transform environmentParent;  // GameObject vacío como contenedor

    [Header("Fallback")]
    [SerializeField] private GameObject defaultEnvironment; // Ambiente por defecto si no hay datos

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("BattleEnvironmentSetup: No hay GameManager activo.");
            ActivateDefault();
            return;
        }

        var envData = GameManager.Instance.currentEnvironment;

        if (envData != null && envData.tilemapPrefab != null)
        {
            // Instanciar el prefab de ambiente dentro del contenedor
            Instantiate(envData.tilemapPrefab, environmentParent);

            // Aplicar color ambiental a la cámara
            if (Camera.main != null)
                Camera.main.backgroundColor = envData.ambientColor;

            // Desactivar el ambiente por defecto si existe
            if (defaultEnvironment != null)
                defaultEnvironment.SetActive(false);

            Debug.Log($"Ambiente cargado: {envData.environmentName}");
        }
        else
        {
            ActivateDefault();
        }
    }

    private void ActivateDefault()
    {
        if (defaultEnvironment != null)
            defaultEnvironment.SetActive(true);
    }
}
