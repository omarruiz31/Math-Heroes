using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class DialogueSystemSetup : EditorWindow
{
    [MenuItem("Tools/Math Heroes/Crear UI de Dialogos")]
    public static void CreateDialogueUI()
    {
        // 1. Crear el Canvas
        GameObject canvasObj = new GameObject("DialogueCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // Para que aparezca encima de todo
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Crear el Panel Principal (Fondo oscuro opcional)
        GameObject panelObj = new GameObject("DialoguePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.5f); // Fondo semitransparente

        // 3. Crear el Cuadro de Texto de Diálogo (Bottom)
        GameObject boxObj = new GameObject("DialogueBox");
        boxObj.transform.SetParent(panelObj.transform, false);
        RectTransform boxRect = boxObj.AddComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0);
        boxRect.anchorMax = new Vector2(0.5f, 0);
        boxRect.pivot = new Vector2(0.5f, 0);
        boxRect.anchoredPosition = new Vector2(0, 50);
        boxRect.sizeDelta = new Vector2(1400, 250);
        Image boxBg = boxObj.AddComponent<Image>();
        boxBg.color = new Color(0.9f, 0.8f, 0.6f); // Color tipo pergamino

        // 4. Crear la Imagen del Retrato
        GameObject portraitObj = new GameObject("PortraitImage");
        portraitObj.transform.SetParent(boxObj.transform, false);
        RectTransform portraitRect = portraitObj.AddComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0, 0.5f);
        portraitRect.anchorMax = new Vector2(0, 0.5f);
        portraitRect.pivot = new Vector2(0, 0.5f);
        portraitRect.anchoredPosition = new Vector2(20, 0);
        portraitRect.sizeDelta = new Vector2(200, 200);
        Image portraitImg = portraitObj.AddComponent<Image>();

        // 5. Crear el Cuadro del Nombre
        GameObject nameBoxObj = new GameObject("NameBox");
        nameBoxObj.transform.SetParent(boxObj.transform, false);
        RectTransform nameBoxRect = nameBoxObj.AddComponent<RectTransform>();
        nameBoxRect.anchorMin = new Vector2(0, 1);
        nameBoxRect.anchorMax = new Vector2(0, 1);
        nameBoxRect.pivot = new Vector2(0, 0);
        nameBoxRect.anchoredPosition = new Vector2(20, 10);
        nameBoxRect.sizeDelta = new Vector2(300, 60);
        Image nameBoxBg = nameBoxObj.AddComponent<Image>();
        nameBoxBg.color = new Color(0.8f, 0.7f, 0.5f);

        // 6. Texto del Nombre
        GameObject nameTextObj = new GameObject("NameText");
        nameTextObj.transform.SetParent(nameBoxObj.transform, false);
        RectTransform nameTextRect = nameTextObj.AddComponent<RectTransform>();
        nameTextRect.anchorMin = new Vector2(0, 0);
        nameTextRect.anchorMax = new Vector2(1, 1);
        nameTextRect.offsetMin = new Vector2(10, 0);
        nameTextRect.offsetMax = new Vector2(-10, 0);
        TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Nombre";
        nameText.fontSize = 36;
        nameText.alignment = TextAlignmentOptions.CenterGeoAligned;
        nameText.color = Color.black;

        // 7. Texto del Diálogo
        GameObject dialogTextObj = new GameObject("DialogueText");
        dialogTextObj.transform.SetParent(boxObj.transform, false);
        RectTransform dialogTextRect = dialogTextObj.AddComponent<RectTransform>();
        dialogTextRect.anchorMin = new Vector2(0, 0);
        dialogTextRect.anchorMax = new Vector2(1, 1);
        dialogTextRect.offsetMin = new Vector2(250, 20); // Deja espacio para el retrato
        dialogTextRect.offsetMax = new Vector2(-20, -20);
        TextMeshProUGUI dialogText = dialogTextObj.AddComponent<TextMeshProUGUI>();
        dialogText.text = "Este es un texto de prueba para el diálogo. Aquí aparecerán las instrucciones.";
        dialogText.fontSize = 42;
        dialogText.alignment = TextAlignmentOptions.TopLeft;
        dialogText.color = Color.black;
        dialogText.enableWordWrapping = true;

        // 7.5. Aviso de Interacción (Interact Prompt)
        GameObject interactPromptObj = new GameObject("InteractPrompt");
        interactPromptObj.transform.SetParent(canvasObj.transform, false);
        RectTransform promptRect = interactPromptObj.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0);
        promptRect.anchorMax = new Vector2(0.5f, 0);
        promptRect.pivot = new Vector2(0.5f, 0);
        promptRect.anchoredPosition = new Vector2(0, 50);
        promptRect.sizeDelta = new Vector2(400, 60);
        Image promptBg = interactPromptObj.AddComponent<Image>();
        promptBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        GameObject promptTextObj = new GameObject("PromptText");
        promptTextObj.transform.SetParent(interactPromptObj.transform, false);
        RectTransform promptTextRect = promptTextObj.AddComponent<RectTransform>();
        promptTextRect.anchorMin = new Vector2(0, 0);
        promptTextRect.anchorMax = new Vector2(1, 1);
        promptTextRect.offsetMin = Vector2.zero;
        promptTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI promptText = promptTextObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "Presiona [E] para hablar";
        promptText.fontSize = 32;
        promptText.alignment = TextAlignmentOptions.CenterGeoAligned;
        promptText.color = Color.white;
        interactPromptObj.SetActive(false);

        // 8. Crear el DialogueManager global
        GameObject managerObj = new GameObject("DialogueManager");
        DialogueManager manager = managerObj.AddComponent<DialogueManager>();
        manager.dialogueUI = panelObj;
        manager.nameText = nameText;
        manager.dialogueText = dialogText;
        manager.portraitImage = portraitImg;
        manager.interactPrompt = interactPromptObj;

        // Limpiar seleccion y mostrar mensaje
        Selection.activeGameObject = managerObj;
        Debug.Log("UI de diálogos generada correctamente.");
    }
}
