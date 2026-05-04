using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuNavigation : MonoBehaviour
{
    private const string WorldMapSceneName = "WorldMap";

    private void Awake()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label == null) continue;

            string text = label.text.Trim().ToUpperInvariant();
            if (text == "JUGAR")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Play);
            }
        }
    }

    private void Play()
    {
        SceneManager.LoadScene(WorldMapSceneName);
    }
}
