using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    [Header("Escena a cargar")]
    [Tooltip("El nombre exacto de la escena a la que quieres viajar (ej: NivelSumas)")]
    public string sceneToLoad;

    [Header("Opciones")]
    [Tooltip("Si está activo, el jugador debe presionar E para entrar. Si está apagado, entrará al tocarlo.")]
    public bool requireButtonPress = true;

    private bool playerInRange = false;

    private void Update()
    {
        if (requireButtonPress && playerInRange && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            Teleport();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "PlayerSprite")
        {
            if (requireButtonPress)
            {
                playerInRange = true;
                // Mostrar el mismo cartel de "Presiona [E]" que hicimos para los NPCs
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.ShowInteractPrompt(true);
                }
            }
            else
            {
                Teleport(); // Si no requiere botón, viaja automáticamente al tocar
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "PlayerSprite")
        {
            if (requireButtonPress)
            {
                playerInRange = false;
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.ShowInteractPrompt(false);
                }
            }
        }
    }

    private void Teleport()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Ocultamos el prompt por si nos cambiamos de escena
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowInteractPrompt(false);
            }
            
            // Cargamos la nueva escena
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("¡Falta el nombre de la escena en el Portal!");
        }
    }
}
