using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    public int[] RewardPool;

    public struct RewardDetails
    {
        public string Name;
        public Sprite Sprite;
        public string Description;
    }
    public RewardDetails[] RewardDetailsArray;

    #region MoreHP
    [Header("MoreHP")]
    [SerializeField] private int _hpIncreaseAmount = 2;

    public string MoreHPName = "Tanky Body";
    public Sprite MoreHPSprite;
    [TextArea(15, 20)]
    public string MoreHPDescription;
    #endregion

    #region Speed
    [Header("Speed")]
    [SerializeField] private float _speedIncreaseAmount = 1f;

    public string SpeedName = "Quickfeet";
    public Sprite SpeedSprite;
    [TextArea(15, 20)]
    public string SpeedDescription;
    #endregion

    #region CooldownReduction
    [Header("Cooldown Reduction")]
    [SerializeField] private float _cooldownReduceFraction = 0.9f;

    public string CooldownReductionName = "Fast Chanter";
    public Sprite CooldownReductionSprite;
    [TextArea(15, 20)]
    public string CooldownReductionDescription;
    #endregion

    #region HigherChances
    [Header("Higher Chances")]
    [SerializeField] private float[][] _higherChanceLevels;

    public string HigherChancesName = "Lucky Roller";
    public Sprite HigherChancesSprite;
    [TextArea(15, 20)]
    public string HigherChancesDescription;
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ResetRewardPool();
        PopulateRewardDetailsArray();
    }

    private void ResetRewardPool()
    {
        RewardPool = new int[Enum.GetNames(typeof(LevelUpReward)).Length-1];

        for(int i = 0; i < RewardPool.Length; i++)
        {
            RewardPool[i] = 5;
        }
    }

    private void PopulateRewardDetailsArray()
    {
        RewardDetailsArray = new RewardDetails[RewardPool.Length];

        RewardDetailsArray[(int)LevelUpReward.MoreHP].Name = MoreHPName;
        RewardDetailsArray[(int)LevelUpReward.MoreHP].Sprite = MoreHPSprite;
        RewardDetailsArray[(int)LevelUpReward.MoreHP].Description = MoreHPDescription;

        RewardDetailsArray[(int)LevelUpReward.Speed].Name = SpeedName;
        RewardDetailsArray[(int)LevelUpReward.Speed].Sprite = SpeedSprite;
        RewardDetailsArray[(int)LevelUpReward.Speed].Description = SpeedDescription;

        RewardDetailsArray[(int)LevelUpReward.CooldownReduction].Name = CooldownReductionName;
        RewardDetailsArray[(int)LevelUpReward.CooldownReduction].Sprite = CooldownReductionSprite;
        RewardDetailsArray[(int)LevelUpReward.CooldownReduction].Description = CooldownReductionDescription;

        RewardDetailsArray[(int)LevelUpReward.HigherChances].Name = HigherChancesName;
        RewardDetailsArray[(int)LevelUpReward.HigherChances].Sprite = HigherChancesSprite;
        RewardDetailsArray[(int)LevelUpReward.HigherChances].Description = HigherChancesDescription;
    }

    public LevelUpReward GenerateRandomAvailableReward()
    {
        while (true)
        {
            bool rewardsAvailable = false;

            for (int i = 0; i < RewardPool.Length; i++)
            {
                if (RewardPool[i] != 0)
                {
                    rewardsAvailable = true;
                    break;
                }
            }

            if (!rewardsAvailable) return LevelUpReward.NoMoreRewards;

            int randIndex = UnityEngine.Random.Range(0, RewardPool.Length);

            if (RewardPool[randIndex] == 0)
            {
                continue;
            }

            return (LevelUpReward)randIndex;
        }
    }

    public void ApplyReward(LevelUpReward reward)
    {
        Debug.Log(reward);

        RewardPool[(int)reward]--;

        switch (reward)
        {
            case LevelUpReward.MoreHP:
                break;
            case LevelUpReward.Speed:
                break;
            case LevelUpReward.CooldownReduction:
                break;
            case LevelUpReward.HigherChances:
                break;
            default:
                break;
        }

    }
}

public enum LevelUpReward
{
    MoreHP,
    Speed,
    CooldownReduction,
    HigherChances,
    NoMoreRewards
}