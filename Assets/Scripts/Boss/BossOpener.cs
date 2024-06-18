using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOpener : MonoBehaviour
{
    [SerializeField] private GameObject _sceneLoadTriggerObject;

    private void Start()
    {
        _sceneLoadTriggerObject.SetActive(false);
    }

    private void Update()
    {
        if(GameManager.Instance.BossRoomKeys[0] == 1 && GameManager.Instance.BossRoomKeys[1] == 1 && GameManager.Instance.BossRoomKeys[2] == 1)
        {
            _sceneLoadTriggerObject.SetActive(true);
        }
    }
}
