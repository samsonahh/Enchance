using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _arrowPivot;
    [SerializeField] private Transform _staffGlowEffect;
    [SerializeField] private Transform _autoRadiusTransform;
    [SerializeField] private Renderer _shieldRenderer;
    private Animator _animator;

    [HideInInspector] public static PlayerController Instance { get; private set; }

    [HideInInspector] public Vector3 ForwardDirection { get; private set; } = Vector3.right;
    [HideInInspector] public Vector3 LastForwardDirection { get; private set; }
    [HideInInspector] public Vector3 MouseWorldPosition { get; private set; }
    private Vector3 _restrictedMouseWorldPosition;
    [HideInInspector] public Vector3 LastMouseWorldPosition { get; private set; }
    [HideInInspector] public Vector3 LastCircleWorldPosition { get; private set; }
    [HideInInspector] public GameObject Target { get; private set; }
    [HideInInspector] public GameObject LastTarget { get; private set; }

    #region Stats
    [Header("Stats")]
    public int CurrentLevel = 1;
    public int CurrentExp = 0;
    [SerializeField] private float _expRequirementGrowth = 1.1f;
    [SerializeField] private int _expRequirementGrowthOffset = 10;
    [HideInInspector] public int ExpToNextLevel = 10;
    [HideInInspector] public int QueuedLevels = 0;

    [SerializeField] private float _autoAttackRadius = 15f;
    [SerializeField] private float _targetFindRadiusOnCursor = 2f;
    #endregion

    #region Conditions
    [HideInInspector] public bool IsMoving { get; private set; }
    [HideInInspector] public bool IsCasting;
    [HideInInspector] public bool IsStunned;
    [HideInInspector] public bool IsBurning;
    [HideInInspector] public bool IsPoisoned;
    [HideInInspector] public bool IsInvincible;
    [HideInInspector] public bool IsShielded;
    [HideInInspector] public bool IsTrapped;
    [HideInInspector] public bool IsVisible = true;
    [HideInInspector] public bool CanCast = true;
    [HideInInspector] public bool LifeSteal;
    [HideInInspector] public bool CanMove = true;
    [HideInInspector] public bool AutoAttacking;
    #endregion

    #region Speed
    [Header("Speed")]
    public float PlayerCurrentMoveSpeed = 5f;
    [HideInInspector] public float PlayerRegularMoveSpeed;
    private Coroutine _currentMoveSpeedCoroutine;
    #endregion

    #region LifeSteal
    [Header("LifeSteal")]
    private Coroutine _lifeStealCoroutine;
    #endregion

    #region Burning
    [Header("Burning")]
    [SerializeField] private float _burnTickDuration = 1f;
    [SerializeField] private int _burnDamage = 1;
    [HideInInspector] public int BurnTicks;
    private IEnumerator _burningPlayerCoroutine;
    private Color _currentColor = Color.white;
    #endregion

    #region Poison
    [Header("Poison")]
    [SerializeField] private float _poisonTickDuration = 1f;
    [SerializeField] private int _poisonDamage = 1;
    [HideInInspector] public int PoisonTicks;
    private IEnumerator _poisonPlayerCoroutine;
    #endregion

    #region Health
    [Header("Health")]
    public int CurrentHealth;
    public int MaxHealth;
    private float _lastDamagedTimer;
    [SerializeField] private float _startRegenTreshold = 5f;
    [SerializeField] private float _regenRate = 3f;
    [SerializeField] private AudioClip _hurtSFX;
    #endregion

    #region Shield
    [Header("Shield")]
    public int CurrentShieldHealth;
    private Coroutine _shieldPlayerCoroutine;
    #endregion

    #region ForBossGridManager
    public static Action<Tile> OnPlayerStepOnNewTile;
    [HideInInspector] public Tile CurrentTile;
    #endregion

    private void Awake()
    {
        Instance = this;

        _animator = GetComponent<Animator>();

        CurrentHealth = MaxHealth;
        StartCoroutine(RegenHealth());

        PlayerRegularMoveSpeed = PlayerCurrentMoveSpeed;
    }

    private void OnDestroy()
    {
        
    }

    private void Start()
    {
        _autoRadiusTransform.localScale = new Vector3(_autoAttackRadius + _targetFindRadiusOnCursor, 1, _autoAttackRadius + _targetFindRadiusOnCursor);
    }

    private void Update()
    {
        GetMouseWorldPosition();
        HandlePlayerMoving();
        HandlePlayerFacingDirection();
        HandleTileChange();
        ManagePlayerHealth();
        HandleLevel();
        HandleOverfillLevels();
        HandleAnimations();
        AssignTarget();

        if (Input.GetKeyDown(KeyCode.Space) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            AddExp(ExpToNextLevel);
        }
    }

    private void OnDrawGizmos()
    {
        CustomGizmos.DrawWireDisk(_restrictedMouseWorldPosition, _targetFindRadiusOnCursor, Color.green);
        CustomGizmos.DrawWireDisk(transform.position, _autoAttackRadius, Color.black);

        if(Target != null)
        {
            CustomGizmos.DrawDisk(Target.transform.position, 1f, Color.green);
        }
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
        _staffGlowEffect.gameObject.SetActive(!IsInvincible && !AutoAttacking);
        _shieldRenderer.gameObject.SetActive(IsShielded);
    }

    private void HandlePlayerMoving()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 movementDirection = new Vector3(x, 0, z).normalized;

        if (!CanMove)
        {
            IsMoving = false;
            return;
        }

        if (IsStunned)
        {
            IsMoving = false;
            _animator.CrossFadeInFixedTime("PlayerStunned", 0.1f);
            return;
        }

        if (IsCasting)
        {
            _animator.Play("PlayerCasting");
            IsMoving = false;
            return;
        }

        IsMoving = movementDirection.magnitude != 0;

        transform.Translate(PlayerCurrentMoveSpeed * Time.deltaTime * movementDirection, Space.World);
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

    public void AssignLastVariables()
    {
        LastForwardDirection = ForwardDirection;
        LastMouseWorldPosition = MouseWorldPosition;
        LastCircleWorldPosition = AbilityCaster.Instance.CircleCastTransform.position;
        LastTarget = Target;
    } 

    private void AssignTarget()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        _restrictedMouseWorldPosition = Vector3.ClampMagnitude(MouseWorldPosition - transform.position, _autoAttackRadius) + transform.position;

        Collider[] collisions = Physics.OverlapCapsule(_restrictedMouseWorldPosition, _restrictedMouseWorldPosition + 5f * Vector3.up, _targetFindRadiusOnCursor);
        
        if (collisions == null)
        {
            Target = null;
            return;
        }
        if (collisions.Length == 0)
        { 
            Target = null;
            return;
        }

        List<GameObject> enemies = new List<GameObject>();

        foreach (Collider collider in collisions)
        {
            if (collider.TryGetComponent(out EnemyController enemy))
            {
                if (enemy.CanBeTargetted)
                {
                    enemies.Add(enemy.gameObject);
                }
            }
            if(collider.TryGetComponent(out BossAI boss))
            {
                enemies.Add(boss.gameObject);
            }
        }

        if(enemies == null)
        {
            Target = null;
            return;
        }
        if(enemies.Count == 0)
        {
            Target = null;
            return;
        }

        enemies = enemies.OrderByDescending(e => Vector3.Distance(transform.position, e.transform.position)).ToList();

        Target = enemies[0].gameObject;
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

        if(CurrentShieldHealth > 0 && IsShielded)
        {
            if(damage > CurrentShieldHealth)
            {
                damage -= CurrentShieldHealth;
                CurrentShieldHealth = 0;
                IsShielded = false;
            }
            else
            {
                CurrentShieldHealth -= damage;
                _spriteRenderer.color = Color.red;
                StartCoroutine(TakeDamageCoroutine());
                
                if(CurrentShieldHealth == 0)
                {
                    IsShielded = false;
                }

                return;
            }
        }

        CurrentHealth -= damage;
        _lastDamagedTimer = 0f;

        _spriteRenderer.color = Color.red;
        StartCoroutine(TakeDamageCoroutine());

        AudioSource.PlayClipAtPoint(_hurtSFX, transform.position);
    }

    public IEnumerator TakeDamageCoroutine()
    {
        _shieldRenderer.material.color = new Color(_shieldRenderer.material.color.r, _shieldRenderer.material.color.g, _shieldRenderer.material.color.b, 150f/255f);

        yield return new WaitForSeconds(0.15f);

        _spriteRenderer.color = _currentColor;
        _shieldRenderer.material.color = new Color(_shieldRenderer.material.color.r, _shieldRenderer.material.color.g, _shieldRenderer.material.color.b, 35f/255f);
    }

    public void Heal(int hp)
    {
        CurrentHealth += hp;
    }

    public void PushPlayer(Vector3 dir, float stunDuration, float startVel)
    {
        if (IsInvincible) return;

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
        _currentColor = GameManager.Instance.EntityBurningColor;

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

    public void PoisonPlayer(int ticks)
    {
        if (IsPoisoned)
        {
            PoisonTicks += ticks;
            return;
        }

        PoisonTicks = ticks;

        if (_poisonPlayerCoroutine != null)
        {
            StopCoroutine(_poisonPlayerCoroutine);
        }
        _poisonPlayerCoroutine = PoisonPlayerCoroutine();
        StartCoroutine(_poisonPlayerCoroutine);
    }

    public IEnumerator PoisonPlayerCoroutine()
    {
        IsPoisoned = true;
        _currentColor = GameManager.Instance.EntityPoisonedColor;

        while (PoisonTicks > 0)
        {
            TakeDamage(_poisonDamage);
            yield return TakeDamageCoroutine();
            yield return new WaitForSeconds(_poisonTickDuration - 0.15f);
            PoisonTicks--;
        }

        _currentColor = Color.white;
        _spriteRenderer.color = _currentColor;
        IsPoisoned = false;
    }

    public void ShieldPlayer(int shieldAmount, float duration)
    {
        if (_shieldPlayerCoroutine != null)
        {
            StopCoroutine(_shieldPlayerCoroutine);
            _shieldPlayerCoroutine = null;
        }
        _shieldPlayerCoroutine = StartCoroutine(ShieldPlayerCoroutine(shieldAmount, duration));
    }

    public IEnumerator ShieldPlayerCoroutine(int shieldAmount, float duration)
    {
        IsShielded = true;
        CurrentShieldHealth = shieldAmount;
        yield return new WaitForSeconds(duration);
        CurrentShieldHealth = 0;
        IsShielded = false;
        _shieldPlayerCoroutine = null;
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

    public void ChangeCurrentMoveSpeed(float speed, float duration)
    {
        if(_currentMoveSpeedCoroutine != null)
        {
            StopCoroutine(_currentMoveSpeedCoroutine);
            _currentMoveSpeedCoroutine = null;
        }
        _currentMoveSpeedCoroutine = StartCoroutine(ChangeCurrentMoveSpeedCoroutine(speed, duration));
    }

    public IEnumerator ChangeCurrentMoveSpeedCoroutine(float speed, float duration)
    {
        PlayerCurrentMoveSpeed = speed;
        yield return new WaitForSeconds(duration);
        PlayerCurrentMoveSpeed = PlayerRegularMoveSpeed;
        _currentMoveSpeedCoroutine = null;
    }

    public void EnableLifeSteal(float duration)
    {
        if (_lifeStealCoroutine != null)
        {
            StopCoroutine(_lifeStealCoroutine);
            _lifeStealCoroutine = null;
        }
        _lifeStealCoroutine = StartCoroutine(EnableLifeStealCoroutine(duration));
    }

    public IEnumerator EnableLifeStealCoroutine(float duration)
    {
        LifeSteal = true;
        yield return new WaitForSeconds(duration);
        LifeSteal = false;
        _lifeStealCoroutine = null;
    }

    public void OnKillEnemy(int exp)
    {
        AddExp(exp);
    }

    public void AddExp(int exp)
    {
        CurrentExp += exp;
    }

    private void HandleLevel()
    {
        if (CurrentLevel >= 99) return;

        if(CurrentExp >= ExpToNextLevel)
        {
            CurrentLevel++;
            CurrentExp = CurrentExp - ExpToNextLevel;
            ExpToNextLevel = (int)Mathf.Floor(ExpToNextLevel * _expRequirementGrowth) + _expRequirementGrowthOffset;

            OnPlayerLevelUp();
        }
    }

    private void OnPlayerLevelUp()
    {
        QueuedLevels++;

        GameManager.Instance.UpdateGameState(GameState.LevelUpSelect);
    }

    private void HandleOverfillLevels()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        if(QueuedLevels > 0)
        {
            GameManager.Instance.UpdateGameState(GameState.LevelUpSelect);
        }
    }

    public void Cleanse()
    {
        BurnTicks = 0;
        PoisonTicks = 0;

        if(PlayerRegularMoveSpeed > PlayerCurrentMoveSpeed)
        {
            ChangeCurrentMoveSpeed(PlayerRegularMoveSpeed, 0f);
        }
    }
}
