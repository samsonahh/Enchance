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
        for(int i = 0; i < 3; i++)
        {
            while (true)
            {
                LevelUpReward generatedReward = LevelUpManager.Instance.GenerateRandomAvailableReward();

                if (generatedReward == LevelUpReward.NoMoreRewards)
                {
                    continue;
                }

                for (int j = 0; j < 3; j++)
                {
                    if(_cards[j] != null)
                    {
                        if(_cards[j].Reward == generatedReward)
                        {
                            continue;
                        }
                    }
                }

                _cards[i] = Instantiate(_cardPrefab, _cardPositions[i]);

                _cards[i].CardName.text = LevelUpManager.Instance.RewardDetailsArray[(int)generatedReward].Name;
                _cards[i].CardImage.sprite = LevelUpManager.Instance.RewardDetailsArray[(int)generatedReward].Sprite;
                _cards[i].CardDescription.text = LevelUpManager.Instance.RewardDetailsArray[(int)generatedReward].Description;
                _cards[i].Reward = generatedReward;
                break;
            }
        }
    }

    private void RemoveAllCards()
    {
        for(int i = 0; i < 3; i++)
        {
            Destroy(_cards[i].gameObject);
            _cards[i] = null;
        }
    }
}
