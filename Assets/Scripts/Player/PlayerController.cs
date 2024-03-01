using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _arrowPivot;
    [SerializeField] private DestinationIndicator _playerDestinationObject;
    private NavMeshAgent _navMeshAgent;

    [HideInInspector] public static PlayerController Instance { get; private set; }

    [HideInInspector] public Vector3 ForwardDirection { get; private set; } = Vector3.right;
    [HideInInspector] public Vector3 MouseWorldPosition { get; private set; }
    [HideInInspector] public Vector3 PlayerDestinationPositon { get; private set; }

    [HideInInspector] public bool IsMoving { get; private set; }
    [HideInInspector] public bool IsCasting;

    //Health
    public int CurrentHealth;
    public int MaxHealth;
    private float _lastDamagedTimer;
    [SerializeField] private float _startRegenTreshold = 5f;
    [SerializeField] private float _regenRate = 3f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        CurrentHealth = MaxHealth;
        StartCoroutine(RegenHealth());
    }

    private void Update()
    {
        GetMouseWorldPosition();
        HandlePlayerMoving();
        HandlePlayerFacingDirection();
        ManagePlayerHealth();

        if (Input.GetKeyDown(KeyCode.Space)) TakeDamage(3);
    }

    private void GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            MouseWorldPosition = ray.GetPoint(distance);
        }
    }

    private void HandlePlayerMoving()
    {
        if (IsCasting)
        {
            IsMoving = false;
            _navMeshAgent.SetDestination(transform.position);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            IsMoving = true;

            PlayerDestinationPositon = MouseWorldPosition;
            _navMeshAgent.SetDestination(PlayerDestinationPositon);

            _playerDestinationObject.SetIndicatorPostion(PlayerDestinationPositon);
        }

        IsMoving = _navMeshAgent.velocity.magnitude != 0;
        _playerDestinationObject.MakeSpriteVisible(IsMoving);
    }

    private void HandlePlayerFacingDirection()
    {
        Vector3 facingDirection = (MouseWorldPosition - transform.position).normalized;

        if (Mathf.Abs(facingDirection.x) > 0) _spriteRenderer.flipX = facingDirection.x < 0;

        if (facingDirection.magnitude > 0)
        {
            ForwardDirection = facingDirection;
        }

        float targetAngle = -(Mathf.Atan2(ForwardDirection.z, ForwardDirection.x) * Mathf.Rad2Deg - 90f);
        _arrowPivot.localRotation = Quaternion.Lerp(_arrowPivot.localRotation, Quaternion.AngleAxis(targetAngle, Vector3.up), 50f * Time.deltaTime);
    }

    private void ManagePlayerHealth()
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        //HANDLE PLAYER DEATH
        if(CurrentHealth <= 0)
        {

        }

        //REGEN
        if (_lastDamagedTimer < _startRegenTreshold + 1) _lastDamagedTimer += Time.deltaTime;
    }

    private IEnumerator RegenHealth()
    {
        while (true)
        {
            if (_lastDamagedTimer <= _startRegenTreshold || CurrentHealth >= MaxHealth || CurrentHealth <= 0)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(_regenRate);
                CurrentHealth++;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        _lastDamagedTimer = 0f;
    }
}
