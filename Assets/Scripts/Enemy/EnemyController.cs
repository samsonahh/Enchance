using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyController : MonoBehaviour
{
    [SerializeField] protected private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _targettedIndicator;
    protected private Animator _animator;

    protected virtual float _distanceToPlayer => Vector3.Distance(PlayerController.Instance.transform.position, transform.position);

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
    [HideInInspector] public bool IsInvincible;
    [HideInInspector] public bool IsVisible = true;
    [HideInInspector] public bool CanCast = true;
    [HideInInspector] public bool IsTargetted;
    #endregion

    #region Speed
    [Header("Speed")]
    [SerializeField] protected private float _enemyCurrentMoveSpeed = 5f;
    protected private float _enemyRegularMoveSpeed;
    #endregion

    #region Burning
    [Header("Burning")]
    [SerializeField] private float _burnTickDuration = 1f;
    [SerializeField] private int _burnDamage = 1;
    [HideInInspector] public int BurnTicks;
    private IEnumerator _burningEnemyCoroutine;
    private Color _currentColor = Color.white;
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

        CurrentHealth = MaxHealth;
        _enemyNameText.text = _enemyName;

        _enemyRegularMoveSpeed = _enemyCurrentMoveSpeed;
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

    protected virtual void OnDeath()
    {

    }

    public virtual void TakeDamage(int damage)
    {
        if (IsInvincible) return;

        CurrentHealth -= damage;

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
}
