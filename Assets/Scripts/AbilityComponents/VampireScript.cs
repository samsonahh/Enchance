using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireScript : MonoBehaviour
{
    [SerializeField] private float _duration;

    private void Start()
    {
        GameManager.Instance.PlayerControllerInstance.EnableLifeSteal(_duration);

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = GameManager.Instance.PlayerControllerInstance.transform.position;
    }
}
