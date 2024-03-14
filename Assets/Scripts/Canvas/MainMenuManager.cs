using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button _startButton;

    public void ScaleStartButton(float scale)
    {
        LeanTween.scale(_startButton.gameObject, new Vector3(scale, scale, scale), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    }
}
