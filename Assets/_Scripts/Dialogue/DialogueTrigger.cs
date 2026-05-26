using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }
    
    // Opcional: si quieres que inicie automáticamente al tocar el objeto
    private bool playerInRange;

    private void Update()
    {
        // Si el jugador está cerca y presiona la tecla 'E' o 'Espacio'
        if (playerInRange && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            TriggerDialogue();
        }
    }
    
    // Detectar cuando el jugador se acerca al NPC
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Revisamos si el que entró es el jugador (por Tag o por el nombre de tu objeto PlayerSprite)
        if (collision.CompareTag("Player") || collision.name == "PlayerSprite")
        {
            playerInRange = true;
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowInteractPrompt(true);
            }
        }
    }

    // Detectar cuando el jugador se aleja
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "PlayerSprite")
        {
            playerInRange = false;
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowInteractPrompt(false);
            }
        }
    }
}
