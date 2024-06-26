using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;

public class EnemyController : MonoBehaviour
{
    [SerializeField] protected private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _targettedIndicator;
    protected private NavMeshAgent _navMeshAgent;
    protected private Animator _animator;

    protected virtual float _distanceToPlayer => Vector3.Distance(PlayerController.Instance.transform.position, transform.position);
    protected private Vector3 _startPosition;
    protected virtual float _distanceFromStart => Vector3.Distance(_startPosition, transform.position);

    [Header("Enemy UI")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Image _enemyHealthFill;
    [SerializeField] private TMP_Text _enemyNameText;

    #region Stats
    [Header("Stats")]
    [SerializeField] protected private string _enemyName; 
    [SerializeField] protected private int _expDrop; 
    #endregion

    #region Conditions
    [HideInInspector] public bool IsMoving { get; private set; }
    [HideInInspector] public bool IsCasting;
    [HideInInspector] public bool IsStunned;
    [HideInInspector] public bool IsBurning;
    [HideInInspector] public bool IsPoisoned;
    [HideInInspector] public bool IsInvincible;
    [HideInInspector] public bool IsVisible = true;
    [HideInInspector] public bool CanCast = true;
    [HideInInspector] public bool IsTargetted;
    [HideInInspector] public bool CanBeTargetted = true;
    #endregion

    #region Speed
    [Header("Speed")]
    [SerializeField] public float EnemyCurrentMoveSpeed = 5f;
    public float EnemyRegularMoveSpeed;
    private Coroutine _currentMoveSpeedCoroutine;
    #endregion

    #region Burning
    [Header("Burning")]
    [SerializeField] private float _burnTickDuration = 1f;
    [SerializeField] private int _burnDamage = 1;
    [HideInInspector] public int BurnTicks;
    private IEnumerator _burningEnemyCoroutine;
    private Color _currentColor = Color.white;
    #endregion

    #region Poison
    [Header("Poison")]
    [SerializeField] private float _poisonTickDuration = 1f;
    [SerializeField] private int _poisonDamage = 1;
    [HideInInspector] public int PoisonTicks;
    private IEnumerator _poisonEnemyCoroutine;
    #endregion

    #region Health
    [Header("Health")]
    public int CurrentHealth;
    public int MaxHealth;
    [Header("Health Bar Colors")]
    [SerializeField] private float _yellowThreshold = 0.5f;
    [SerializeField] private float _redThreshold = 0.25f;
    #endregion


    private void Awake()
    {
        
    }

    private void OnDestroy()
    {
        
    }

    protected virtual void OnStart()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();

        CurrentHealth = MaxHealth;
        _enemyNameText.text = _enemyName;

        EnemyRegularMoveSpeed = EnemyCurrentMoveSpeed;
        _navMeshAgent.speed = EnemyCurrentMoveSpeed;
        _navMeshAgent.enabled = false;

        _startPosition = transform.position;
    }

    private void Start()
    {
        OnStart();
    }

    protected virtual void OnUpdate()
    {
        ManageEnemyHealth();
        HandleAnimations();
        HandleTargetting();
    }

    private void Update()
    {
        OnUpdate();
    }

    protected virtual void HandleAnimations()
    {
        _targettedIndicator.sprite = _spriteRenderer.sprite;
    }

    protected virtual void HandleTargetting()
    {
        IsTargetted = PlayerController.Instance.Target == gameObject;
        _targettedIndicator.gameObject.SetActive(IsTargetted);
    }

    protected virtual void ManageEnemyHealth()
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        HandleHealthBar();

        if (CurrentHealth <= 0)
        {
            PlayerController.Instance.OnKillEnemy(_expDrop);
            OnDeath();
        }
    }

    protected virtual void HandleHealthBar()
    {
        float enemyHealthPercentage = (float)CurrentHealth / MaxHealth;

        _healthSlider.value = Mathf.Lerp(_healthSlider.value, enemyHealthPercentage, 5f * Time.deltaTime);

        if (enemyHealthPercentage <= 1f && enemyHealthPercentage > _yellowThreshold)
        {
            _enemyHealthFill.color = Color.Lerp(_enemyHealthFill.color, GameManager.Instance.GreenHealthColor, 5f * Time.deltaTime);
        }
        if (enemyHealthPercentage <= _yellowThreshold && enemyHealthPercentage > _redThreshold)
        {
            _enemyHealthFill.color = Color.Lerp(_enemyHealthFill.color, GameManager.Instance.YellowHealthColor, 5f * Time.deltaTime);
        }
        if (enemyHealthPercentage <= _redThreshold && enemyHealthPercentage >= 0f)
        {
            _enemyHealthFill.color = Color.Lerp(_enemyHealthFill.color, GameManager.Instance.RedHealthColor, 5f * Time.deltaTime);
        }
    }

    protected virtual void LookAtPlayer()
    {
        Vector3 dir = PlayerController.Instance.transform.position - transform.position;
        if (Mathf.Abs(dir.x) > 0) 
        { 
            _spriteRenderer.flipX = dir.x > 0;
            _targettedIndicator.flipX = dir.x > 0;
        }
    }

    protected virtual void LookAt(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        if (Mathf.Abs(dir.x) > 0)
        {
            _spriteRenderer.flipX = dir.x > 0;
            _targettedIndicator.flipX = dir.x > 0;
        }
    }

    protected virtual void HideHealth()
    {
        _healthSlider.transform.parent.gameObject.SetActive(false);
    }

    protected virtual void ShowHealth()
    {
        _healthSlider.transform.parent.gameObject.SetActive(true);
    }

    protected virtual void OnDeath()
    {
        
    }

    protected virtual void OnDamaged()
    {

    }

    protected virtual bool CheckNavMeshPathFinished()
    {
        if (!_navMeshAgent.pathPending)
        {
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                if (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsInvincible) return;

        CurrentHealth -= damage;
        OnDamaged();

        _spriteRenderer.color = Color.red;
        StartCoroutine(TakeDamageCoroutine());
    }

    private IEnumerator TakeDamageCoroutine()
    {
        yield return new WaitForSeconds(0.15f);
        _spriteRenderer.color = _currentColor;
    }

    public virtual void Heal(int hp)
    {
        CurrentHealth += hp;
    }

    public virtual void BurnEnemy(int ticks)
    {
        if (IsBurning)
        {
            BurnTicks += ticks;
            return;
        }

        BurnTicks = ticks;

        if (_burningEnemyCoroutine != null)
        {
            StopCoroutine(_burningEnemyCoroutine);
        }
        _burningEnemyCoroutine = BurnEnemyCoroutine();
        StartCoroutine(_burningEnemyCoroutine);
    }

    private IEnumerator BurnEnemyCoroutine()
    {
        IsBurning = true;
        _currentColor = GameManager.Instance.EntityBurningColor;

        while (BurnTicks > 0)
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

    public void PoisonEnemy(int ticks)
    {
        if (IsPoisoned)
        {
            PoisonTicks += ticks;
            return;
        }

        PoisonTicks = ticks;

        if (_poisonEnemyCoroutine != null)
        {
            StopCoroutine(_poisonEnemyCoroutine);
        }
        _poisonEnemyCoroutine = PoisonEnemyCoroutine();
        StartCoroutine(_poisonEnemyCoroutine);
    }

    public IEnumerator PoisonEnemyCoroutine()
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

    public void StunEnemy(float duration)
    {
        if (IsInvincible) return;

        StartCoroutine(StunEnemyCoroutine(duration));
    }

    public IEnumerator StunEnemyCoroutine(float duration)
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

    public void ChangeCurrentMoveSpeed(float speed, float duration)
    {
        if (_currentMoveSpeedCoroutine != null)
        {
            StopCoroutine(_currentMoveSpeedCoroutine);
            _currentMoveSpeedCoroutine = null;
        }
        _currentMoveSpeedCoroutine = StartCoroutine(ChangeCurrentMoveSpeedCoroutine(speed, duration));
    }

    public IEnumerator ChangeCurrentMoveSpeedCoroutine(float speed, float duration)
    {
        EnemyCurrentMoveSpeed = speed;
        yield return new WaitForSeconds(duration);
        EnemyCurrentMoveSpeed = EnemyRegularMoveSpeed;
        _currentMoveSpeedCoroutine = null;
    }

    public void Cleanse()
    {
        BurnTicks = 0;
        PoisonTicks = 0;

        if (EnemyRegularMoveSpeed > EnemyCurrentMoveSpeed)
        {
            ChangeCurrentMoveSpeed(EnemyRegularMoveSpeed, 0f);
        }
    }

    public virtual void Aggro()
    {

    }

    public virtual bool IsFacingPlayer()
    {
        Vector3 dir = PlayerController.Instance.transform.position - transform.position;
        return _spriteRenderer.flipX == dir.x > 0;
    }

    public static bool IsPointInsideNavMeshSurface(Vector3 pos)
    {
        NavMeshHit hit;

        return NavMesh.SamplePosition(pos, out hit, 0.1f, NavMesh.AllAreas);
    }
}
