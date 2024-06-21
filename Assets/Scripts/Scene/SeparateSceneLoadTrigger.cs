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
            SwitchScene(0.5f, 0.5f);
        }
    }

    public void SwitchScene(float fadeDuration, float fadeInDelay)
    {
        if(transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }

        if (!fadeCoroutineStarted)
        {
            fadeCoroutineStarted = true;

            StartCoroutine(SwitchSceneCoroutine(fadeDuration, fadeInDelay));
        }
    }

    IEnumerator SwitchSceneCoroutine(float fadeDuration, float fadeInDelay)
    {
        PlayerController.Instance.CanMove = false;

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

        PlayerController.Instance.transform.position = Vector3.zero;
        CameraMovement.Instance.InstantlyFixPosition();

        if (transform.parent == null)
        {
            transform.position = Vector3.one * 1000f;
        }
        else
        {
            transform.root.position = Vector3.one * 1000f;
        }

        for (float timer = 0; timer < fadeDuration; timer += Time.unscaledDeltaTime)
        {
            _fadePanelImage.color = Color.Lerp(Color.black, Color.clear, timer / fadeDuration);
            yield return null;
        }
        _fadePanelImage.color = Color.clear;
        _fadeCanvasObject.SetActive(false);

        PlayerController.Instance.CanMove = true;

        PlayerPrefs.SetInt("AtBoss", 1);

        if (transform.parent == null)
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
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
