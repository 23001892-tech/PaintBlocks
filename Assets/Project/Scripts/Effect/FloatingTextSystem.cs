using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingTextSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform textParent;

    [Header("Default Text Settings")]
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private int fontSize = 38;
    [SerializeField] private Color normalScoreColor = new Color32(255, 255, 255, 255);
    [SerializeField] private Color targetScoreColor = new Color32(80, 255, 220, 255);
    [SerializeField] private Color comboColor = new Color32(255, 210, 80, 255);
    [SerializeField] private Color bombColor = new Color32(255, 120, 255, 255);

    [Header("Animation")]
    [SerializeField] private float lifeTime = 0.75f;
    [SerializeField] private float moveUpDistance = 85f;
    [SerializeField] private float scaleFrom = 0.75f;
    [SerializeField] private float scaleTo = 1.15f;

    [Header("Positions")]
    [SerializeField] private Vector2 scorePopupPosition = new Vector2(0f, 130f);
    [SerializeField] private Vector2 comboPopupPosition = new Vector2(0f, 190f);
    [SerializeField] private Vector2 bombPopupPosition = new Vector2(0f, 80f);

    private RectTransform canvasRect;

    private void Awake()
    {
        if (rootCanvas == null)
        {
            rootCanvas = FindAnyObjectByType<Canvas>();
        }

        if (rootCanvas != null)
        {
            canvasRect = rootCanvas.GetComponent<RectTransform>();
        }

        if (textParent == null && rootCanvas != null)
        {
            textParent = rootCanvas.transform as RectTransform;
        }
    }

    public void ShowScoreText(int amount, bool isTarget)
    {
        if (amount <= 0)
            return;

        string message = isTarget ? $"+{amount} TARGET!" : $"+{amount}";
        Color color = isTarget ? targetScoreColor : normalScoreColor;

        ShowText(message, scorePopupPosition, color, fontSize);
    }

    public void ShowComboText(int comboStreak)
    {
        if (comboStreak < 2)
            return;

        ShowText($"COMBO x{comboStreak}", comboPopupPosition, comboColor, fontSize + 4);
    }

    public void ShowBombText(int amount)
    {
        if (amount <= 0)
            return;

        ShowText($"BOOM +{amount}", bombPopupPosition, bombColor, fontSize + 6);
    }

    public void ShowText(string message, Vector2 anchoredPosition, Color color, int size)
    {
        if (textParent == null)
            return;

        GameObject textObject = new GameObject("FloatingText");
        textObject.transform.SetParent(textParent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(520f, 90f);

        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.raycastTarget = false;
        text.enableWordWrapping = false;

        if (fontAsset != null)
        {
            text.font = fontAsset;
        }

        StartCoroutine(AnimateText(text, rectTransform, anchoredPosition, color));
    }

    private IEnumerator AnimateText(TMP_Text text, RectTransform rectTransform, Vector2 startPosition, Color startColor)
    {
        if (text == null || rectTransform == null)
            yield break;

        float timer = 0f;

        Vector2 endPosition = startPosition + new Vector2(0f, moveUpDistance);
        Vector3 startScale = Vector3.one * scaleFrom;
        Vector3 endScale = Vector3.one * scaleTo;

        rectTransform.localScale = startScale;

        while (timer < lifeTime)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / lifeTime);
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, smoothT);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, smoothT);

            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(1f, 0f, t);
            text.color = currentColor;

            yield return null;
        }

        if (rectTransform != null)
        {
            Destroy(rectTransform.gameObject);
        }
    }
}