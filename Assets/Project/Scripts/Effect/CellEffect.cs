using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CellEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image targetImage;
    [SerializeField] private RectTransform targetRect;

    [Header("Mix Effect")]
    [SerializeField] private Color mixFlashColor = new Color32(255, 255, 255, 255);
    [SerializeField] private float mixScale = 1.18f;
    [SerializeField] private float mixDuration = 0.16f;

    [Header("Ash Effect")]
    [SerializeField] private Color ashFlashColor = new Color32(90, 65, 45, 255);
    [SerializeField] private float ashScale = 1.25f;
    [SerializeField] private float ashDuration = 0.22f;

    [Header("Clear Effect")]
    [SerializeField] private Color clearFlashColor = new Color32(255, 255, 255, 255);
    [SerializeField] private float clearScale = 1.3f;
    [SerializeField] private float clearDuration = 0.22f;

    private Coroutine currentRoutine;
    private Vector3 originalScale = Vector3.one;
    private Cell ownerCell;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetRect == null)
        {
            targetRect = GetComponent<RectTransform>();
        }

        ownerCell = GetComponent<Cell>();

        if (targetRect != null)
        {
            originalScale = targetRect.localScale;
        }
    }

    public void PlayMixEffect()
    {
        PlayFlashEffect(mixFlashColor, mixScale, mixDuration);
    }

    public void PlayAshEffect()
    {
        PlayFlashEffect(ashFlashColor, ashScale, ashDuration);
    }

    public void PlayClearEffect(Color fromColor, Color toColor)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ClearEffectRoutine(fromColor, toColor));
    }

    private void PlayFlashEffect(Color flashColor, float scaleMultiplier, float duration)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(FlashEffectRoutine(flashColor, scaleMultiplier, duration));
    }

    private IEnumerator FlashEffectRoutine(Color flashColor, float scaleMultiplier, float duration)
    {
        if (targetImage == null || targetRect == null)
            yield break;

        Color startColor = targetImage.color;
        Vector3 startScale = originalScale;
        Vector3 peakScale = originalScale * scaleMultiplier;

        float halfDuration = duration * 0.5f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);

            targetRect.localScale = Vector3.Lerp(startScale, peakScale, t);
            targetImage.color = Color.Lerp(startColor, flashColor, t);

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);

            targetRect.localScale = Vector3.Lerp(peakScale, startScale, t);
            targetImage.color = Color.Lerp(flashColor, startColor, t);

            yield return null;
        }

        targetRect.localScale = originalScale;
        targetImage.color = startColor;
        currentRoutine = null;
    }

    private IEnumerator ClearEffectRoutine(Color fromColor, Color toColor)
    {
        if (targetImage == null || targetRect == null)
            yield break;

        Vector3 startScale = originalScale;
        Vector3 peakScale = originalScale * clearScale;

        float halfDuration = clearDuration * 0.5f;
        float timer = 0f;

        targetImage.color = fromColor;
        targetRect.localScale = startScale;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);

            targetRect.localScale = Vector3.Lerp(startScale, peakScale, t);
            targetImage.color = Color.Lerp(fromColor, clearFlashColor, t);

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);

            targetRect.localScale = Vector3.Lerp(peakScale, startScale, t);
            targetImage.color = Color.Lerp(clearFlashColor, toColor, t);

            yield return null;
        }

        targetRect.localScale = originalScale;
        targetImage.color = toColor;

        if (ownerCell != null)
        {
            ownerCell.ForceRefreshVisual();
        }

        currentRoutine = null;
    }
}