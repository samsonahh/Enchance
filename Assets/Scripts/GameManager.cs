using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State;

    public static event Action<GameState> OnGameStateChanged;

    #region Singletons
    [Header("Singletons")]
    [SerializeField] private GameObject _camera;
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _abilityCanvas;
    [SerializeField] private GameObject _playerCanvas;
    [SerializeField] private MenuCanvasManager _menuCanvas;
    #endregion

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(_camera);
        DontDestroyOnLoad(_player);
        DontDestroyOnLoad(_abilityCanvas);
        DontDestroyOnLoad(_playerCanvas);
        DontDestroyOnLoad(_menuCanvas.gameObject);

        if(LevelManager.Instance != null)
        {
            LevelManager.Instance.ForceChangeToTargetScene("PracticeRange");
        }
    }

    private void Start()
    {
        UpdateGameState(GameState.Playing);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _menuCanvas.ResetMenus();
            UpdateGameState(State == GameState.Paused ? GameState.Playing : GameState.Paused);
            _menuCanvas.gameObject.SetActive(State == GameState.Paused);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            LevelManager.Instance.FadeToTargetScene("PracticeRange");
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            LevelManager.Instance.FadeToTargetScene("Game");
        }
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                GameObject descPanel = GameObject.Find("AbilityDescPanel");
                if(descPanel != null)
                {
                    descPanel.SetActive(false);
                }
                Time.timeScale = 0f;
                break;
            case GameState.Dead:
                Time.timeScale = 0f;
                LevelManager.Instance.FadeToTargetScene("Game");
                break;
            case GameState.Win:
                Time.timeScale = 0f;
                LevelManager.Instance.FadeToTargetScene("Menu");
                Destroy(gameObject);
                Destroy(_camera);
                Destroy(_player);
                Destroy(_abilityCanvas);
                Destroy(_playerCanvas);
                Destroy(_menuCanvas.gameObject);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }
}

public enum GameState
{
    Playing,
    Paused,
    Dead,
    Win
}