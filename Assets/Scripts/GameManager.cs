using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerController PlayerControllerInstance;
    public AbilityCaster AbilityCasterInstance;

    public GameState State;

    public static event Action<GameState> OnGameStateChanged;

    #region Global
    [Header("Global")]
    public Color GreenHealthColor;
    public Color YellowHealthColor;
    public Color RedHealthColor;
    public Color EntityBurningColor;
    public Color EntityPoisonedColor;
    public Color[] StarColors;
    #endregion

    #region Keys
    [Header("Keys")]
    public int[] BossRoomKeys = { 0, 0, 0 };
    #endregion

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(transform.parent.gameObject);
    }

    private void Start()
    {
        UpdateGameState(GameState.Playing);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UpdateGameState(State == GameState.Paused ? GameState.Playing : GameState.Paused);
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
            case GameState.LevelUpSelect:
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
    LevelUpSelect,
    Dead,
    Win
}