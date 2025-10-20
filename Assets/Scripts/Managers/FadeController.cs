using UnityEngine;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (fadeCanvas == null)
            fadeCanvas = GetComponent<CanvasGroup>();

        // Garante que o overlay cubra a tela inteira
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void Start()
    {
        // Começa com fade in (tela preta → visível)
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        fadeCanvas.blocksRaycasts = true;
        fadeCanvas.alpha = 1;

        while (fadeCanvas.alpha > 0)
        {
            fadeCanvas.alpha -= Time.deltaTime / fadeDuration;
            yield return null;
        }

        fadeCanvas.alpha = 0;
        fadeCanvas.blocksRaycasts = false;
    }

    public IEnumerator FadeOut()
    {
        fadeCanvas.blocksRaycasts = true;

        while (fadeCanvas.alpha < 1)
        {
            fadeCanvas.alpha += Time.deltaTime / fadeDuration;
            yield return null;
        }

        fadeCanvas.alpha = 1;
    }
}
