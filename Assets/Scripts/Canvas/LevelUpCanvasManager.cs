using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpCanvasManager : MonoBehaviour
{
    public static LevelUpCanvasManager Instance;

    [SerializeField] private LevelUpCardDescriptionHandler _cardPrefab;

    [SerializeField] private Transform[] _cardPositions = new Transform[3];
    private LevelUpCardDescriptionHandler[] _cards = new LevelUpCardDescriptionHandler[3];

    private void Awake()
    {
        Instance = this;

        GameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(GameState state)
    {
        if(PlayerController.Instance.QueuedLevels == 0)
        {
            RemoveAllCards();
        }
    }

    private void OnEnable()
    {
        GenerateThreeRandomCards();
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }

    private void GenerateThreeRandomCards()
    {
        int noRewardCounter = 0;

        if (PlayerPrefs.HasKey("CurrentLevelReward0"))
        {
            LevelUpReward[] prevRewards = { (LevelUpReward)PlayerPrefs.GetInt($"CurrentLevelReward0"), (LevelUpReward)PlayerPrefs.GetInt($"CurrentLevelReward1"), (LevelUpReward)PlayerPrefs.GetInt($"CurrentLevelReward2") };

            for (int i = 0; i < 3; i++)
            {
                PlayerPrefs.SetInt($"CurrentLevelReward{i}", (int)prevRewards[i]);

                if (prevRewards[i] == LevelUpReward.NoMoreRewards)
                {
                    noRewardCounter++;
                    continue;
                }

                _cards[i] = Instantiate(_cardPrefab, _cardPositions[i]);

                _cards[i].CardName.text = LevelUpManager.Instance.Rewards[(int)prevRewards[i]].Name;
                _cards[i].CardImage.sprite = LevelUpManager.Instance.Rewards[(int)prevRewards[i]].Sprite;
                _cards[i].CardDescription.text = LevelUpManager.Instance.Rewards[(int)prevRewards[i]].Description;
                _cards[i].Reward = LevelUpManager.Instance.Rewards[(int)prevRewards[i]].RewardType;

            }
        }
        else
        {
            LevelUpReward[] randomRewards = LevelUpManager.Instance.GenerateThreeRandomAvailableRewards();

            for (int i = 0; i < 3; i++)
            {
                PlayerPrefs.SetInt($"CurrentLevelReward{i}", (int)randomRewards[i]);

                if (randomRewards[i] == LevelUpReward.NoMoreRewards)
                {
                    noRewardCounter++;
                    continue;
                }

                _cards[i] = Instantiate(_cardPrefab, _cardPositions[i]);

                _cards[i].CardName.text = LevelUpManager.Instance.Rewards[(int)randomRewards[i]].Name;
                _cards[i].CardImage.sprite = LevelUpManager.Instance.Rewards[(int)randomRewards[i]].Sprite;
                _cards[i].CardDescription.text = LevelUpManager.Instance.Rewards[(int)randomRewards[i]].Description;
                _cards[i].Reward = LevelUpManager.Instance.Rewards[(int)randomRewards[i]].RewardType;
            }
        }

        if(noRewardCounter == 3)
        {
            GameManager.Instance.UpdateGameState(GameState.Playing);
        }
    }

    public void RemoveAllCards()
    {
        for(int i = 0; i < 3; i++)
        {
            PlayerPrefs.DeleteKey($"CurrentLevelReward{i}");

            if (_cards[i] == null) continue;

            Destroy(_cards[i].gameObject);
            _cards[i] = null;
        }
    }
}
