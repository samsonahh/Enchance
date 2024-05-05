using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;
using System;

public class Entity : MonoBehaviour
{
    [Header("Entity Base Class")]
    [SerializeField] private protected SpriteRenderer _spriteRenderer;
    private protected Animator _animator;

    #region Conditions
    [HideInInspector] public bool IsMoving { get; private set; }
    [HideInInspector] public bool IsCasting { get; private set; }
    [HideInInspector] public bool IsStunned { get; private set; }
    [HideInInspector] public bool IsBurning { get; private set; }
    [HideInInspector] public bool IsPoisoned { get; private set; }
    [HideInInspector] public bool IsInvincible { get; private set; }
    [HideInInspector] public bool IsShielded { get; private set; }
    [HideInInspector] public bool IsVisible { get; private set; } = true;
    [HideInInspector] public bool CanCast { get; private set; } = true;
    [HideInInspector] public bool LifeSteal { get; private set; }
    [HideInInspector] public bool CanMove { get; private set; } = true;
    [HideInInspector] public bool AutoAttacking { get; private set; }
    #endregion

    #region Speed
    [HideInInspector] public float CurrentMoveSpeed { get; private set; }
    [HideInInspector] public float DefaultMoveSpeed { get; private set; }
    private Coroutine _currentMoveSpeedCoroutine;
    #endregion

    #region LifeSteal
    private Coroutine _lifeStealCoroutine;
    #endregion

    #region Burning
    private float _burnTickDuration = 1f;
    private int _burnDamage = 1;
    [HideInInspector] public int BurnTicks { get; private set; }
    private IEnumerator _burningCoroutine;
    private Color _currentColor = Color.white;
    #endregion

    #region Poison
    private float _poisonTickDuration = 1f;
    private int _poisonDamage = 1;
    [HideInInspector] public int PoisonTicks { get; private set; }
    private IEnumerator _poisonCoroutine;
    #endregion

    #region Health
    [Header("Health")]
    [SerializeField] private float _startRegenThreshold = 5f;
    [SerializeField] private float _regenRate = 3f;
    private float _lastDamagedTimer;
    public int CurrentHealth { get; private set; }
    [field: SerializeField] public int MaxHealth { get; private set; } = 10;
    #endregion

    #region Shield
    private int _currentShieldHealth;
    private Coroutine _shieldCoroutine;
    #endregion

    protected virtual void OnStart()
    {
        _animator = GetComponent<Animator>();

        CurrentHealth = MaxHealth;

        DefaultMoveSpeed = CurrentMoveSpeed;
    }

    void Start()
    {
        OnStart();
    }

    protected virtual void OnUpdate()
    {
        
    }

    void Update()
    {
        OnUpdate();
    }

    protected virtual void HandleAnimations()
    {

    }

    protected virtual void ManageHealth()
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        if(CurrentHealth <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {

    }

    protected virtual void LookAtTargetPosition(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        if (Mathf.Abs(dir.x) > 0)
        {
            _spriteRenderer.flipX = dir.x > 0;
        }
    }

    protected virtual void LookAtDirection(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) > 0)
        {
            _spriteRenderer.flipX = dir.x > 0;
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        if (IsInvincible) return;

        CurrentHealth -= dmg;

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

    public virtual void Burn(int ticks)
    {
        if (IsBurning)
        {
            BurnTicks += ticks;
            return;
        }

        BurnTicks = ticks;

        if (_burningCoroutine != null)
        {
            StopCoroutine(_burningCoroutine);
        }
        _burningCoroutine = BurnCoroutine();
        StartCoroutine(_burningCoroutine);
    }

    private IEnumerator BurnCoroutine()
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

    public void Poison(int ticks)
    {
        if (IsPoisoned)
        {
            PoisonTicks += ticks;
            return;
        }

        PoisonTicks = ticks;

        if (_poisonCoroutine != null)
        {
            StopCoroutine(_poisonCoroutine);
        }
        _poisonCoroutine = PoisonCoroutine();
        StartCoroutine(_poisonCoroutine);
    }

    public IEnumerator PoisonCoroutine()
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

    public void Stun(float duration)
    {
        if (IsInvincible) return;

        StartCoroutine(StunCoroutine(duration));
    }

    public IEnumerator StunCoroutine(float duration)
    {
        IsStunned = true;

        yield return new WaitForSeconds(duration);

        IsStunned = false;
    }

    public void Push(Vector3 dir, float stunDuration, float startVel)
    {
        if (IsInvincible) return;

        StartCoroutine(PushCoroutine(dir, stunDuration, startVel));
    }

    public IEnumerator PushCoroutine(Vector3 dir, float stunDuration, float startVel)
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

    public void Shield(int shieldAmount, float duration)
    {
        if (_shieldCoroutine != null)
        {
            StopCoroutine(_shieldCoroutine);
            _shieldCoroutine = null;
        }
        _shieldCoroutine = StartCoroutine(ShieldCoroutine(shieldAmount, duration));

    }

    public IEnumerator ShieldCoroutine(int shieldAmount, float duration)
    {
        IsShielded = true;
        _currentShieldHealth = shieldAmount;
        yield return new WaitForSeconds(duration);
        _currentShieldHealth = 0;
        IsShielded = false;
        _shieldCoroutine = null;
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

    public void ChangeSpeed(float speed, float duration)
    {
        if (_currentMoveSpeedCoroutine != null)
        {
            StopCoroutine(_currentMoveSpeedCoroutine);
            _currentMoveSpeedCoroutine = null;
        }
        _currentMoveSpeedCoroutine = StartCoroutine(ChangeSpeedCoroutine(speed, duration));

    }

    public IEnumerator ChangeSpeedCoroutine(float speed, float duration)
    {
        CurrentMoveSpeed = speed;
        yield return new WaitForSeconds(duration);
        CurrentMoveSpeed = DefaultMoveSpeed;
        _currentMoveSpeedCoroutine = null;
    }

    public void Cleanse()
    {
        BurnTicks = 0;
        PoisonTicks = 0;

        if(DefaultMoveSpeed > CurrentMoveSpeed)
        {
            ChangeSpeed(DefaultMoveSpeed, 0);
        }
    }
}
