using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;



public class SceneChanger : MonoBehaviour
{
    [Header("References")]
    public FadeController fadeController;

    [Header("Settings")]
    public float delayBeforeLoad = 0.2f;

    [Header("Scene to Load")]
    [SerializeField] private string sceneName;

    public void ChangeScene()
    {
        fadeController.GetComponent<Image>().color = Color.black;
        string sceneNameToLoad = GetSceneName();
        StartCoroutine(ChangeSceneRoutine(sceneNameToLoad));
    }

    private string GetSceneName()
    {
    return sceneName;
    }

    public IEnumerator ChangeSceneRoutine(string sceneName)
    {
        if (fadeController != null)
            yield return StartCoroutine(fadeController.FadeOut());

        yield return new WaitForSeconds(delayBeforeLoad);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            yield return null;
        }

        if (fadeController != null)
            yield return StartCoroutine(fadeController.FadeIn());
    }
}
