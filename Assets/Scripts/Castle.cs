using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Castle : MonoBehaviour
{
    [SerializeField] private Button _openButton;
    [SerializeField] private GameObject _dialogueTextObject;
    [SerializeField] private SeparateSceneLoadTrigger _separateSceneLoader;

    private bool _onTrigger;
    private bool _openToPlayer = false;

    private void Awake()
    {
        _openButton.onClick.AddListener(() =>
        {
            if (_openToPlayer)
            {
                _separateSceneLoader.SwitchScene(0.5f, 0.5f);
            }
            else
            {
                _dialogueTextObject.SetActive(true);
                _openButton.gameObject.SetActive(false);
            }
        });
    }

    private void Start()
    {
        _openButton.gameObject.SetActive(false);
        _dialogueTextObject.SetActive(false);
    }

    private void Update()
    {
        _openToPlayer = GameManager.Instance.BossRoomKeys[0] == 1 && GameManager.Instance.BossRoomKeys[1] == 1 && GameManager.Instance.BossRoomKeys[2] == 1;

        if (_onTrigger)
        {
            if (_dialogueTextObject.activeSelf) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_openToPlayer)
                {
                    _separateSceneLoader.SwitchScene(0.5f, 0.5f);
                }
                else
                {
                    _dialogueTextObject.SetActive(true);
                    _openButton.gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            _onTrigger = true;

            _openButton.gameObject.SetActive(true);
            _dialogueTextObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            _onTrigger = false;

            _openButton.gameObject.SetActive(false);
            _dialogueTextObject.SetActive(false);
        }
    }
}
