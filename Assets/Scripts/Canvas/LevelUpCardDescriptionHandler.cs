using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpCardDescriptionHandler : MonoBehaviour
{
    public Image CardImage;
    public TMP_Text CardName;
    public TMP_Text CardDescription;

    public LevelUpReward Reward;

    public void OnSelectButtonPressed()
    {
        LevelUpManager.Instance.ApplyReward(Reward);

        LevelUpCanvasManager.Instance.RemoveAllCards();
    }


}
