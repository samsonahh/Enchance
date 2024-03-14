using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuCanvasManager : MonoBehaviour
{
    [SerializeField] private Button _menuButton;

    public void ScaleMenuButtonSize(float size)
    {
        LeanTween.scale(_menuButton.gameObject, new Vector3(size, size, size), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    } 
}
