using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Objects")]
    [SerializeField] private GameObject _loadingBarObject;
    [SerializeField] private Slider _loadingBarSlider;
    [SerializeField] private GameObject[] _objectsToHide;

    [Header("Scenes to Load")]
    [SerializeField] private SceneField _persistentGameplay;
    [SerializeField] private SceneField _practiceRange;

    [Header("Needs Tweening")]
    [SerializeField] private Button _startButton;

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        _loadingBarObject.SetActive(false);
    }

    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();

        StartGame();
    }

    public void ClientJoinGame()
    {
        NetworkManager.Singleton.StartClient();

        StartGame();
    }

    public void StartGame()
    {
        HideMenu();

        _loadingBarObject.SetActive(true);

        _scenesToLoad.Add(SceneManager.LoadSceneAsync(_practiceRange));

/*        _scenesToLoad.Add(SceneManager.LoadSceneAsync(_persistentGameplay));
        _scenesToLoad.Add(SceneManager.LoadSceneAsync(_practiceRange, LoadSceneMode.Additive));*/

        StartCoroutine(ProgressLoadingBar());
    }

    private void HideMenu()
    {
        for(int i = 0; i < _objectsToHide.Length; i++)
        {
            _objectsToHide[i].SetActive(false);
        }
    }

    private IEnumerator ProgressLoadingBar()
    {
        float loadProgress = 0f;
        for(int i = 0; i < _scenesToLoad.Count; i++)
        {
            while (!_scenesToLoad[i].isDone)
            {
                loadProgress += _scenesToLoad[i].progress;
                _loadingBarSlider.value = loadProgress / _scenesToLoad.Count;
                yield return null;
            }
        }
    }

    public void ScaleStartButton(float scale)
    {
        LeanTween.scale(_startButton.gameObject, new Vector3(scale, scale, scale), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    }
}
