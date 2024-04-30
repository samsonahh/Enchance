using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetworkManager : NetworkBehaviour
{
    public NetworkVariable<Vector3> NetworkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> NetworkFlipped = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> NetworkIsMoving = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<Vector3> NetworkLastMouseWorldPos = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> NetworkLastCircleWorldPos = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> NetworkLastForwardDirection = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Vector3 _smoothVelocity;

    [SerializeField] private SpriteRenderer _spriteRenderer;

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            transform.position = Vector3.SmoothDamp(transform.position, NetworkPosition.Value, ref _smoothVelocity, 0.1f);

            _spriteRenderer.flipX = NetworkFlipped.Value;
            _playerController.IsMoving = NetworkIsMoving.Value;

            _playerController.LastMouseWorldPosition = NetworkLastMouseWorldPos.Value;
            _playerController.LastCircleWorldPosition = NetworkLastCircleWorldPos.Value;
            _playerController.LastForwardDirection = NetworkLastForwardDirection.Value;

            return;
        }
        else
        {
            NetworkPosition.Value = transform.position;
            NetworkFlipped.Value = _spriteRenderer.flipX;

            NetworkIsMoving.Value = _playerController.IsMoving;
            NetworkLastMouseWorldPos.Value = _playerController.LastMouseWorldPosition;
            NetworkLastCircleWorldPos.Value = _playerController.LastCircleWorldPosition;
            NetworkLastForwardDirection.Value = _playerController.LastForwardDirection;
        }
    }
}
