using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    [TextArea(3, 10)]
    public string sentence;
    public Sprite characterPortrait;
}

[System.Serializable]
public class Dialogue
{
    public DialogueLine[] lines;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialogueUI;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    
    [Header("Settings")]
    public float typingSpeed = 0.02f;

    [Header("Interact Prompt")]
    public GameObject interactPrompt;
    public TextMeshProUGUI interactPromptText;

    private Queue<DialogueLine> sentences;
    private bool isTyping = false;
    private string currentSentence = "";
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sentences = new Queue<DialogueLine>();
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    public void ShowInteractPrompt(bool show, string message = "Presiona [E] para hablar")
    {
        if (interactPrompt != null && !dialogueUI.activeSelf)
        {
            if (show && interactPromptText != null)
            {
                interactPromptText.text = message;
            }
            interactPrompt.SetActive(show);
        }
    }

    private void Update()
    {
        // Avanzar el diálogo si se presiona Espacio, Enter o Click izquierdo
        if (dialogueUI.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
        {
            if (isTyping)
            {
                // Si está escribiendo, termina la frase instantáneamente
                CompleteSentence();
            }
            else
            {
                // Si ya terminó de escribir, pasa a la siguiente
                DisplayNextSentence();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueUI.SetActive(true);
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
        sentences.Clear();

        foreach (DialogueLine line in dialogue.lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = sentences.Dequeue();
        
        nameText.text = line.characterName;
        
        if (line.characterPortrait != null)
        {
            portraitImage.sprite = line.characterPortrait;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        currentSentence = line.sentence;
        typingCoroutine = StartCoroutine(TypeSentence(line.sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;
        
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }

    private void CompleteSentence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        dialogueText.text = currentSentence;
        isTyping = false;
    }

    public void EndDialogue()
    {
        dialogueUI.SetActive(false);
    }
}
