using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject _menuCanvas, _playerCanvas, _abilityCanvas, _levelUpCanvas;

    private void Awake()
    {
        GameManager.OnGameStateChanged += GameManagerOnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameManagerOnGameStateChanged;
    }

    private void GameManagerOnGameStateChanged(GameState state)
    {
        if (_menuCanvas == null) return;
        if (_playerCanvas == null) return;
        if (_abilityCanvas == null) return;
        if (_levelUpCanvas == null) return;

        _menuCanvas.SetActive(state == GameState.Paused);
        _playerCanvas.SetActive(state == GameState.Playing);
        _abilityCanvas.SetActive(state == GameState.Playing);
        _levelUpCanvas.SetActive(state == GameState.LevelUpSelect);
    }
}
