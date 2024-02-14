using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public static PlayerController Instance { get; private set; }

    [HideInInspector] public Vector3 ForwardDirection { get; private set; } = Vector3.right;

    [SerializeField] private Transform _arrowPivot;
    [SerializeField] private Transform _playerAimMarker;
    private Vector3 _playerAimMarketOffset;
    [SerializeField] private float _aimSpeed;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        HandlePlayerFacingDirection();
    }

    private void HandlePlayerFacingDirection()
    {
        float x = Input.GetAxisRaw("RightHorizontal");
        float z = Input.GetAxisRaw("RightVertical");

        Vector3 facingDirection = new Vector3(x, 0, z).normalized;

        Vector3 botLeft = Camera.main.ScreenToWorldPoint(Vector2.zero);
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight));

        Debug.Log("Bot Left: " + botLeft.ToString() + ", Top Right: " + topRight.ToString() + ", Aim: " + _playerAimMarker.position.ToString());

        _playerAimMarketOffset += _aimSpeed * Time.deltaTime * facingDirection;
        _playerAimMarker.position = transform.position + _playerAimMarketOffset;
        //_playerAimMarker.position = new Vector3(Mathf.Clamp(_playerAimMarker.position.x, botLeft.x, topRight.x - botLeft.x), 0, Mathf.Clamp(_playerAimMarker.position.z, botLeft.z, topRight.z - botLeft.z));

        if (Mathf.Abs(x) > 0) _spriteRenderer.flipX = x < 0;

        if (facingDirection.magnitude > 0)
        {
            ForwardDirection = facingDirection;
        }

        float targetAngle = -(Mathf.Atan2(ForwardDirection.z, ForwardDirection.x) * Mathf.Rad2Deg - 90f);
        _arrowPivot.rotation = Quaternion.Lerp(_arrowPivot.rotation, Quaternion.AngleAxis(targetAngle, Vector3.up), 50f * Time.deltaTime);
    }
}
