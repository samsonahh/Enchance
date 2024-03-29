using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _arrowPivot;
    [SerializeField] private Transform _staffGlowEffect;
    private Animator _animator;

    [HideInInspector] public static PlayerController Instance { get; private set; }

    [HideInInspector] public Vector3 ForwardDirection { get; private set; } = Vector3.right;
    [HideInInspector] public Vector3 LastForwardDirection { get; private set; }
    [HideInInspector] public Vector3 MouseWorldPosition { get; private set; }
    [HideInInspector] public Vector3 LastMouseWorldPosition { get; private set; }
    [HideInInspector] public Vector3 LastCircleWorldPosition { get; private set; }
    [HideInInspector] public Vector3 PlayerDestinationPositon { get; private set; }

    #region Conditions
    [HideInInspector] public bool IsMoving { get; private set; }
    [HideInInspector] public bool IsCasting;
    [HideInInspector] public bool IsStunned;
    [HideInInspector] public bool IsBurning;
    [HideInInspector] public bool IsInvincible;
    [HideInInspector] public bool IsVisible = true;
    [HideInInspector] public bool CanCast = true;
    #endregion

    #region Speed
    [Header("Speed")]
    [SerializeField] private float _playerCurrentMoveSpeed = 5f;
    private float _playerRegularMoveSpeed;
    #endregion

    #region Burning
    [Header("Burning")]
    [SerializeField] private float _burnTickDuration = 1f;
    [SerializeField] private int _burnDamage = 1;
    [HideInInspector] public int BurnTicks;
    private IEnumerator _burningPlayerCoroutine;
    private Color _currentColor = Color.white;
    [SerializeField] private Color _burningColor;
    #endregion

    #region Health
    [Header("Health")]
    public int CurrentHealth;
    public int MaxHealth;
    private float _lastDamagedTimer;
    [SerializeField] private float _startRegenTreshold = 5f;
    [SerializeField] private float _regenRate = 3f;
    #endregion

    #region BossGridStuff
    public static Action<Tile> OnPlayerStepOnNewTile;
    [HideInInspector] public Tile CurrentTile;
    #endregion

    private void Awake()
    {
        Instance = this;

        AbilityCaster.OnAbilityCast += AbilityCaster_OnAbilityCast;
    }

    private void OnDestroy()
    {
        AbilityCaster.OnAbilityCast -= AbilityCaster_OnAbilityCast;
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();

        CurrentHealth = MaxHealth;
        StartCoroutine(RegenHealth());

        _playerRegularMoveSpeed = _playerCurrentMoveSpeed;
    }

    private void Update()
    {
        GetMouseWorldPosition();
        HandlePlayerMoving();
        HandlePlayerFacingDirection();
        HandleTileChange();
        ManagePlayerHealth();
        HandleAnimations();
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
    
    private void HandleAnimations()
    {
        _animator.SetBool("IsMoving", IsMoving);
        _animator.SetBool("IsCasting", IsCasting);
        _animator.SetBool("IsStunned", IsStunned);

        _staffGlowEffect.localPosition = _spriteRenderer.flipX ? new Vector3(-0.508f, _staffGlowEffect.localPosition.y, _staffGlowEffect.localPosition.z) : new Vector3(0.508f, _staffGlowEffect.localPosition.y, _staffGlowEffect.localPosition.z);
        _spriteRenderer.enabled = IsVisible;
        _staffGlowEffect.gameObject.SetActive(!IsInvincible);
    }

    private void HandlePlayerMoving()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 movementDirection = new Vector3(x, 0, z).normalized;

        if (IsStunned)
        {
            IsMoving = false;
            _animator.CrossFadeInFixedTime("PlayerStunned", 0.1f);
            return;
        }

        if (IsCasting)
        {
            IsMoving = false;
            return;
        }

        IsMoving = movementDirection.magnitude != 0;

        transform.Translate(_playerCurrentMoveSpeed * Time.deltaTime * movementDirection, Space.World);
    }

    private void HandlePlayerFacingDirection()
    {
        if (IsCasting) return;

        Vector3 facingDirection = (MouseWorldPosition - transform.position).normalized;

        if (Mathf.Abs(facingDirection.x) > 0) _spriteRenderer.flipX = facingDirection.x > 0;

        if (facingDirection.magnitude > 0)
        {
            ForwardDirection = facingDirection;
        }

        float targetAngle = -(Mathf.Atan2(ForwardDirection.z, ForwardDirection.x) * Mathf.Rad2Deg - 90f);
        _arrowPivot.localRotation = Quaternion.Lerp(_arrowPivot.localRotation, Quaternion.AngleAxis(targetAngle, Vector3.up), 50f * Time.deltaTime);
    }

    private void AssignLastVariables()
    {
        LastForwardDirection = ForwardDirection;
        LastMouseWorldPosition = MouseWorldPosition;
        LastCircleWorldPosition = AbilityCaster.Instance.CircleCastTransform.position;
    }

    private void AbilityCaster_OnAbilityCast(int i)
    {
        if (i == 1) return;

        AssignLastVariables();
    }

    private void ManagePlayerHealth()
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        //HANDLE PLAYER DEATH
        if(CurrentHealth <= 0)
        {
            GameManager.Instance.UpdateGameState(GameState.Dead);
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
        if (IsInvincible) return;

        CurrentHealth -= damage;
        _lastDamagedTimer = 0f;

        _spriteRenderer.color = Color.red;
        StartCoroutine(TakeDamageCoroutine());
    }

    public IEnumerator TakeDamageCoroutine()
    {
        yield return new WaitForSeconds(0.15f);
        _spriteRenderer.color = _currentColor;
    }

    public void Heal(int hp)
    {
        CurrentHealth += hp;
    }

    public void PushPlayer(Vector3 dir, float stunDuration, float startVel)
    {
        StartCoroutine(PushPlayerCoroutine(dir, stunDuration, startVel));
    }

    public IEnumerator PushPlayerCoroutine(Vector3 dir, float stunDuration, float startVel)
    {
        IsStunned = true;

        float currVel = startVel;
        float timer = stunDuration;

        while (timer > 0)
        {
            transform.Translate(currVel * Time.deltaTime * dir);

            currVel = (timer / stunDuration) * startVel;
            timer -= Time.deltaTime;

            yield return null;
        }

        IsStunned = false;
    }

    public void StunPlayer(float duration)
    {
        if (IsInvincible) return;

        StartCoroutine(StunPlayerCoroutine(duration));
    }

    public IEnumerator StunPlayerCoroutine(float duration)
    {
        IsStunned = true;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            yield return null;
        }

        IsStunned = false;
    }

    public void BurnPlayer(int ticks)
    {
        if(IsBurning)
        {
            BurnTicks += ticks;
            return;
        }

        BurnTicks = ticks;

        if(_burningPlayerCoroutine != null)
        {
            StopCoroutine(_burningPlayerCoroutine);
        }
        _burningPlayerCoroutine = BurnPlayerCoroutine();
        StartCoroutine(_burningPlayerCoroutine);
    }

    public IEnumerator BurnPlayerCoroutine()
    {
        IsBurning = true;
        _currentColor = _burningColor;

        while(BurnTicks > 0)
        {
            TakeDamage(_burnDamage);
            yield return TakeDamageCoroutine();
            yield return new WaitForSeconds(_burnTickDuration - 0.15f);
            BurnTicks--;
        }

        _currentColor = Color.white;
        _spriteRenderer.color = _currentColor;
        IsBurning = false;
    }

    private void HandleTileChange()
    {
        if (GridManager.Instance == null) return;

        Tile t = GridManager.Instance.GetTileAtPosition(transform.position);
        if(CurrentTile != t)
        {
            CurrentTile = t;
            OnPlayerStepOnNewTile?.Invoke(CurrentTile);
        }
    }
}
