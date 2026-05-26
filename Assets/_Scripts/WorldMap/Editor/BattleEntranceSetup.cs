using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BattleEntranceSetup : EditorWindow
{
    [MenuItem("Tools/Math Heroes/Crear UI de Entrada de Batalla")]
    public static void CreateBattleEntranceUI()
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
            canvasObj = new GameObject("BattleEntranceCanvas", typeof(RectTransform));
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 45; // Justo debajo de los diálogos (50)
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("Creado nuevo Canvas para la entrada de batalla.");
        }

        // Eliminar duplicados anteriores en la escena
        Transform existingModal = canvasObj.transform.Find("BattleEntranceModal");
        if (existingModal != null)
        {
            DestroyImmediate(existingModal.gameObject);
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

        // 2. Crear el Panel del Modal (Fondo oscuro semitransparente que cubre la pantalla)
        GameObject modalBgObj = CreateUIObject("BattleEntranceModal", canvasObj.transform);
        RectTransform modalBgRect = modalBgObj.GetComponent<RectTransform>();
        modalBgRect.anchorMin = Vector2.zero;
        modalBgRect.anchorMax = Vector2.one;
        modalBgRect.offsetMin = Vector2.zero;
        modalBgRect.offsetMax = Vector2.zero;
        
        Image modalBgImg = modalBgObj.AddComponent<Image>();
        modalBgImg.color = new Color(0f, 0f, 0f, 0.6f); // Fondo atenuado

        // 3. Crear el Contenedor del Cuadro de Diálogo (Centrado)
        GameObject boxObj = CreateUIObject("EntranceBox", modalBgObj.transform);
        RectTransform boxRect = boxObj.GetComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.anchoredPosition = Vector2.zero;
        boxRect.sizeDelta = new Vector2(700, 450);
        
        Image boxImg = boxObj.AddComponent<Image>();
        boxImg.color = new Color(0.12f, 0.14f, 0.2f, 0.95f); // Gris oscuro RPG
        
        // Agregar un borde sutil
        GameObject borderObj = CreateUIObject("Border", boxObj.transform);
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(4, 4);
        borderRect.offsetMax = new Vector2(-4, -4);
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(1f, 0.84f, 0f, 0.4f); // Borde dorado
        borderImg.raycastTarget = false;

        // 4. Título (Nombre de la zona/batalla)
        GameObject titleObj = CreateUIObject("TitleText", boxObj.transform);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(-40, 60);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Zona de Batalla";
        titleText.fontSize = 42;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(1f, 0.84f, 0f); // Dorado brillante
        titleText.alignment = TextAlignmentOptions.Center;

        // Separador visual
        GameObject sepObj = CreateUIObject("Separator", boxObj.transform);
        RectTransform sepRect = sepObj.GetComponent<RectTransform>();
        sepRect.anchorMin = new Vector2(0.5f, 1);
        sepRect.anchorMax = new Vector2(0.5f, 1);
        sepRect.anchoredPosition = new Vector2(0, -100);
        sepRect.sizeDelta = new Vector2(550, 4);
        Image sepImg = sepObj.AddComponent<Image>();
        sepImg.color = new Color(1f, 1f, 1f, 0.15f);

        // 4.5. Panel de Fondo para el Texto (Opcional, para legibilidad sobre Sprites)
        GameObject textPanelObj = CreateUIObject("TextBackgroundPanel", boxObj.transform);
        RectTransform textPanelRect = textPanelObj.GetComponent<RectTransform>();
        textPanelRect.anchorMin = new Vector2(0, 0);
        textPanelRect.anchorMax = new Vector2(1, 1);
        textPanelRect.offsetMin = new Vector2(30, 120); // Libre para botones
        textPanelRect.offsetMax = new Vector2(-30, -110); // Libre para título
        Image textPanelImg = textPanelObj.AddComponent<Image>();
        textPanelImg.color = new Color(0f, 0f, 0f, 0.4f); // Fondo oscuro de contraste

        // 5. Detalles de la batalla: Operaciones usadas (Hijo del Panel de Fondo)
        GameObject opsObj = CreateUIObject("OperationsText", textPanelObj.transform);
        RectTransform opsRect = opsObj.GetComponent<RectTransform>();
        opsRect.anchorMin = new Vector2(0, 0.5f);
        opsRect.anchorMax = new Vector2(1, 0.5f);
        opsRect.pivot = new Vector2(0.5f, 0.5f);
        opsRect.anchoredPosition = new Vector2(0, 35);
        opsRect.sizeDelta = new Vector2(-20, 80);
        
        TextMeshProUGUI opsText = opsObj.AddComponent<TextMeshProUGUI>();
        opsText.text = "Operaciones: Suma (+), Resta (-)";
        opsText.fontSize = 28;
        opsText.color = Color.white;
        opsText.alignment = TextAlignmentOptions.Center;
        opsText.enableWordWrapping = true;

        // 6. Detalles de la batalla: Recompensa de XP (Hijo del Panel de Fondo)
        GameObject xpObj = CreateUIObject("XpText", textPanelObj.transform);
        RectTransform xpRect = xpObj.GetComponent<RectTransform>();
        xpRect.anchorMin = new Vector2(0, 0.5f);
        xpRect.anchorMax = new Vector2(1, 0.5f);
        xpRect.pivot = new Vector2(0.5f, 0.5f);
        xpRect.anchoredPosition = new Vector2(0, -35);
        xpRect.sizeDelta = new Vector2(-20, 50);
        
        TextMeshProUGUI xpText = xpObj.AddComponent<TextMeshProUGUI>();
        xpText.text = "Recompensa: ~40 XP";
        xpText.fontSize = 28;
        xpText.color = new Color(0.7f, 0.85f, 1f); // Azul claro informativo
        xpText.alignment = TextAlignmentOptions.Center;

        // 7. Botón Cancelar (Izquierda)
        GameObject btnCancelObj = CreateUIObject("CancelButton", boxObj.transform);
        RectTransform btnCancelRect = btnCancelObj.GetComponent<RectTransform>();
        btnCancelRect.anchorMin = new Vector2(0.25f, 0);
        btnCancelRect.anchorMax = new Vector2(0.25f, 0);
        btnCancelRect.pivot = new Vector2(0.5f, 0);
        btnCancelRect.anchoredPosition = new Vector2(0, 40);
        btnCancelRect.sizeDelta = new Vector2(200, 60);
        
        Image btnCancelImg = btnCancelObj.AddComponent<Image>();
        btnCancelImg.color = new Color(0.35f, 0.35f, 0.4f); // Gris neutro
        Button btnCancel = btnCancelObj.AddComponent<Button>();
        btnCancel.targetGraphic = btnCancelImg;
        btnCancelObj.AddComponent<UIButtonEffects>();

        GameObject txtCancelObj = CreateUIObject("Text", btnCancelObj.transform);
        RectTransform txtCancelRect = txtCancelObj.GetComponent<RectTransform>();
        txtCancelRect.anchorMin = Vector2.zero;
        txtCancelRect.anchorMax = Vector2.one;
        txtCancelRect.offsetMin = Vector2.zero;
        txtCancelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI txtCancel = txtCancelObj.AddComponent<TextMeshProUGUI>();
        txtCancel.text = "Volver";
        txtCancel.fontSize = 26;
        txtCancel.color = Color.white;
        txtCancel.alignment = TextAlignmentOptions.CenterGeoAligned;
        txtCancel.raycastTarget = false;

        // 8. Botón Entrar (Derecha)
        GameObject btnEnterObj = CreateUIObject("EnterButton", boxObj.transform);
        RectTransform btnEnterRect = btnEnterObj.GetComponent<RectTransform>();
        btnEnterRect.anchorMin = new Vector2(0.75f, 0);
        btnEnterRect.anchorMax = new Vector2(0.75f, 0);
        btnEnterRect.pivot = new Vector2(0.5f, 0);
        btnEnterRect.anchoredPosition = new Vector2(0, 40);
        btnEnterRect.sizeDelta = new Vector2(200, 60);
        
        Image btnEnterImg = btnEnterObj.AddComponent<Image>();
        btnEnterImg.color = new Color(0.18f, 0.54f, 0.34f); // Verde bosque
        Button btnEnter = btnEnterObj.AddComponent<Button>();
        btnEnter.targetGraphic = btnEnterImg;
        btnEnterObj.AddComponent<UIButtonEffects>();

        GameObject txtEnterObj = CreateUIObject("Text", btnEnterObj.transform);
        RectTransform txtEnterRect = txtEnterObj.GetComponent<RectTransform>();
        txtEnterRect.anchorMin = Vector2.zero;
        txtEnterRect.anchorMax = Vector2.one;
        txtEnterRect.offsetMin = Vector2.zero;
        txtEnterRect.offsetMax = Vector2.zero;
        TextMeshProUGUI txtEnter = txtEnterObj.AddComponent<TextMeshProUGUI>();
        txtEnter.text = "¡Pelear!";
        txtEnter.fontSize = 26;
        txtEnter.fontStyle = FontStyles.Bold;
        txtEnter.color = Color.white;
        txtEnter.alignment = TextAlignmentOptions.CenterGeoAligned;
        txtEnter.raycastTarget = false;

        // 9. Configurar el componente controlador en la raíz
        BattleEntranceUI manager = canvasObj.GetComponent<BattleEntranceUI>();
        if (manager == null)
        {
            manager = canvasObj.AddComponent<BattleEntranceUI>();
        }
        
        // Usar Reflection para asignar los campos serializados privados
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("modalPanel").objectReferenceValue = modalBgObj;
        so.FindProperty("titleText").objectReferenceValue = titleText;
        so.FindProperty("operationsText").objectReferenceValue = opsText;
        so.FindProperty("xpText").objectReferenceValue = xpText;
        so.FindProperty("enterButton").objectReferenceValue = btnEnter;
        so.FindProperty("cancelButton").objectReferenceValue = btnCancel;
        so.ApplyModifiedProperties();

        Selection.activeGameObject = canvasObj;
        Debug.Log("UI de Entrada de Batalla creada y configurada correctamente en la escena.");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}
