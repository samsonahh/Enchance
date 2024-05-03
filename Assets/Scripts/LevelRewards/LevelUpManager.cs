using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    public int[] RewardPool;
    public LevelReward[] Rewards;

    #region MoreHP
    [Header("MoreHP")]
    [SerializeField] private int _hpIncreaseAmount = 2;
    #endregion

    #region Speed
    [Header("Speed")]
    [SerializeField] private float _speedIncreaseAmount = 1f;
    #endregion

    #region CooldownReduction
    [Header("Cooldown Reduction")]
    [SerializeField] private float _cooldownReduceFraction = 0.9f;
    #endregion

    #region HigherChances
    [Header("Higher Chances")]
    private float[][] _higherChanceLevels = {
        new float[]{ 0.7f, 0.225f },
        new float[]{ 0.65f, 0.25f },
        new float[]{ 0.6f, 0.275f },
        new float[]{ 0.55f, 0.3f },
        new float[]{ 0.4f, 0.4f }
    };
    #endregion

    #region CastTimeReduction
    [Header("Cast Time Reduction")]
    [SerializeField] private float _castTimeReduceFraction = 0.8f;
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ResetRewardPool();
    }

    private void ResetRewardPool()
    {
        RewardPool = new int[Enum.GetNames(typeof(LevelUpReward)).Length-1];

        for(int i = 0; i < RewardPool.Length; i++)
        {
            RewardPool[i] = 5;
        }
    }

    public LevelUpReward[] GenerateThreeRandomAvailableRewards()
    {
        LevelUpReward[] resultingRewards = new LevelUpReward[3];
        List<LevelUpReward> availableRewards = new List<LevelUpReward>();

        for (int i = 0; i < RewardPool.Length; i++)
        {
            for (int j = 0; j < RewardPool[i]; j++)
            {
                availableRewards.Add((LevelUpReward)i);
            }
        }

        for(int i = 0; i < 3; i++)
        {
            if (availableRewards.Count == 0)
            {
                resultingRewards[i] = LevelUpReward.NoMoreRewards;
                continue;
            }

            int randIndex = UnityEngine.Random.Range(0, availableRewards.Count);

            resultingRewards[i] = availableRewards[randIndex];

            int tempIndex = 0;
            while (availableRewards.Contains(resultingRewards[i]))
            {
                if(availableRewards[tempIndex] == resultingRewards[i])
                {
                    availableRewards.RemoveAt(tempIndex);
                }
                else
                {
                    tempIndex++;
                }
            }
        }

        return resultingRewards;
    }

    public void ApplyReward(LevelUpReward reward)
    {
        switch (reward)
        {
            case LevelUpReward.MoreHP:
                PlayerController.Instance.MaxHealth += _hpIncreaseAmount;
                break;
            case LevelUpReward.Speed:
                PlayerController.Instance.PlayerRegularMoveSpeed += _speedIncreaseAmount;
                PlayerController.Instance.ChangeCurrentMoveSpeed(PlayerController.Instance.PlayerRegularMoveSpeed, 0f);
                break;
            case LevelUpReward.CooldownReduction:
                AbilityCaster.Instance.CooldownReductionMultiplier *= _cooldownReduceFraction;
                break;
            case LevelUpReward.HigherChances:
                AbilityCaster.Instance.StarChances = _higherChanceLevels[5 - RewardPool[(int)LevelUpReward.HigherChances]];
                break;
            case LevelUpReward.CastTimeReduction:
                AbilityCaster.Instance.CastTimeReductionMultiplier *= _castTimeReduceFraction;
                break;
            default:
                break;
        }

        RewardPool[(int)reward]--;

        PlayerController.Instance.QueuedLevels--;
        GameManager.Instance.UpdateGameState(GameState.Playing);
    }
}

public enum LevelUpReward
{
    MoreHP,
    Speed,
    CooldownReduction,
    HigherChances,
    CastTimeReduction,
    NoMoreRewards
}