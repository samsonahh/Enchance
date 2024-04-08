using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State;

    public static event Action<GameState> OnGameStateChanged;

    #region Singletons
    [Header("Singletons")]
    [SerializeField] private MenuCanvasManager _menuCanvas;
    #endregion

    #region Global
    [Header("Global")]
    public Color GreenHealthColor;
    public Color YellowHealthColor;
    public Color RedHealthColor;
    public Color EntityBurningColor;
    #endregion

    private void Awake()
    {
        Instance = this;
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
                SceneManager.LoadScene("Menu");
                break;
            case GameState.Win:
                Time.timeScale = 0f;
                SceneManager.LoadScene("Menu");
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