using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCanvasManager : MonoBehaviour
{
    [SerializeField] private Slider _playerHealthSlider;
    [SerializeField] private TMP_Text _playerHealthText;

    private void Update()
    {
        HandleHealthBar();
    }

    private void HandleHealthBar()
    {
        _playerHealthText.text = $"{PlayerController.Instance.CurrentHealth}/{PlayerController.Instance.MaxHealth}";
        _playerHealthSlider.value = (float)PlayerController.Instance.CurrentHealth / PlayerController.Instance.MaxHealth;
    }
}
