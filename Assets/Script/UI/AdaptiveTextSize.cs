using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AdaptiveTextSize : MonoBehaviour
{

    public float baseFontSize = 36f;

    public float baseScreenWidth = 1920f;

    public float minFontSize = 18f;

    public float maxFontSize = 48f;

    private TextMeshProUGUI tmpText;
    private RectTransform rectTransform;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        UpdateFontSize();
    }

    void Update()
    {

    }

    public void UpdateFontSize()
    {

        float currentScreenWidth = Screen.width;

        float targetFontSize = baseFontSize * (currentScreenWidth / baseScreenWidth);

        targetFontSize = Mathf.Clamp(targetFontSize, minFontSize, maxFontSize);

        tmpText.fontSize = targetFontSize;

        tmpText.lineSpacing = targetFontSize * 0.3f;

        tmpText.ForceMeshUpdate();
    }
}