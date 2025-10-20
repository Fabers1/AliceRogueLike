using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneChanger : MonoBehaviour
{
    [Header("References")]
    public FadeController fadeController;

    [Header("Settings")]
    public float delayBeforeLoad = 0.2f;

    [Header("Scene to Load")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneToLoad;
#else
    [SerializeField] private string sceneName;
#endif

    public void ChangeScene()
    {
        string sceneNameToLoad = GetSceneName();
        StartCoroutine(ChangeSceneRoutine(sceneNameToLoad));
    }

    private string GetSceneName()
    {
#if UNITY_EDITOR
        return sceneToLoad != null ? sceneToLoad.name : string.Empty;
#else
        return sceneName;
#endif
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
