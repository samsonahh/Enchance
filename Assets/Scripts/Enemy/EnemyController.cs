using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private PlayerController _playerController;

    [Header("Enemy UI")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Image _enemyHealthFill;

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
    [SerializeField] private float _enemyCurrentMoveSpeed = 5f;
    private float _enemyRegularMoveSpeed;
    #endregion

    #region Burning
    [Header("Burning")]
    [SerializeField] private float _burnTickDuration = 1f;
    [SerializeField] private int _burnDamage = 1;
    [HideInInspector] public int BurnTicks;
    private IEnumerator _burningEnemyCoroutine;
    private Color _currentColor = Color.white;
    [SerializeField] private Color _burningColor;
    #endregion

    #region Health
    [Header("Health")]
    public int CurrentHealth;
    public int MaxHealth;
    [Header("Health Bar Colors")]
    [SerializeField] private Color _greenColor;
    [SerializeField] private Color _yellowColor;
    [SerializeField] private Color _redColor;
    [SerializeField] private float _yellowThreshold = 0.5f;
    [SerializeField] private float _redThreshold = 0.25f;
    #endregion

    private void Awake()
    {
        
    }

    private void OnDestroy()
    {
        
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _playerController = PlayerController.Instance;

        CurrentHealth = MaxHealth;

        _enemyRegularMoveSpeed = _enemyCurrentMoveSpeed;
    }

    void Update()
    {
        ManageEnemyHealth();
        HandleAnimations();

    }

    private void HandleAnimations()
    {

    }

    private void ManageEnemyHealth()
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        HandleHealthBar();

        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void HandleHealthBar()
    {
        float enemyHealthPercentage = (float)CurrentHealth / MaxHealth;

        _healthSlider.value = Mathf.Lerp(_healthSlider.value, enemyHealthPercentage, 5f * Time.deltaTime);

        if (enemyHealthPercentage <= 1f && enemyHealthPercentage > _yellowThreshold)
        {
            _enemyHealthFill.color = Color.Lerp(_enemyHealthFill.color, _greenColor, 5f * Time.deltaTime);
        }
        if (enemyHealthPercentage <= _yellowThreshold && enemyHealthPercentage > _redThreshold)
        {
            _enemyHealthFill.color = Color.Lerp(_enemyHealthFill.color, _yellowColor, 5f * Time.deltaTime);
        }
        if (enemyHealthPercentage <= _redThreshold && enemyHealthPercentage >= 0f)
        {
            _enemyHealthFill.color = Color.Lerp(_enemyHealthFill.color, _redColor, 5f * Time.deltaTime);
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsInvincible) return;

        CurrentHealth -= damage;

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

    public void BurnEnemy(int ticks)
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

    public IEnumerator BurnEnemyCoroutine()
    {
        IsBurning = true;
        _currentColor = _burningColor;

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

}
