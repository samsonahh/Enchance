using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _cameraSmoothTime;

    private Vector3 _offsetPosition;
    private PlayerController _playerController;

    private void Start()
    {
        _offsetPosition = transform.position;
        _playerController = FindObjectOfType<PlayerController>();
    }

    private void FixedUpdate()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        transform.position = Vector3.Lerp(transform.position, _playerController.transform.position + _offsetPosition, _cameraSmoothTime * Time.deltaTime);
    }
}
