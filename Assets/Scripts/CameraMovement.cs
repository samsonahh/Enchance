using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _cameraSmoothTime;

    private Vector3 _offsetPosition;
    private PlayerController _playerController;
    private float _zoom = 10f;

    private void Start()
    {
        _offsetPosition = transform.position;
        _playerController = PlayerController.Instance;
    }

    private void Update()
    {
        FollowPlayer();
        HandleZoom();
    }

    private void FollowPlayer()
    {
        transform.position = Vector3.Lerp(transform.position, _playerController.transform.position + _offsetPosition, _cameraSmoothTime * Time.deltaTime);
    }

    private void HandleZoom()
    {
        _zoom -= Input.mouseScrollDelta.y;
        _zoom = Mathf.Clamp(_zoom, 3f, 10f);

        _offsetPosition = Vector3.Lerp(_offsetPosition, new Vector3(0, _zoom, -_zoom), 2f * _cameraSmoothTime * Time.deltaTime);
    }
}
