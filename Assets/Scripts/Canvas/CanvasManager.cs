using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject _menuCanvas, _playerCanvas, _abilityCanvas;

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
        _menuCanvas.SetActive(state == GameState.Paused);
        _playerCanvas.SetActive(state == GameState.Playing);
        _abilityCanvas.SetActive(state == GameState.Playing);
    }
}
