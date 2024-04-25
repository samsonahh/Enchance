using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SeparateSceneLoadTrigger : MonoBehaviour
{
    [SerializeField] private SceneField _sceneToLoad;
    [SerializeField] private SceneField _persistentScene;

    [SerializeField] private GameObject _fadeCanvasObject;
    [SerializeField] private Image _fadePanelImage;
    private bool fadeCoroutineStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            DontDestroyOnLoad(gameObject);

            if (!fadeCoroutineStarted)
            {
                StartCoroutine(SwitchSceneCoroutine(0.5f, 0.5f));
            }
        }
    }

    IEnumerator SwitchSceneCoroutine(float fadeDuration, float fadeInDelay)
    {
        GameManager.Instance.PlayerControllerInstance.CanMove = false;

        _fadeCanvasObject.SetActive(true);
        for(float timer = 0; timer < fadeDuration; timer += Time.unscaledDeltaTime)
        {
            _fadePanelImage.color = Color.Lerp(Color.clear, Color.black, timer / fadeDuration);
            yield return null;
        }
        _fadePanelImage.color = Color.black;

        UnloadScenes();
        yield return new WaitForSeconds(fadeInDelay);
        yield return SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);

        for (float timer = 0; timer < fadeDuration; timer += Time.unscaledDeltaTime)
        {
            _fadePanelImage.color = Color.Lerp(Color.black, Color.clear, timer / fadeDuration);
            yield return null;
        }
        _fadePanelImage.color = Color.clear;
        _fadeCanvasObject.SetActive(false);

        GameManager.Instance.PlayerControllerInstance.CanMove = true;

        Destroy(gameObject);
    }

    private void UnloadScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name != _persistentScene.SceneName)
            {
                SceneManager.UnloadSceneAsync(loadedScene);
            }
        }
    }
}
