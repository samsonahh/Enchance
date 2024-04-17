using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpCanvasManager : MonoBehaviour
{
    [SerializeField] private LevelUpCardDescriptionHandler _cardPrefab;

    [SerializeField] private Transform[] _cardPositions = new Transform[3];
    private LevelUpCardDescriptionHandler[] _cards = new LevelUpCardDescriptionHandler[3];

    private void OnEnable()
    {
        GenerateThreeRandomCards();
    }

    private void OnDisable()
    {
        RemoveAllCards();
    }

    private void GenerateThreeRandomCards()
    {
        LevelUpReward[] randomRewards = LevelUpManager.Instance.GenerateThreeRandomAvailableRewards();

        int noRewardCounter = 0;

        for (int i = 0; i < 3; i++)
        {
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

        if(noRewardCounter == 3)
        {
            GameManager.Instance.UpdateGameState(GameState.Playing);
        }
    }

    private void RemoveAllCards()
    {
        for(int i = 0; i < 3; i++)
        {
            if (_cards[i] == null) continue;

            Destroy(_cards[i].gameObject);
            _cards[i] = null;
        }
    }
}
