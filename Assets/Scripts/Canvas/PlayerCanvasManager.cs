using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCanvasManager : MonoBehaviour
{
    [SerializeField] private Slider _playerHealthSlider;
    [SerializeField] private Image _playerHealthFill;
    [SerializeField] private Slider _playerShieldSlider;
    [SerializeField] private TMP_Text _playerHealthText;
    [SerializeField] private Slider _playerExpSlider;
    [SerializeField] private TMP_Text _playerLevelText;

    [Header("Health Bar Colors")]
    [SerializeField] private float _yellowThreshold = 0.5f;
    [SerializeField] private float _redThreshold = 0.25f;

    [Header("SFX")]
    [SerializeField] private AudioClip _lowHpDangerSfx;
    private bool _canPlayLowHpDangerSfx = true;

    private void Update()
    {
        HandleHealthBar();
        HandleLevelBar();
        HandleShieldBar();
    }

    private void HandleHealthBar()
    {
        float playerHealthPercentage = (float)PlayerController.Instance.CurrentHealth/PlayerController.Instance.MaxHealth;

        _playerHealthText.text = $"{PlayerController.Instance.CurrentHealth}/{PlayerController.Instance.MaxHealth}";
        _playerHealthSlider.value = Mathf.Lerp(_playerHealthSlider.value, (float)PlayerController.Instance.CurrentHealth / PlayerController.Instance.MaxHealth, 5f * Time.deltaTime);

        // COLORS!!!
        if(playerHealthPercentage <= 1f && playerHealthPercentage > _yellowThreshold)
        {
            _playerHealthFill.color = Color.Lerp(_playerHealthFill.color, GameManager.Instance.GreenHealthColor, 5f * Time.deltaTime);
            _canPlayLowHpDangerSfx = true;
        }
        if (playerHealthPercentage <= _yellowThreshold && playerHealthPercentage > _redThreshold)
        {
            _playerHealthFill.color = Color.Lerp(_playerHealthFill.color, GameManager.Instance.YellowHealthColor, 5f * Time.deltaTime);
            _canPlayLowHpDangerSfx = true;
        }
        if (playerHealthPercentage <= _redThreshold && playerHealthPercentage >= 0f)
        {
            if (_canPlayLowHpDangerSfx)
            {
                AudioSource.PlayClipAtPoint(_lowHpDangerSfx, PlayerController.Instance.transform.position);
            }
            _canPlayLowHpDangerSfx = false;
            _playerHealthFill.color = Color.Lerp(_playerHealthFill.color, GameManager.Instance.RedHealthColor, 5f * Time.deltaTime);
        }
    }

    private void HandleLevelBar()
    {
        _playerLevelText.text = $"{PlayerController.Instance.CurrentLevel}";

        float playerLevelPercentage = (float)PlayerController.Instance.CurrentExp / PlayerController.Instance.ExpToNextLevel;

        _playerExpSlider.value = Mathf.Lerp(_playerExpSlider.value, playerLevelPercentage, 5f * Time.deltaTime);
    }

    private void HandleShieldBar()
    {
        float playerShieldPercentage = (float)PlayerController.Instance.CurrentShieldHealth / PlayerController.Instance.MaxHealth;

        _playerShieldSlider.value = Mathf.Lerp(_playerShieldSlider.value, playerShieldPercentage, 5f * Time.deltaTime);
    }
}
