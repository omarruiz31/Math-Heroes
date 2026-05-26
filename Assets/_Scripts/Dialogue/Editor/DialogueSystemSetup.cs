using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class DialogueSystemSetup : EditorWindow
{
    [MenuItem("Tools/Math Heroes/Crear UI de Dialogos")]
    public static void CreateDialogueUI()
    {
        // 1. Buscar Canvas existente en la escena o crear uno nuevo
        Canvas canvas = FindAnyObjectByType<Canvas>();
        GameObject canvasObj;
        
        if (canvas != null)
        {
            canvasObj = canvas.gameObject;
            Debug.Log($"Usando Canvas existente: {canvasObj.name}");
        }
        else
        {
            canvasObj = new GameObject("DialogueCanvas", typeof(RectTransform));
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50; // Para que aparezca encima de todo
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("Creado nuevo Canvas para diálogos.");
        }

        // Eliminar duplicados anteriores en la escena
        Transform existingPanel = canvasObj.transform.Find("DialoguePanel");
        if (existingPanel != null)
        {
            DestroyImmediate(existingPanel.gameObject);
        }

        Transform existingPrompt = canvasObj.transform.Find("InteractPrompt");
        if (existingPrompt != null)
        {
            DestroyImmediate(existingPrompt.gameObject);
        }

        // 1.5. Asegurar que haya un EventSystem en la escena para procesar los clics
        UnityEngine.EventSystems.EventSystem eventSystem = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            #if ENABLE_INPUT_SYSTEM
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            #else
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            #endif
            
            Debug.Log("Creado EventSystem automático en la escena.");
        }

        // 2. Crear el Panel Principal (Fondo oscuro opcional)
        GameObject panelObj = CreateUIObject("DialoguePanel", canvasObj.transform);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.5f); // Fondo semitransparente

        // 3. Crear el Cuadro de Texto de Diálogo (Bottom)
        GameObject boxObj = CreateUIObject("DialogueBox", panelObj.transform);
        RectTransform boxRect = boxObj.GetComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0);
        boxRect.anchorMax = new Vector2(0.5f, 0);
        boxRect.pivot = new Vector2(0.5f, 0);
        boxRect.anchoredPosition = new Vector2(0, 50);
        boxRect.sizeDelta = new Vector2(1400, 250);
        Image boxBg = boxObj.AddComponent<Image>();
        boxBg.color = new Color(0.12f, 0.14f, 0.2f, 0.95f); // Color oscuro RPG

        // 4. Crear la Imagen del Retrato
        GameObject portraitObj = CreateUIObject("PortraitImage", boxObj.transform);
        RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0, 0.5f);
        portraitRect.anchorMax = new Vector2(0, 0.5f);
        portraitRect.pivot = new Vector2(0, 0.5f);
        portraitRect.anchoredPosition = new Vector2(20, 0);
        portraitRect.sizeDelta = new Vector2(200, 200);
        Image portraitImg = portraitObj.AddComponent<Image>();
        portraitImg.preserveAspect = true;

        // 5. Crear el Cuadro del Nombre
        GameObject nameBoxObj = CreateUIObject("NameBox", boxObj.transform);
        RectTransform nameBoxRect = nameBoxObj.GetComponent<RectTransform>();
        nameBoxRect.anchorMin = new Vector2(0, 1);
        nameBoxRect.anchorMax = new Vector2(0, 1);
        nameBoxRect.pivot = new Vector2(0, 0);
        nameBoxRect.anchoredPosition = new Vector2(20, 10);
        nameBoxRect.sizeDelta = new Vector2(300, 60);
        Image nameBoxBg = nameBoxObj.AddComponent<Image>();
        nameBoxBg.color = new Color(0.12f, 0.14f, 0.2f, 0.95f); // Fondo oscuro a juego

        // 6. Texto del Nombre
        GameObject nameTextObj = CreateUIObject("NameText", nameBoxObj.transform);
        RectTransform nameTextRect = nameTextObj.GetComponent<RectTransform>();
        nameTextRect.anchorMin = new Vector2(0, 0);
        nameTextRect.anchorMax = new Vector2(1, 1);
        nameTextRect.offsetMin = new Vector2(10, 0);
        nameTextRect.offsetMax = new Vector2(-10, 0);
        TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Nombre";
        nameText.fontSize = 36;
        nameText.alignment = TextAlignmentOptions.CenterGeoAligned;
        nameText.color = Color.white;
        nameText.raycastTarget = false;

        // 7. Panel de Fondo para el Texto (Opcional, para legibilidad sobre Sprites)
        GameObject textPanelObj = CreateUIObject("TextBackgroundPanel", boxObj.transform);
        RectTransform textPanelRect = textPanelObj.GetComponent<RectTransform>();
        textPanelRect.anchorMin = new Vector2(0, 0);
        textPanelRect.anchorMax = new Vector2(1, 1);
        textPanelRect.offsetMin = new Vector2(240, 15); // Espacio para el retrato a la izquierda
        textPanelRect.offsetMax = new Vector2(-15, -15);
        Image textPanelImg = textPanelObj.AddComponent<Image>();
        textPanelImg.color = new Color(0f, 0f, 0f, 0.35f); // Negro semi-transparente para dar contraste

        // 7.5. Texto del Diálogo (Hijo del Panel de Fondo)
        GameObject dialogTextObj = CreateUIObject("DialogueText", textPanelObj.transform);
        RectTransform dialogTextRect = dialogTextObj.GetComponent<RectTransform>();
        dialogTextRect.anchorMin = new Vector2(0, 0);
        dialogTextRect.anchorMax = new Vector2(1, 1);
        dialogTextRect.offsetMin = new Vector2(15, 5);
        dialogTextRect.offsetMax = new Vector2(-15, -5);
        TextMeshProUGUI dialogText = dialogTextObj.AddComponent<TextMeshProUGUI>();
        dialogText.text = "Este es un texto de prueba para el diálogo. Aquí aparecerán las instrucciones.";
        dialogText.fontSize = 42;
        dialogText.alignment = TextAlignmentOptions.TopLeft;
        dialogText.color = Color.white; // Blanco para máxima legibilidad
        dialogText.enableWordWrapping = true;
        dialogText.raycastTarget = false;

        // 7.6. Aviso de Interacción (Interact Prompt)
        GameObject interactPromptObj = CreateUIObject("InteractPrompt", canvasObj.transform);
        RectTransform promptRect = interactPromptObj.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0);
        promptRect.anchorMax = new Vector2(0.5f, 0);
        promptRect.pivot = new Vector2(0.5f, 0);
        promptRect.anchoredPosition = new Vector2(0, 50);
        promptRect.sizeDelta = new Vector2(400, 60);
        Image promptBg = interactPromptObj.AddComponent<Image>();
        promptBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        GameObject promptTextObj = CreateUIObject("PromptText", interactPromptObj.transform);
        RectTransform promptTextRect = promptTextObj.GetComponent<RectTransform>();
        promptTextRect.anchorMin = Vector2.zero;
        promptTextRect.anchorMax = Vector2.one;
        promptTextRect.offsetMin = Vector2.zero;
        promptTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI promptText = promptTextObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "Presiona [E] para hablar";
        promptText.fontSize = 32;
        promptText.alignment = TextAlignmentOptions.CenterGeoAligned;
        promptText.color = Color.white;
        promptText.raycastTarget = false;
        interactPromptObj.SetActive(false);

        // 8. Crear el DialogueManager global
        GameObject managerObj = new GameObject("DialogueManager");
        DialogueManager manager = managerObj.GetComponent<DialogueManager>();
        if (manager == null)
        {
            manager = managerObj.AddComponent<DialogueManager>();
        }
        manager.dialogueUI = panelObj;
        manager.nameText = nameText;
        manager.dialogueText = dialogText;
        manager.portraitImage = portraitImg;
        manager.interactPrompt = interactPromptObj;
        manager.interactPromptText = promptText;

        // Limpiar seleccion y mostrar mensaje
        Selection.activeGameObject = managerObj;
        Debug.Log("UI de diálogos generada correctamente.");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}
