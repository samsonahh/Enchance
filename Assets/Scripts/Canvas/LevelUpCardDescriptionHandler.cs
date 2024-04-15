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

    public void OnClickCard()
    {
        GameManager.Instance.UpdateGameState(GameState.Playing);
    }
}