using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCaster : MonoBehaviour
{
    [SerializeField] private Abilities _abilities;
    [SerializeField] private RectTransform _selectedBorder;
    [SerializeField] private Image[] _abilityImages;
    private RectTransform[] _coolDownOverlays = new RectTransform[3];

    private int _selectedAbility = 0;
    private Ability[] _currentAbilities = new Ability[3];

    [HideInInspector] public float CastRadius;
    private AbilityType _abilityType;

    [SerializeField] private Transform _projectileArrowPivotTransform;
    [SerializeField] private Transform _castRadiusTransform;
    [SerializeField] private Transform _circleCastTransform;
    [SerializeField] private float _aimSpeed;

    private void Start()
    {
        InitializeOverlays();
        RandomizeAllAbilities();

        SetBasedOnNewAbility();
    }

    private void Update()
    {
        HandleBumperSelectAbilities();

        HandleCircleCast();
        HandleCastRadius();
        HandleProjectileArrowPivot();

        if (PlayerController.Instance.UsingController)
        {
            if (Input.GetButtonDown("RightTrigger")) UseAbility(_selectedAbility);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space)) UseAbility(_selectedAbility);
        }
    }
    private void UseAbility(int index)
    {
        if (_currentAbilities[index].OnCooldown) return;

        InstantiateAbility(_currentAbilities[index]);

        StartCoroutine(AbilityCooldown(index));
    }

    private void InitializeOverlays()
    {
        for (int i = 0; i < _abilityImages.Length; i++)
        {
            _coolDownOverlays[i] = _abilityImages[i].transform.GetChild(0).GetComponent<RectTransform>();
            SetRectHeight(_coolDownOverlays[i], 0);
        }

        _selectedBorder.position = _abilityImages[_selectedAbility].rectTransform.position;
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

    private void HandleBumperSelectAbilities()
    {
        if (PlayerController.Instance.UsingController)
        {
            if (Input.GetButtonDown("LeftBumper"))
            {
                if (_selectedAbility == 0)
                {
                    _selectedAbility = 2;
                }
                else
                {
                    _selectedAbility--;
                }
            }

            if (Input.GetButtonDown("RightBumper"))
            {
                if (_selectedAbility == 2)
                {
                    _selectedAbility = 0;
                }
                else
                {
                    _selectedAbility++;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (_selectedAbility == 0)
                {
                    _selectedAbility = 2;
                }
                else
                {
                    _selectedAbility--;
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_selectedAbility == 2)
                {
                    _selectedAbility = 0;
                }
                else
                {
                    _selectedAbility++;
                }
            }
        }

        _selectedBorder.position = _abilityImages[_selectedAbility].rectTransform.position;
        SetBasedOnNewAbility();
    }

    private void SetBasedOnNewAbility()
    {
        _abilityType = _currentAbilities[_selectedAbility].AbilityType;
        CastRadius = _currentAbilities[_selectedAbility].CastRadius;
        _projectileArrowPivotTransform.localScale = new Vector3(_projectileArrowPivotTransform.localScale.x, _projectileArrowPivotTransform.localScale.y, CastRadius);
        _castRadiusTransform.localScale = new Vector3(CastRadius, _castRadiusTransform.localScale.y, CastRadius);
        _circleCastTransform.localScale = new Vector3(_currentAbilities[_selectedAbility].CircleCastRadius, _circleCastTransform.localScale.y, _currentAbilities[_selectedAbility].CircleCastRadius);
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
        _circleCastTransform.gameObject.SetActive((_abilityType == AbilityType.Circle) && !_currentAbilities[_selectedAbility].OnCooldown);

        float x = Input.GetAxisRaw("RightHorizontal");
        float z = Input.GetAxisRaw("RightVertical");

        Vector3 dir = new Vector3(x, 0, z).normalized;

        _circleCastTransform.localPosition += _aimSpeed * Time.deltaTime * dir;

        _circleCastTransform.localPosition = Vector3.ClampMagnitude(_circleCastTransform.localPosition, CastRadius);

        /*
       Vector2 screenPos = Camera.main.WorldToViewportPoint(_playerAimMarker.position);
       screenPos = new Vector2(Mathf.Clamp(screenPos.x, 0, 1), Mathf.Clamp(screenPos.y, 0, 1));

       Plane plane = new Plane(Vector3.up, Vector3.zero);
       Ray ray = Camera.main.ViewportPointToRay(screenPos);
       float distance;
       if (plane.Raycast(ray, out distance))
       {
           Vector3 worldPos = ray.GetPoint(distance);
           //worldPos = Vector3.ClampMagnitude(worldPos, AbilityCaster.Instance.CastRadius);
           _playerAimMarker.position = worldPos;
       }*/
    }

    private void HandleCastRadius()
    {
        _castRadiusTransform.gameObject.SetActive((_abilityType == AbilityType.Circle || _abilityType == AbilityType.Self) && !_currentAbilities[_selectedAbility].OnCooldown);
    }

    private void HandleProjectileArrowPivot()
    {
        _projectileArrowPivotTransform.gameObject.SetActive((_abilityType == AbilityType.Projectile) && !_currentAbilities[_selectedAbility].OnCooldown);
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
