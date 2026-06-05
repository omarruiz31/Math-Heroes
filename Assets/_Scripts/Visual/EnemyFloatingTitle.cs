using UnityEngine;
using TMPro;

public class EnemyFloatingTitle : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private int sortingOrder = 100;

    private GameObject titleContainer;
    private TextMeshPro titleText;
    private WorldMapBattleZone zone;

    private void Start()
    {
        zone = GetComponent<WorldMapBattleZone>();
        
        titleContainer = new GameObject("FloatingTitle");
        titleContainer.transform.SetParent(transform);
        titleContainer.transform.localPosition = offset;
        
        titleText = titleContainer.AddComponent<TextMeshPro>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 4;
        titleText.sortingOrder = sortingOrder;
        
        if (fontAsset != null)
        {
            titleText.font = fontAsset;
        }

        if (zone != null && zone.enemy != null)
        {
            titleText.text = $"<color=#FFCC00>Lv.{zone.enemy.enemyLevel}</color> {zone.enemy.enemyName}";
        }
        else
        {
            titleText.text = "";
        }
    }

    private void Update()
    {
        if (titleContainer == null) return;

        float newY = offset.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        titleContainer.transform.localPosition = new Vector3(offset.x, newY, offset.z);
    }
}

