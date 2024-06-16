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
    }

    private void Start()
    {
        LoadGame();

        UpdateGameState(GameState.Playing);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UpdateGameState(State == GameState.Paused ? GameState.Playing : GameState.Paused);
        }

        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            ResetGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
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

    public void SaveGame()
    {
        // rewards
        PlayerPrefs.SetInt("MoreHPUpgrades", LevelUpManager.Instance.RewardPool[0]);
        PlayerPrefs.SetInt("SpeedUpgrades", LevelUpManager.Instance.RewardPool[1]);
        PlayerPrefs.SetInt("CooldownReductionUpgrades", LevelUpManager.Instance.RewardPool[2]);
        PlayerPrefs.SetInt("HigherChancesUpgrades", LevelUpManager.Instance.RewardPool[3]);
        PlayerPrefs.SetInt("CastTimeReductionUpgrades", LevelUpManager.Instance.RewardPool[4]);

        // stats
        PlayerPrefs.SetInt("PlayerLevel", PlayerController.Instance.CurrentLevel);
        PlayerPrefs.SetInt("PlayerCurrentExperience", PlayerController.Instance.CurrentExp);
        PlayerPrefs.SetInt("PlayerMaxExperience", PlayerController.Instance.ExpToNextLevel);

        // pos
        PlayerPrefs.SetFloat("PlayerPosX", PlayerController.Instance.transform.position.x);
        PlayerPrefs.SetFloat("PlayerPosZ", PlayerController.Instance.transform.position.z);
    }

    public void LoadGame()
    {
        LevelUpManager.Instance.ResetRewardPool();

        if (!PlayerPrefs.HasKey("MoreHPUpgrades"))
        {
            return;
        }

        // rewards
        int[] rewardPool = new int[LevelUpManager.Instance.RewardPool.Length];

        rewardPool[0] = PlayerPrefs.GetInt("MoreHPUpgrades");
        rewardPool[1] = PlayerPrefs.GetInt("SpeedUpgrades");
        rewardPool[2] = PlayerPrefs.GetInt("CooldownReductionUpgrades");
        rewardPool[3] = PlayerPrefs.GetInt("HigherChancesUpgrades");
        rewardPool[4] = PlayerPrefs.GetInt("CastTimeReductionUpgrades");

        LevelUpManager.Instance.LoadRewards(rewardPool);

        // stats
        PlayerController.Instance.CurrentLevel = PlayerPrefs.GetInt("PlayerLevel");
        PlayerController.Instance.CurrentExp = PlayerPrefs.GetInt("PlayerCurrentExperience");
        PlayerController.Instance.ExpToNextLevel = PlayerPrefs.GetInt("PlayerMaxExperience");
        PlayerController.Instance.CurrentHealth = PlayerController.Instance.MaxHealth;

        // position
        PlayerController.Instance.transform.position = new Vector3(PlayerPrefs.GetFloat("PlayerPosX"), 0, PlayerPrefs.GetFloat("PlayerPosZ"));
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();

        SceneManager.LoadScene("Menu");
    }

    public static void BackToMenu()
    {
        Instance.SaveGame();
        SceneManager.LoadScene("Menu");
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