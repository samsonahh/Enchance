using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCaster : MonoBehaviour
{
    [SerializeField] private Abilities _abilities;
    [SerializeField] private Image[] _abilityImages;
    [SerializeField] private RectTransform _selectedOverlay;
    private RectTransform[] _coolDownOverlays = new RectTransform[3];

    private bool _isSelectingAbility = false;
    private int _selectedAbility = 0;
    private Ability[] _currentAbilities = new Ability[3];

    [SerializeField] private Transform _projectileArrowPivotTransform;
    [SerializeField] private Transform _castRadiusTransform;
    [SerializeField] private Transform _circleCastTransform;
    [SerializeField] private float _aimSpeed;

    private void Start()
    {
        InitializeOverlays();
        RandomizeAllAbilities();
    }

    private void Update()
    {
        HandleCircleCast();
        HandleCastRadius();
        HandleProjectileArrowPivot();

        SelectAbility();
    }
    private void UseAbility(int index)
    {
        if (_currentAbilities[index].OnCooldown) return;
        if (PlayerController.Instance.IsCasting) return;

        StartCoroutine(UseAbilityCoroutine(index));
    }

    private void InitializeOverlays()
    {
        for (int i = 0; i < _abilityImages.Length; i++)
        {
            _coolDownOverlays[i] = _abilityImages[i].transform.GetChild(0).GetComponent<RectTransform>();
            SetRectHeight(_coolDownOverlays[i], 0);
        }
    }

    private void SetRectHeight(RectTransform rt, float height)
    {
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
    }

    private void RandomizeAbility(int index)
    {
        List<Ability> availableAbilities = new List<Ability>();
        foreach (Ability ability in _abilities.GameAbilities)
        {
            availableAbilities.Add(ability);
            if (_currentAbilities[0] != null)
            {
                if (_currentAbilities[0].Name.Equals(ability.Name))
                {
                    availableAbilities.Remove(ability);
                }
            }
            if (_currentAbilities[1] != null)
            {
                if (_currentAbilities[1].Name.Equals(ability.Name))
                {
                    availableAbilities.Remove(ability);
                }
            }
            if (_currentAbilities[2] != null)
            {
                if (_currentAbilities[2].Name.Equals(ability.Name))
                {
                    availableAbilities.Remove(ability);
                }
            }
        }

        int randomIndex = Random.Range(0, availableAbilities.Count);
        Ability randomAbility = availableAbilities[randomIndex];

        _abilityImages[index].sprite = randomAbility.IconSprite;

        _currentAbilities[index] = Ability.CopyAbility(randomAbility);
    }

    private void RandomizeAllAbilities()
    {
        RandomizeAbility(0);
        RandomizeAbility(1);
        RandomizeAbility(2);
    }

    private IEnumerator AbilityCooldown(int index)
    {
        _currentAbilities[index].OnCooldown = true;

        _currentAbilities[index].Timer = _currentAbilities[index].Cooldown;

        while(_currentAbilities[index].Timer > 0)
        {
            _currentAbilities[index].Timer -= Time.deltaTime;

            SetRectHeight(_coolDownOverlays[index], (_currentAbilities[index].Timer / _currentAbilities[index].Cooldown) * 150);

            yield return null;
        }

        SetRectHeight(_coolDownOverlays[index], 0);
        _currentAbilities[index].UseCount--;

        if (_currentAbilities[index].UseCount == 0) RandomizeAbility(index);

        _currentAbilities[index].OnCooldown = false;
    }

    private IEnumerator UseAbilityCoroutine(int index)
    {
        PlayerController.Instance.IsCasting = true;

        yield return new WaitForSeconds(_currentAbilities[index].CastTime);

        PlayerController.Instance.IsCasting = false;

        InstantiateAbility(_currentAbilities[index]);

        StartCoroutine(AbilityCooldown(index));
    }

    private void SelectAbility()
    {
        _selectedOverlay.gameObject.SetActive(_isSelectingAbility);

        if (PlayerController.Instance.IsCasting) return;

        

        if (!_isSelectingAbility)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!_currentAbilities[0].OnCooldown)
                {
                    _selectedAbility = 0;
                    _isSelectingAbility = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (!_currentAbilities[1].OnCooldown)
                {
                    _selectedAbility = 1;
                    _isSelectingAbility = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!_currentAbilities[2].OnCooldown)
                {
                    _selectedAbility = 2;
                    _isSelectingAbility = true;
                }
            }

            return;
        }

        _projectileArrowPivotTransform.localScale = new Vector3(_projectileArrowPivotTransform.localScale.x, _projectileArrowPivotTransform.localScale.y, _currentAbilities[_selectedAbility].CastRadius);
        _castRadiusTransform.localScale = new Vector3(_currentAbilities[_selectedAbility].CastRadius, _castRadiusTransform.localScale.y, _currentAbilities[_selectedAbility].CastRadius);
        _circleCastTransform.localScale = new Vector3(_currentAbilities[_selectedAbility].CircleCastRadius, _circleCastTransform.localScale.y, _currentAbilities[_selectedAbility].CircleCastRadius);
        _selectedOverlay.position = _coolDownOverlays[_selectedAbility].transform.parent.position;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(_selectedAbility == 0)
            {
                _isSelectingAbility = false;
                UseAbility(_selectedAbility);
            }
            else
            {
                _isSelectingAbility = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (_selectedAbility == 1)
            {
                _isSelectingAbility = false;
                UseAbility(_selectedAbility);
            }
            else
            {
                _isSelectingAbility = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_selectedAbility == 2)
            {
                _isSelectingAbility = false;
                UseAbility(_selectedAbility);
            }
            else
            {
                _isSelectingAbility = false;
            }
        }
    }

    private void InstantiateAbility(Ability ability)
    {
        switch (ability.Name)
        {
            case "Fireball":
                Fireball fireball = (Fireball)Instantiate(ability.AbilityPrefab, transform.position, Quaternion.identity);
                fireball.Init(this);
                fireball.SetDirection(PlayerController.Instance.ForwardDirection);
                break;
            case "Magic Bomb":
                MagicBomb bomb = (MagicBomb)Instantiate(ability.AbilityPrefab, transform.position, Quaternion.identity);
                bomb.Init(this);
                break;
            default:
                break;
        }
    }

    private void DebugInputs()
    {
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                Debug.Log("Key pressed: " + keyCode);
            }
        }
    }

    private void HandleCircleCast()
    {
        _circleCastTransform.gameObject.SetActive((_currentAbilities[_selectedAbility].AbilityType == AbilityType.Circle) && _isSelectingAbility);

        _circleCastTransform.position = PlayerController.Instance.MouseWorldPosition;
    }

    private void HandleCastRadius()
    {
        _castRadiusTransform.gameObject.SetActive((_currentAbilities[_selectedAbility].AbilityType == AbilityType.Circle || _currentAbilities[_selectedAbility].AbilityType == AbilityType.Self) && _isSelectingAbility);
    }

    private void HandleProjectileArrowPivot()
    {
        _projectileArrowPivotTransform.gameObject.SetActive((_currentAbilities[_selectedAbility].AbilityType == AbilityType.Projectile) && _isSelectingAbility);
    }
}

public enum AbilityType
{
    Projectile,
    Circle,
    Self,
    Closest,
    Beam,
}
