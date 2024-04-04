using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private GameObject _fadeCanvas;
    [SerializeField] private Image _fadeImage;

    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if(Instance == null)
        {
            Destroy(Instance);
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void ForceChangeToTargetScene(string target)
    {
        SceneManager.LoadScene(target);
    }

    public void FadeToTargetScene(string target)
    {
        _fadeCoroutine = StartCoroutine(FadeToTargetSceneCoroutine(target));  
    }

    IEnumerator FadeToTargetSceneCoroutine(string target)
    {
        _fadeCanvas.SetActive(true);

        for(float t = 0; t < 1f; t += Time.unscaledDeltaTime)
        {
            _fadeImage.color = Color.Lerp(Color.clear, Color.black, t);
            yield return null;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(target);

        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
            _fadeImage.color = Color.Lerp(Color.black, Color.clear, t);
            yield return null;
        }

        _fadeCanvas.SetActive(false);
        _fadeCoroutine = null;
    }
}
