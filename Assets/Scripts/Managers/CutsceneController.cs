using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    [Header("Fade")]
    public Image fadeOverlay;
    public float fadeDuration = 0.5f;

    [Header("Cutscene")]
    public Image firstImage;
    public Image secondImage;

    [Header("Painéis")]
    public GameObject painelInicial;
    public GameObject painelFinal1;

    [Header("Botões")]
    public Button avancarButton;
    public Button botaoParaDesativar;

    private int step = 0;
    private bool canAdvance = false;
    private CanvasGroup fadeCanvasRenderer;

    private void Awake()
    {
        if (fadeOverlay == null)
        {
            Debug.LogError("fadeOverlay não está atribuído!");
            return;
        }

        // Pega o CanvasRenderer
        fadeCanvasRenderer = fadeOverlay.GetComponent<CanvasGroup>();
        if (fadeCanvasRenderer != null)
        {
            Debug.Log("Erro pra ver");
        }

        // Verifica se tem sprite
        if (fadeOverlay.sprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            fadeOverlay.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        }

        fadeOverlay.type = Image.Type.Simple;
        fadeOverlay.preserveAspect = false;

        // Configura o overlay para cobrir a tela inteira
        RectTransform rt = fadeOverlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.enabled = true;
        fadeOverlay.raycastTarget = true;

        // Verifica Canvas
        Canvas fadeCanvas = fadeOverlay.GetComponentInParent<Canvas>();
        if (fadeCanvas != null)
        {
            fadeCanvas.overrideSorting = true;
            fadeCanvas.sortingOrder = 9999;
        }

        // Começa totalmente preto
        SetFadeAlpha(1f);

        Canvas.ForceUpdateCanvases();
    }

    private void Start()
    {
        firstImage.gameObject.SetActive(false);
        secondImage.gameObject.SetActive(false);

        if (avancarButton)
        {
            avancarButton.onClick.RemoveAllListeners();
            avancarButton.onClick.AddListener(OnAdvancePressed);
            avancarButton.gameObject.SetActive(false);
        }

        StartCoroutine(InitialFadeSequence());
    }

    private IEnumerator InitialFadeSequence()
    {
        yield return new WaitForEndOfFrame();

        fadeOverlay.transform.SetAsLastSibling();

        yield return StartCoroutine(Fade(1, 0, fadeDuration));
    }

    public void StartCutscene()
    {
        fadeOverlay.transform.SetAsLastSibling();
        if (painelInicial) painelInicial.SetActive(false);
        step = 0;
        ShowStep();
    }

    private void ShowStep()
    {
        if (step == 0)
            StartCoroutine(FadeSequence(firstImage));
        else if (step == 1)
            StartCoroutine(FadeSequence(secondImage, firstImage, true));
        else
            StartCoroutine(EndCutscene());
    }

    private IEnumerator FadeSequence(Image showImage, Image hideImage = null, bool deactivateHide = false)
    {
        canAdvance = false;

        fadeOverlay.transform.SetAsLastSibling();

        // Escurece
        yield return StartCoroutine(Fade(0, 1, fadeDuration));

        // Troca imagens
        if (hideImage != null && deactivateHide) hideImage.gameObject.SetActive(false);
        showImage.gameObject.SetActive(true);

        // Clareia
        yield return StartCoroutine(Fade(1, 0, fadeDuration));

        if (avancarButton)
        {
            avancarButton.gameObject.SetActive(true);
            avancarButton.interactable = true;
        }

        canAdvance = true;
    }

    public void OnAdvancePressed()
    {
        if (!canAdvance) return;

        Debug.Log("Botão avançar pressionado!");

        if (avancarButton)
        {
            avancarButton.gameObject.SetActive(false);
            avancarButton.interactable = false;
        }

        step++;
        ShowStep();
    }

    private IEnumerator EndCutscene()
    {
        canAdvance = false;

        fadeOverlay.transform.SetAsLastSibling();

        yield return StartCoroutine(Fade(0, 1, fadeDuration));

        firstImage.gameObject.SetActive(false);
        secondImage.gameObject.SetActive(false);

        if (painelFinal1) painelFinal1.SetActive(true);
        if (botaoParaDesativar) botaoParaDesativar.gameObject.SetActive(false);

        fadeOverlay.raycastTarget = false;

        yield return StartCoroutine(Fade(1, 0, fadeDuration));
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(start, end, t);

            SetFadeAlpha(alpha);

            yield return null;
        }

        fadeOverlay.raycastTarget = (end > 0.9f);

        SetFadeAlpha(end);
    }

    // Método centralizado para mudar o alpha e forçar atualização
    private void SetFadeAlpha(float alpha)
    {
        Color color = fadeOverlay.color;
        color.a = alpha;
        fadeOverlay.color = color;

        // Força atualização do CanvasRenderer
        if (fadeCanvasRenderer != null)
        {
            fadeCanvasRenderer.alpha = alpha;
        }

        // Marca como dirty para forçar redesenho
        fadeOverlay.SetAllDirty();
    }
}