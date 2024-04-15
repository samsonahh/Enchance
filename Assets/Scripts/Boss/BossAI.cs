using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossAI : MonoBehaviour
{
    private PlayerController _playerController;
    private GridManager _gridManager;
    private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _targettedIndicator;

    public BossState _currentState;

    private float _distanceToPlayer;

    private Action<Tile> OnBossStepOnNewTile;
    private Tile _currentTile;
    private Tile _playerTile;
    private List<Tile> _path;

    private bool _isTargetted;

    [Header("Boss UI")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private GameObject _bossIndicator;

    [Header("Boss Stats")]
    [SerializeField] private int _currentHealth = 100;
    [SerializeField] private int _maxHealth = 100;
    private Coroutine _currentMoveSpeedCoroutine;

    [SerializeField] private int _phase = 1;

    private List<Coroutine> _bossStateCoroutines;

    #region Burning
    [Header("Burning")]
    [HideInInspector] public bool IsBurning = false;
    [SerializeField] private float _burnTickDuration = 1f;
    [SerializeField] private int _burnDamage = 1;
    [HideInInspector] public int BurnTicks;
    private IEnumerator _burningEnemyCoroutine;
    private Color _currentColor = Color.white;
    #endregion

    [Header("Poison")]
    [HideInInspector] public bool IsPoisoned = false;
    [SerializeField] private float _poisonTickDuration = 1f;
    [SerializeField] private int _poisonDamage = 1;
    [HideInInspector] public int PoisonTicks;
    private IEnumerator _poisonEnemyCoroutine;

    #region IdleVariables
    [Header("Idle Variables")]
    [SerializeField] private float _activateRange = 10f;
    #endregion

    #region FollowPlayerVariables
    [Header("Follow Player Variables")]
    [SerializeField] private float _followPlayerWaitTime = 1f;
    private float _originalFollowPlayerWaitTime = 1f;
    [SerializeField] private float _followPlayerPatienceLimit = 7f;
    [SerializeField] private float _playerStraightThresholdToSlam = 1.5f;
    [SerializeField] private float _followPlayerSpeedInterval = 0.4f;
    private float _originalFollowPlayerSpeedInterval = 0.4f;
    private float _followPlayerTimer = 0f;
    private float _followPlayerPatienceTimer = 0f;
    private float _playerStraightPathTimer = 0f;
    #endregion

    #region PushPlayerVariables
    [Header("Push Player Variables")]
    [SerializeField] private float _pushStartVelocity = 1f;
    [SerializeField] private float _pushStunDuration = 2f;
    [SerializeField] private int _pushDamage = 3;
    #endregion

    #region SlamVariables
    [Header("Slam Variables")]
    [SerializeField] private float _slamDuration = 1f;
    [SerializeField] private float _slamJumpHeight = 2f;
    [SerializeField] private int _slamDamage = 3;
    [SerializeField] private AudioClip _slamSfx;
    #endregion

    #region SuckVariables
    [Header("Suck Variables")]
    [SerializeField] private GameObject _suckEffectPrefab;
    private GameObject _suckEffect;
    [SerializeField] private float _suckDuration = 5f;
    [SerializeField] private float _suckStrength = 10f;
    private float _suckTimer = 0f;
    [SerializeField] private AudioClip _suckSfx;
    #endregion

    #region BombingRunVariables
    [Header("Bombing Run Variables")]
    [SerializeField] private float _bombingRunJumpCount = 3f;
    [SerializeField] private float _bombingRunStunDuration = 1f;
    [SerializeField] private int _bombingRunDamage = 3;
    #endregion

    #region FloorIsLavaVariables
    [Header("Floor Is Lava Variables")]
    [SerializeField] private float _floorIsLavaDuration = 5f;
    [SerializeField] private int _floorIsLavaDamage = 3;
    [SerializeField] private float _floorIsLavaStunDuration = 1f;
    [SerializeField] private GameObject _lavaBlastParticlePrefab;
    #endregion

    #region GetAngryVariables
    [Header("Get Angry Variables")]
    [SerializeField] private float _getAngryDuration = 2f;
    private bool _getAngryStarted = false;
    #endregion

    private void Awake()
    {
        PlayerController.OnPlayerStepOnNewTile += PlayerController_OnPlayerStepOnNewTile;
        OnBossStepOnNewTile += BossAI_OnBossStepOnNewTile;
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerStepOnNewTile -= PlayerController_OnPlayerStepOnNewTile;
        OnBossStepOnNewTile -= BossAI_OnBossStepOnNewTile;
    }

    private void Start()
    {
        _playerController = PlayerController.Instance;
        _gridManager = GridManager.Instance;
        _animator = GetComponent<Animator>();

        _currentTile = GridManager.Instance._tiles[new Vector2(0, 14)];
        transform.position = _currentTile.transform.position;

        _currentHealth = _maxHealth;
        _healthSlider.maxValue = _maxHealth;
        _healthSlider.value = _currentHealth;

        _currentColor = Color.white;
        _bossStateCoroutines = new List<Coroutine>();

        _originalFollowPlayerWaitTime = _followPlayerWaitTime;
        _originalFollowPlayerSpeedInterval = _followPlayerSpeedInterval;
    }

    private void Update()
    {
        Debug.Log($"{_followPlayerSpeedInterval}, {_followPlayerWaitTime}");

        CalculatePlayerDistance();
        HandleTileChange();
        HandleBossHealth();
        HandleBossIndicator();
        HandleTargetting();

        switch (_currentState)
        {
            case BossState.Idle:

                if (_distanceToPlayer < _activateRange)
                {
                    ChangeBossState(BossState.FollowPlayer);
                }

                if(_currentHealth < _maxHealth)
                {
                    ChangeBossState(BossState.FollowPlayer);
                }

                break;
            case BossState.FollowPlayer:

                _gridManager.ClearPath();

                if (!PlayerController.Instance.IsVisible) return;

                if (_gridManager.IsPathStraight(_path) || _gridManager.IsPathDiagonal(_path))
                {
                    _gridManager.PathTiles(_path);
                    _playerStraightPathTimer += Time.deltaTime;
                }
                else
                {
                    _playerStraightPathTimer = 0f;
                }

                _followPlayerTimer += Time.deltaTime;
                _followPlayerPatienceTimer += Time.deltaTime;

                if (_path.Count == 0) _followPlayerTimer = 0f;

                if(_followPlayerTimer > _followPlayerWaitTime)
                {
                    IEnumerator moving = MoveBoss();
                    StopCoroutine(moving);
                    _bossStateCoroutines.Add(StartCoroutine(moving));
                    _followPlayerTimer = 0f;
                }

                if(_phase == 1)
                {
                    if (_followPlayerPatienceTimer > _followPlayerPatienceLimit)
                    {
                        bool suck = UnityEngine.Random.Range(0, 2) == 1;
                        if (suck)
                        {
                            ChangeBossState(BossState.Suck);
                        }
                        else
                        {
                            ChangeBossState(BossState.SlamOntoPlayer);
                        }
                    }
                }
                else
                {
                    if (_followPlayerPatienceTimer > _followPlayerPatienceLimit / 1.5f)
                    {
                        int randAbility = UnityEngine.Random.Range(0, 6);

                        switch (randAbility)
                        {
                            case 0:
                                ChangeBossState(BossState.Suck);
                                break;
                            case 1:
                                ChangeBossState(BossState.SlamOntoPlayer);
                                break;
                            case 2:
                                ChangeBossState(BossState.BombingRun);
                                break;
                            case 3:
                                ChangeBossState(BossState.BombingRun);
                                break;
                            case 4:
                                ChangeBossState(BossState.FloorIsLava);
                                break;
                            case 5:
                                ChangeBossState(BossState.FloorIsLava);
                                break;
                            default:
                                break;
                        }
                    }
                }

                if(IsPlayerOnBossTiles(_currentTile))
                {
                    ChangeBossState(BossState.PushAway);
                }

                if(_playerStraightPathTimer > (_phase == 1 ? _playerStraightThresholdToSlam : _playerStraightThresholdToSlam / 1.5f))
                {
                    ChangeBossState(BossState.SlamOntoPlayer);
                }

                break;
            case BossState.PushAway:

                break;
            case BossState.SlamOntoPlayer:

                break;
            case BossState.FloorIsLava:

                if (IsPlayerOnBossTiles(_currentTile))
                {
                    ChangeBossState(BossState.PushAway);
                }

                break;
            case BossState.Suck:

                if(_suckEffect == null)
                {
                    _suckEffect = Instantiate(_suckEffectPrefab, transform.position, Quaternion.identity);
                }

                _suckTimer += Time.deltaTime;

                if(_phase == 1)
                {
                    _playerController.transform.position = Vector3.MoveTowards(_playerController.transform.position, transform.position, _suckStrength * Time.deltaTime);
                }
                else
                {
                    _playerController.transform.position = Vector3.MoveTowards(_playerController.transform.position, transform.position, 1.1f * _suckStrength * Time.deltaTime);
                }

                if (_suckTimer > _suckDuration)
                {
                    _animator.SetBool("IsSucking", false);
                    ChangeBossState(BossState.FollowPlayer);
                }

                if (IsPlayerOnBossTiles(_currentTile))
                {
                    _animator.SetBool("IsSucking", false);
                    ChangeBossState(BossState.PushAway);
                }

                break;
            case BossState.BombingRun:

                break;
            case BossState.GetAngry:

                break;
            default:
                break;
        }
    }

    private void ChangeBossState(BossState state)
    {
        _currentState = state;

        foreach(Coroutine coroutine in _bossStateCoroutines)
        {
            StopCoroutine(coroutine);
        }

        _gridManager.ClearPath();
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        Destroy(_suckEffect);
        _suckEffect = null;

        //Debug.Log(state.ToString());

        switch (state)
        {
            case BossState.Idle:

                break;
            case BossState.FollowPlayer:
                _followPlayerTimer = 0f;
                _followPlayerPatienceTimer = 0f;
                _playerStraightPathTimer = 0f;
                break;
            case BossState.PushAway:
                _bossStateCoroutines.Add(StartCoroutine(PushPlayer()));
                break;
            case BossState.SlamOntoPlayer:
                _bossStateCoroutines.Add(StartCoroutine(SlamPlayer(_playerTile)));
                break;
            case BossState.FloorIsLava:
                _bossStateCoroutines.Add(StartCoroutine(FloorIsLava()));
                break;
            case BossState.Suck:
                _suckTimer = 0f;
                _animator.SetBool("IsSucking", true);
                AudioSource.PlayClipAtPoint(_suckSfx, transform.position);
                break;
            case BossState.BombingRun:
                _bossStateCoroutines.Add(StartCoroutine(BombingRun()));
                break;
            case BossState.GetAngry:
                _bossStateCoroutines.Add(StartCoroutine(GetAngry()));
                _phase = 2;
                break;
            default:
                break;
        }
    }

    private void CalculatePlayerDistance()
    {
        _distanceToPlayer = Vector3.Distance(_playerController.transform.position, transform.position);
    }

    protected virtual void HandleTargetting()
    {
        _isTargetted = PlayerController.Instance.Target == gameObject;
        _targettedIndicator.gameObject.SetActive(_isTargetted);
    }

    private void HandleBossHealth()
    {
        _healthText.text = $"{_currentHealth}/{_maxHealth}";
        _healthSlider.value = Mathf.Lerp(_healthSlider.value, _currentHealth, 5f * Time.deltaTime);
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        if (_currentHealth <= 50 && !_getAngryStarted)
        {
            _getAngryStarted = true;

            ChangeBossState(BossState.GetAngry);
        }

        if (_currentHealth <= 0)
        {
            GameManager.Instance.UpdateGameState(GameState.Win);
            Destroy(gameObject);
        }
    }

    private void HandleBossIndicator()
    {
        if (Camera.main == null) return;

        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        Vector3 topScreenPos = Camera.main.WorldToViewportPoint(transform.position + 5.7f * Vector3.up + 5.7f * Vector3.forward);
        bool onScreen = (screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1) || (topScreenPos.x > 0 && topScreenPos.x < 1 && topScreenPos.y > 0 && topScreenPos.y < 1);
        _bossIndicator.SetActive(!onScreen);
        if (!onScreen)
        {
            if(screenPos.z < 0)
            {
                screenPos *= -1;
            }
            Vector3 newScreenPos = new Vector3(Mathf.Clamp(screenPos.x, 0.056f, 1f - 0.056f), Mathf.Clamp(screenPos.y, 0.1f, 0.9f), 0);
            Vector3 uiPos = Camera.main.ViewportToScreenPoint(newScreenPos);
            _bossIndicator.transform.position = uiPos;
        }
    }

    private void HandleTileChange()
    {
        Tile t = GridManager.Instance.GetTileAtPosition(transform.position);
        if (_currentTile != t)
        {
            _currentTile = t;
            OnBossStepOnNewTile?.Invoke(_currentTile);
        }
    }


    private void PlayerController_OnPlayerStepOnNewTile(Tile t)
    {
        _playerTile = t;
        _path = _gridManager.AStarPathFind(_currentTile, _playerTile);
    }

    private void BossAI_OnBossStepOnNewTile(Tile t)
    {
        _path = _gridManager.AStarPathFind(_currentTile, _playerTile);
    }

    private void LookAtPlayer(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) > 0)
        {
            _spriteRenderer.flipX = dir.x < 0;
            _targettedIndicator.flipX = dir.x < 0;
        }
    }

    IEnumerator MoveBoss()
    {
        List<Tile> path = new List<Tile>(_path);

        Vector3 dir = path[0].transform.position - transform.position;
        LookAtPlayer(dir);

        if(_phase == 1)
        {
            for (float timer = 0f; timer < _followPlayerSpeedInterval; timer += Time.deltaTime)
            {
                transform.position = Vector3.Slerp(transform.position, path[0].transform.position, timer / _followPlayerSpeedInterval);
                yield return null;
            }
        }
        else
        {
            for (float timer = 0f; timer < _followPlayerSpeedInterval / 1.5f; timer += Time.deltaTime)
            {
                transform.position = Vector3.Slerp(transform.position, path[0].transform.position, timer / (_followPlayerSpeedInterval / 1.5f));
                yield return null;
            }
        }
        transform.position = path[0].transform.position;
    }

    private bool IsPlayerOnBossTiles(Tile t)
    {
        Tile left = _gridManager.GetTileAtPosition(t.X - 2, t.Y);
        Tile right = _gridManager.GetTileAtPosition(t.X + 2, t.Y);

        if (left == null) left = t;
        if (right == null) right = t;

        return _playerTile == _currentTile || _playerTile == left || _playerTile == right;
    }

    IEnumerator PushPlayer()
    {
        CameraShake.Instance.Shake(0.25f, 0.25f);
        _animator.Play("BossPushAway");

        Vector3 dir = (_playerController.transform.position - transform.position).normalized;

        _playerController.TakeDamage(_pushDamage);
        _playerController.PushPlayer(dir, _pushStunDuration, _pushStartVelocity);

        yield return new WaitForSeconds(1f);

        Tile randTile = _gridManager.GetRandomTileAwayFromPlayer(7.5f);
        Vector3 aboveRandTile = randTile.transform.position;
        dir = randTile.transform.position - transform.position;
        LookAtPlayer(dir);

        for (float timer = 0f; timer < _slamDuration; timer += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(transform.position, aboveRandTile, timer / _slamDuration);
            yield return null;
        }

        transform.position = aboveRandTile;

        ChangeBossState(BossState.FollowPlayer);
    }

    IEnumerator SlamPlayer(Tile t)
    {
        if(t == null)
        {
            ChangeBossState(BossState.FollowPlayer);
            yield break;
        }

        if (_currentTile == null)
        {
            ChangeBossState(BossState.FollowPlayer);
            yield break;
        }

        Tile[] dangerTiles = new Tile[13];
        dangerTiles[0] = _gridManager.GetTileAtPosition(t.X + 2, t.Y + 2);
        dangerTiles[1] = _gridManager.GetTileAtPosition(t.X + 2, t.Y);
        dangerTiles[2] = _gridManager.GetTileAtPosition(t.X + 2, t.Y - 2);
        dangerTiles[3] = _gridManager.GetTileAtPosition(t.X, t.Y - 2);
        dangerTiles[4] = t;
        dangerTiles[5] = _gridManager.GetTileAtPosition(t.X, t.Y + 2);
        dangerTiles[6] = _gridManager.GetTileAtPosition(t.X - 2, t.Y + 2);
        dangerTiles[7] = _gridManager.GetTileAtPosition(t.X - 2, t.Y);
        dangerTiles[8] = _gridManager.GetTileAtPosition(t.X - 2, t.Y - 2);
        if(_phase == 2)
        {
            dangerTiles[9] = _gridManager.GetTileAtPosition(t.X, t.Y - 4);
            dangerTiles[10] = _gridManager.GetTileAtPosition(t.X, t.Y + 4);
            dangerTiles[11] = _gridManager.GetTileAtPosition(t.X - 4, t.Y);
            dangerTiles[12] = _gridManager.GetTileAtPosition(t.X + 4, t.Y);
        }

        foreach(Tile dangerTile in dangerTiles)
        {
            if(dangerTile != null)
            {
                dangerTile.Pathed = true;
            }
        }

        Vector3 abovePlayerTile = t.transform.position + _slamJumpHeight * Vector3.up + _slamJumpHeight * Vector3.forward;
        Vector3 dir = t.transform.position - transform.position;
        LookAtPlayer(dir);

        float timer;
        for (timer = 0f; timer < _slamDuration; timer += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(transform.position, abovePlayerTile, timer / _slamDuration);
            yield return null;
        }
        transform.position = abovePlayerTile;

        for (timer = 0f; timer < 0.2f; timer += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(transform.position, t.transform.position, timer / 0.2f);
            yield return null;
        }
        transform.position = t.transform.position;

        CameraShake.Instance.Shake(0.25f, 0.25f);
        AudioSource.PlayClipAtPoint(_slamSfx, transform.position);
        _animator.Play("BossSquish");

        foreach (Tile dangerTile in dangerTiles)
        {
            if (dangerTile == null) continue;

            if (_playerTile == null)
            {
                continue;
            }

            if (_playerTile == dangerTile)
            {
                _playerController.TakeDamage(_slamDamage);
                _playerController.StunPlayer(_slamDuration + 0.5f);
                break;
            }
        }

        _gridManager.ClearPath();
        yield return new WaitForSeconds(0.5f);

        ChangeBossState(BossState.FollowPlayer);
    }

    IEnumerator BombingRun()
    {
        if(_playerTile == null)
        {
            ChangeBossState(BossState.FollowPlayer);
            yield break;
        }

        if(_currentTile == null)
        {
            ChangeBossState(BossState.FollowPlayer);
            yield break;
        }

        _bossStateCoroutines.Add(StartCoroutine(CheckPlayerBurning(2f)));
        for(int i = 0; i < _bombingRunJumpCount; i++)
        {
            Tile t = _playerTile;

            _gridManager.PathCrossPattern(t);

            Vector3 abovePlayerTile = t.transform.position + _slamJumpHeight * Vector3.up + _slamJumpHeight * Vector3.forward;
            Vector3 dir = t.transform.position - transform.position;
            LookAtPlayer(dir);

            float timer;
            for (timer = 0f; timer < _slamDuration; timer += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(transform.position, abovePlayerTile, timer / _slamDuration);
                yield return null;
            }
            transform.position = abovePlayerTile;

            for (timer = 0f; timer < 0.2f; timer += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(transform.position, t.transform.position, timer / 0.2f);
                yield return null;
            }
            transform.position = t.transform.position;

            CameraShake.Instance.Shake(0.25f, 0.25f);
            _animator.Play("BossSquish");
            AudioSource.PlayClipAtPoint(_slamSfx, transform.position);
            List<Tile> dangerTiles1 = _gridManager.BurnCrossPattern(t);

            foreach (Tile dangerTile in dangerTiles1)
            {
                if (dangerTile == null) continue;

                dangerTile.Burning = true;
                Instantiate(_lavaBlastParticlePrefab, dangerTile.transform.position, Quaternion.Euler(-90, 0, 0));

                if (_playerTile == null)
                {
                    yield return new WaitForSeconds(0.01f);
                    continue;
                }

                if (_playerTile == dangerTile)
                {
                    _playerController.TakeDamage(_bombingRunDamage);
                    _playerController.StunPlayer(_bombingRunStunDuration);
                }

                yield return new WaitForSeconds(0.01f);
            }

            Tile randTile = _gridManager.GetRandomTileAwayFromPlayer(7.5f);

            _gridManager.PathCrossPattern(randTile);

            Vector3 aboveRandTile = randTile.transform.position + _slamJumpHeight * Vector3.up + _slamJumpHeight * Vector3.forward;
            dir = randTile.transform.position - transform.position;
            LookAtPlayer(dir);

            for (timer = 0f; timer < _slamDuration; timer += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(transform.position, aboveRandTile, timer / _slamDuration);
                yield return null;
            }
            transform.position = aboveRandTile;

            for (timer = 0f; timer < 0.2f; timer += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(transform.position, randTile.transform.position, timer / 0.2f);
                yield return null;
            }

            transform.position = randTile.transform.position;

            CameraShake.Instance.Shake(0.25f, 0.25f);
            _animator.Play("BossSquish");
            AudioSource.PlayClipAtPoint(_slamSfx, transform.position);
            List<Tile> dangerTiles2 = _gridManager.BurnCrossPattern(randTile);

            foreach (Tile dangerTile in dangerTiles2)
            {
                if (dangerTile == null) continue;

                dangerTile.Burning = true;
                Instantiate(_lavaBlastParticlePrefab, dangerTile.transform.position, Quaternion.Euler(-90, 0, 0));

                if (_playerTile == null)
                {
                    yield return new WaitForSeconds(0.01f);
                    continue;
                }

                if (_playerTile == dangerTile)
                {
                    _playerController.TakeDamage(_bombingRunDamage);
                    _playerController.StunPlayer(_bombingRunStunDuration);
                }

                yield return new WaitForSeconds(0.01f);
            }
        }

        yield return new WaitForSeconds(0.5f);

        ChangeBossState(BossState.FollowPlayer);
    }

    IEnumerator CheckPlayerBurning(float cooldown)
    {
        while (true)
        {
            if(_playerTile == null)
            {
                yield return null;
                continue;
            }

            if (_playerController.IsInvincible)
            {
                yield return null;
                continue;
            }

            if (_playerTile.Burning)
            {
                _playerController.BurnPlayer(3);

                yield return new WaitForSeconds(cooldown);
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator FloorIsLava()
    {
        if (_playerTile == null)
        {
            ChangeBossState(BossState.FollowPlayer);
            yield break;
        }

        if (_currentTile == null)
        {
            ChangeBossState(BossState.FollowPlayer);
            yield break;
        }

        _bossStateCoroutines.Add(StartCoroutine(CheckPlayerBurning(1f)));

        Vector3 aboveCurrTile = _currentTile.transform.position + 2f * _slamJumpHeight * Vector3.up + 2f* _slamJumpHeight * Vector3.forward;

        _gridManager.PathCheckerBoard(_currentTile.Black);

        float timer;
        for (timer = 0f; timer < 1f; timer += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(transform.position, aboveCurrTile, timer / 1f);
            yield return null;
        }
        transform.position = aboveCurrTile;

        for (timer = 0f; timer < 0.2f; timer += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(transform.position, _currentTile.transform.position, timer / 0.2f);
            yield return null;
        }
        transform.position = _currentTile.transform.position;

        CameraShake.Instance.Shake(0.25f, 0.25f);
        AudioSource.PlayClipAtPoint(_slamSfx, transform.position);
        _animator.Play("BossSquish");

        List<Tile> burnedTiles = _gridManager.BurnCheckerBoard(_currentTile, _currentTile.Black);

        foreach (Tile burnedTile in burnedTiles)
        {
            if (burnedTile == null) continue;

            burnedTile.Burning = true;
            Instantiate(_lavaBlastParticlePrefab, burnedTile.transform.position, Quaternion.Euler(-90, 0, 0));

            if (_playerTile == null)
            {
                yield return new WaitForSeconds(0.0015f);
                continue;
            }

            if (_playerTile == burnedTile)
            {
                _playerController.TakeDamage(_floorIsLavaDamage);
                _playerController.StunPlayer(_floorIsLavaStunDuration);
            }

            yield return new WaitForSeconds(0.0015f);
        }

        yield return new WaitForSeconds(_floorIsLavaDuration);

        ChangeBossState(BossState.FollowPlayer);
    }

    IEnumerator GetAngry()
    {
        _animator.SetBool("IsAngry", true);
        yield return new WaitForSeconds(_getAngryDuration);
        _animator.SetBool("IsAngry", false);

        yield return new WaitForSeconds(0.5f);
        ChangeBossState(BossState.BombingRun);
    }

    public void TakeDamage(int dmg)
    {
        _currentHealth -= dmg;

        _spriteRenderer.color = Color.red;
        StartCoroutine(TakeDamageCoroutine());
    }

    public IEnumerator TakeDamageCoroutine()
    {
        yield return new WaitForSeconds(0.15f);
        _spriteRenderer.color = _currentColor;
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

    public void ChangeCurrentMoveSpeed(float fraction, float duration)
    {
        if (_currentMoveSpeedCoroutine != null)
        {
            _followPlayerSpeedInterval = _originalFollowPlayerSpeedInterval;
            _followPlayerWaitTime = _originalFollowPlayerWaitTime;
            StopCoroutine(_currentMoveSpeedCoroutine);
            _currentMoveSpeedCoroutine = null;
        }
        _currentMoveSpeedCoroutine = StartCoroutine(ChangeCurrentMoveSpeedCoroutine(fraction, duration));
    }

    public IEnumerator ChangeCurrentMoveSpeedCoroutine(float fraction, float duration)
    {
        float multiplier = 1f / fraction;

        _followPlayerSpeedInterval *= multiplier;
        _followPlayerWaitTime *= multiplier;

        yield return new WaitForSeconds(duration);

        _followPlayerSpeedInterval = _originalFollowPlayerSpeedInterval;
        _followPlayerWaitTime = _originalFollowPlayerWaitTime;

        _currentMoveSpeedCoroutine = null;
    }
}

public enum BossState
{
    Idle,
    FollowPlayer,
    PushAway,
    SlamOntoPlayer,
    FloorIsLava,
    Suck,
    BombingRun,
    GetAngry
}
