using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCaster : MonoBehaviour
{
    [HideInInspector] public static AbilityCaster Instance;

    [SerializeField] private Abilities _abilities;
    [SerializeField] private Image[] _abilityImages;
    [SerializeField] private RectTransform _selectedOverlay;
    private RectTransform[] _coolDownOverlays = new RectTransform[3];

    private bool _isSelectingAbility = false;
    [HideInInspector] public int SelectedAbility = 0;
    [HideInInspector] public Ability[] CurrentAbilities = new Ability[3];

    [SerializeField] private Transform _projectileArrowPivotTransform;
    [SerializeField] private Transform _castRadiusTransform;
    public Transform CircleCastTransform;
    [SerializeField] private float _aimSpeed;

    public static event Action<int> OnAbilityCast; // 0 - casting, 1 - finished cast

    private void Awake()
    {
        Instance = this;

        OnAbilityCast += HandleOnAbilityCast;
    }

    private void OnDestroy()
    {
        OnAbilityCast -= HandleOnAbilityCast;
    }

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
        if (CurrentAbilities[index].OnCooldown) return;
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
            if (CurrentAbilities[0] != null)
            {
                if (CurrentAbilities[0].Name.Equals(ability.Name))
                {
                    availableAbilities.Remove(ability);
                }
            }
            if (CurrentAbilities[1] != null)
            {
                if (CurrentAbilities[1].Name.Equals(ability.Name))
                {
                    availableAbilities.Remove(ability);
                }
            }
            if (CurrentAbilities[2] != null)
            {
                if (CurrentAbilities[2].Name.Equals(ability.Name))
                {
                    availableAbilities.Remove(ability);
                }
            }
        }

        int randomIndex = UnityEngine.Random.Range(0, availableAbilities.Count);
        Ability randomAbility = availableAbilities[randomIndex];

        _abilityImages[index].sprite = randomAbility.IconSprite;

        CurrentAbilities[index] = Ability.CopyAbility(randomAbility);
    }

    private void RandomizeAllAbilities()
    {
        RandomizeAbility(0);
        RandomizeAbility(1);
        RandomizeAbility(2);
    }

    private IEnumerator AbilityCooldown(int index)
    {
        CurrentAbilities[index].OnCooldown = true;

        CurrentAbilities[index].Timer = CurrentAbilities[index].Cooldown;

        while(CurrentAbilities[index].Timer > 0)
        {
            CurrentAbilities[index].Timer -= Time.deltaTime;

            SetRectHeight(_coolDownOverlays[index], (CurrentAbilities[index].Timer / CurrentAbilities[index].Cooldown) * 150);

            yield return null;
        }

        SetRectHeight(_coolDownOverlays[index], 0);
        CurrentAbilities[index].UseCount--;

        if (CurrentAbilities[index].UseCount == 0) RandomizeAbility(index);

        CurrentAbilities[index].OnCooldown = false;
    }

    private IEnumerator UseAbilityCoroutine(int index)
    {
        PlayerController.Instance.IsCasting = true;
        OnAbilityCast?.Invoke(0);

        yield return new WaitForSeconds(CurrentAbilities[index].CastTime);

        PlayerController.Instance.IsCasting = false;

        OnAbilityCast?.Invoke(1);

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
                if (!CurrentAbilities[0].OnCooldown)
                {
                    SelectedAbility = 0;
                    _isSelectingAbility = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (!CurrentAbilities[1].OnCooldown)
                {
                    SelectedAbility = 1;
                    _isSelectingAbility = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!CurrentAbilities[2].OnCooldown)
                {
                    SelectedAbility = 2;
                    _isSelectingAbility = true;
                }
            }

            return;
        }

        _projectileArrowPivotTransform.localScale = new Vector3(_projectileArrowPivotTransform.localScale.x, _projectileArrowPivotTransform.localScale.y, CurrentAbilities[SelectedAbility].CastRadius);
        _castRadiusTransform.localScale = new Vector3(CurrentAbilities[SelectedAbility].CastRadius, _castRadiusTransform.localScale.y, CurrentAbilities[SelectedAbility].CastRadius);
        CircleCastTransform.localScale = new Vector3(CurrentAbilities[SelectedAbility].CircleCastRadius, CircleCastTransform.localScale.y, CurrentAbilities[SelectedAbility].CircleCastRadius);
        _selectedOverlay.position = _coolDownOverlays[SelectedAbility].transform.parent.position;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(SelectedAbility == 0)
            {
                _isSelectingAbility = false;
                UseAbility(SelectedAbility);
            }
            else
            {
                _isSelectingAbility = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (SelectedAbility == 1)
            {
                _isSelectingAbility = false;
                UseAbility(SelectedAbility);
            }
            else
            {
                _isSelectingAbility = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (SelectedAbility == 2)
            {
                _isSelectingAbility = false;
                UseAbility(SelectedAbility);
            }
            else
            {
                _isSelectingAbility = false;
            }
        }
    }

    private void HandleOnAbilityCast(int i)
    {
        if (i == 0) return;

        if(i == 1)
        {
            if(CurrentAbilities[SelectedAbility].AbilityPrefab != null)
            {
                Instantiate(CurrentAbilities[SelectedAbility].AbilityPrefab);
            }
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
        CircleCastTransform.gameObject.SetActive((CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Circle) && _isSelectingAbility);

        CircleCastTransform.position = PlayerController.Instance.MouseWorldPosition;
        CircleCastTransform.localPosition = Vector3.ClampMagnitude(CircleCastTransform.localPosition, CurrentAbilities[SelectedAbility].CastRadius);
    }

    private void HandleCastRadius()
    {
        _castRadiusTransform.gameObject.SetActive((CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Circle || CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Self) && _isSelectingAbility);
    }

    private void HandleProjectileArrowPivot()
    {
        _projectileArrowPivotTransform.gameObject.SetActive((CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Projectile) && _isSelectingAbility);
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
