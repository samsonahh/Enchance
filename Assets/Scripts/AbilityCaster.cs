using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCaster : MonoBehaviour
{
    [HideInInspector] public static AbilityCaster Instance;

    [Header("Global Abilities")]
    public Abilities Abilities;
    [SerializeField] private Ability _meleeAttackAbility;

    [HideInInspector] public bool IsSelectingAbility = false;
    [HideInInspector] public int SelectedAbility;
    [HideInInspector] public Ability[] CurrentAbilities;

    [Header("Canvas")]
    [SerializeField] private AbilityCanvasManager _abilityCanvas;
    [HideInInspector] public int HoveredAbility;

    [Header("Indicators")]
    [SerializeField] private Transform _projectileArrowPivotTransform;
    [SerializeField] private Transform _castRadiusTransform;
    public Transform CircleCastTransform;

    [Header("Chances")]
    public float[] StarChances = { 0.75f, 0.2f, 0.05f };

    public static event Action<int, int> OnAbilityCast; // 0 - casting, 1 - finished cast

    private void Awake()
    {
        Instance = this;

        OnAbilityCast += HandleOnAbilityCast;
        SortAbilitiesByStar();
    }

    private void OnDestroy()
    {
        OnAbilityCast -= HandleOnAbilityCast;
    }

    private void Start()
    {
        CurrentAbilities = new Ability[4];
        SelectedAbility = 3;

        RandomizeAllAbilities();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        HandleCircleCast();
        HandleCastRadius();
        HandleProjectileArrowPivot();

        Cheats();
        SelectAbility();
    }

    private void Cheats()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha1))
        {
            RandomizeAllAbilitiesByStarCheat(0);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha2))
        {
            RandomizeAllAbilitiesByStarCheat(1);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha3))
        {
            RandomizeAllAbilitiesByStarCheat(2);
        }
    }

    private void UseAbility(int index)
    {
        if (CurrentAbilities[index].OnCooldown) return;
        if (PlayerController.Instance.IsCasting) return;
        if (PlayerController.Instance.IsStunned) return;
        if (!PlayerController.Instance.CanCast) return;

        StartCoroutine(UseAbilityCoroutine(index));
    }

    private void SortAbilitiesByStar()
    {
        if (Abilities.GameAbilities.Length == 0) return;

        foreach(Ability a in Abilities.GameAbilities)
        {
            Abilities.StarSortedAbilities[a.Star - 1].Add(a);
        }
    }

    private void RandomizeAbility(int index)
    {
        if(index == 3)
        {
            _abilityCanvas.AbilityImages[index].sprite = _meleeAttackAbility.IconSprite;
            CurrentAbilities[index] = _meleeAttackAbility;
            return;
        }

        while (true)
        {
            float randomStarFraction = UnityEngine.Random.Range(0f, 1f);
            int randomStar = 0;
            if(randomStarFraction > StarChances[0])
            {
                randomStar = 1;
            }
            if (randomStarFraction > StarChances[0] + StarChances[1])
            {
                randomStar = 2;
            }

            List<Ability> availableAbilities = new List<Ability>();
            foreach (Ability ability in Abilities.StarSortedAbilities[randomStar])
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

            if(availableAbilities.Count == 0)
            {
                continue;
            }

            int randomIndex = UnityEngine.Random.Range(0, availableAbilities.Count);
            Ability randomAbility = availableAbilities[randomIndex];

            _abilityCanvas.AbilityImages[index].sprite = randomAbility.IconSprite;

            CurrentAbilities[index] = Ability.CopyAbility(randomAbility);
            break;
        }
    }

    private void RandomizeAllAbilitiesByStarCheat(int star)
    {
        for (int index = 0; index < 3; index++)
        {
            while (true)
            {
                List<Ability> availableAbilities = new List<Ability>();
                foreach (Ability ability in Abilities.StarSortedAbilities[star])
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

                if (availableAbilities.Count == 0)
                {
                    //Debug.LogError($"Out of {randomStar + 1} star abilities!");
                    break;
                }

                int randomIndex = UnityEngine.Random.Range(0, availableAbilities.Count);
                Ability randomAbility = availableAbilities[randomIndex];

                _abilityCanvas.AbilityImages[index].sprite = randomAbility.IconSprite;

                CurrentAbilities[index] = Ability.CopyAbility(randomAbility);
                break;
            }
        }
    }

    private void RandomizeAllAbilities()
    {
        RandomizeAbility(0);
        RandomizeAbility(1);
        RandomizeAbility(2);
        RandomizeAbility(3);
    }

    private IEnumerator AbilityCooldown(int index)
    {
        CurrentAbilities[index].OnCooldown = true;

        CurrentAbilities[index].Timer = CurrentAbilities[index].Cooldown;

        while(CurrentAbilities[index].Timer > 0)
        {
            CurrentAbilities[index].Timer -= Time.deltaTime;

            _abilityCanvas.SetRectHeight(_abilityCanvas.CoolDownOverlays[index], (CurrentAbilities[index].Timer / CurrentAbilities[index].Cooldown) * _abilityCanvas.AbilityImages[index].rectTransform.rect.height);

            yield return null;
        }

        _abilityCanvas.SetRectHeight(_abilityCanvas.CoolDownOverlays[index], 0);

        if(index != 3)
        {
            CurrentAbilities[index].UseCount--;
        }
        
        if (CurrentAbilities[index].UseCount == 0) RandomizeAbility(index);

        CurrentAbilities[index].OnCooldown = false;
    }

    private IEnumerator UseAbilityCoroutine(int index)
    {
        if(CurrentAbilities[index].CastTime != 0)
        {
            PlayerController.Instance.IsCasting = true;
        }
        OnAbilityCast?.Invoke(0, index);

        yield return new WaitForSeconds(CurrentAbilities[index].CastTime);

        PlayerController.Instance.IsCasting = false;

        OnAbilityCast?.Invoke(1, index);

        StartCoroutine(AbilityCooldown(index));
    }

    private void SelectAbility()
    {
        if (PlayerController.Instance.IsCasting) return;

        if (!IsSelectingAbility)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && PlayerController.Instance.Target != null)
            {
                UseAbility(3);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (!CurrentAbilities[0].OnCooldown)
                {
                    SelectedAbility = 0;
                    IsSelectingAbility = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (!CurrentAbilities[1].OnCooldown)
                {
                    SelectedAbility = 1;
                    IsSelectingAbility = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (!CurrentAbilities[2].OnCooldown)
                {
                    SelectedAbility = 2;
                    IsSelectingAbility = true;
                }
            }

            return;
        }

        _projectileArrowPivotTransform.localScale = new Vector3(_projectileArrowPivotTransform.localScale.x, _projectileArrowPivotTransform.localScale.y, CurrentAbilities[SelectedAbility].CastRadius);
        _castRadiusTransform.localScale = new Vector3(CurrentAbilities[SelectedAbility].CastRadius, _castRadiusTransform.localScale.y, CurrentAbilities[SelectedAbility].CastRadius);
        CircleCastTransform.localScale = new Vector3(CurrentAbilities[SelectedAbility].CircleCastRadius, CircleCastTransform.localScale.y, CurrentAbilities[SelectedAbility].CircleCastRadius);
        _abilityCanvas.SelectedOverlay.position = _abilityCanvas.CoolDownOverlays[SelectedAbility].transform.parent.position;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            IsSelectingAbility = false;
            UseAbility(SelectedAbility);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(SelectedAbility == 0)
            {
                IsSelectingAbility = false;
            }
            SelectedAbility = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (SelectedAbility == 1)
            {
                IsSelectingAbility = false;
            }
            SelectedAbility = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (SelectedAbility == 2)
            {
                IsSelectingAbility = false;
            }
            SelectedAbility = 2;
        }
    }

    private void HandleOnAbilityCast(int i, int index)
    {
        if (i == 0) return;

        if(i == 1)
        {
            if(CurrentAbilities[index].AbilityPrefab != null)
            {
                Instantiate(CurrentAbilities[index].AbilityPrefab, transform.position, Quaternion.identity);
            }
        }  
    }

    public void AssignHoveredAbility(int index)
    {
        HoveredAbility = index;
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
        CircleCastTransform.gameObject.SetActive((CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Circle) && IsSelectingAbility);

        CircleCastTransform.position = PlayerController.Instance.MouseWorldPosition;
        CircleCastTransform.localPosition = Vector3.ClampMagnitude(CircleCastTransform.localPosition, CurrentAbilities[SelectedAbility].CastRadius);
    }

    private void HandleCastRadius()
    {
        _castRadiusTransform.gameObject.SetActive((CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Circle || CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Self) && IsSelectingAbility);
    }

    private void HandleProjectileArrowPivot()
    {
        _projectileArrowPivotTransform.gameObject.SetActive((CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Projectile) && IsSelectingAbility);
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
