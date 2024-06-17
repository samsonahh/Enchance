using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyCollectable : MonoBehaviour
{
    [SerializeField] private RequiredKeys _keyType;

    [SerializeField] private Button _collectButton;

    private bool _onTrigger;

    private void Awake()
    {
        _collectButton.onClick.AddListener(() => {
            CollectKey();
        });

        _collectButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_onTrigger)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CollectKey();
        }
    }

    private void CollectKey()
    {
        GameManager.Instance.BossRoomKeys[(int)_keyType] = 1;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            _collectButton.gameObject.SetActive(true);
            _onTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            _collectButton.gameObject.SetActive(false);
            _onTrigger = false;
        }
    }
}

public enum RequiredKeys
{
    Grass,
    Fire,
    Ice
}