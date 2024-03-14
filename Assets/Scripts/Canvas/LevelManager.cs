using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private Animator _animator;

    private void Awake()
    {
        Instance = this;
    }

    public void FadeToTargetScene(string target)
    {
        StartCoroutine(FadeToTargetSceneCoroutine(target));  
    }

    IEnumerator FadeToTargetSceneCoroutine(string target)
    {
        _animator.SetTrigger("FadeOut");
        for(float t = 0; t < 1f; t += Time.unscaledDeltaTime)
        {
            yield return null;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(target);
    }
}
