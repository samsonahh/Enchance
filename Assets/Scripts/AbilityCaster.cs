using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityCaster : MonoBehaviour
{
    [HideInInspector] public static AbilityCaster Instance;

    public Abilities Abilities;

    private bool _isSelectingAbility = false;
    [HideInInspector] public int SelectedAbility = 0;
    [HideInInspector] public Ability[] CurrentAbilities = new Ability[3];

    [SerializeField] private Transform _projectileArrowPivotTransform;
    [SerializeField] private Transform _castRadiusTransform;
    public Transform CircleCastTransform;

    [Header("Chances")]
    [SerializeField] private float[] _starChances = { 0.6f, 0.3f, 0.1f };

    public static event Action<int> OnAbilityCast; // 0 - casting, 1 - finished cast

    #region UI
    [Header("UI")]
    [SerializeField] private Image[] _abilityImages;
    [SerializeField] private RectTransform _selectedOverlay;
    [SerializeField] private TMP_Text _abilityNameText;
    [SerializeField] private TMP_Text[] _abilityStarChanceTexts;
    private RectTransform[] _coolDownOverlays = new RectTransform[3];
    [SerializeField] private RectTransform _abilityDescriptionPanel;
    [SerializeField] private TMP_Text _abilityDescriptionName;
    [SerializeField] private TMP_Text _abilityDescriptionStar;
    [SerializeField] private TMP_Text _abilityDescription;
    private int _hoveredAbility;
    #endregion

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
        InitializeOverlays();
        RandomizeAllAbilities();
    }

    private void Update()
    {
        HandleCircleCast();
        HandleCastRadius();
        HandleProjectileArrowPivot();
        HandleUI();

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
        while (true)
        {
            float randomStarFraction = UnityEngine.Random.Range(0f, 1f);
            int randomStar = 0;
            if(randomStarFraction > _starChances[0])
            {
                randomStar = 1;
            }
            if (randomStarFraction > _starChances[0] + _starChances[1])
            {
                randomStar = 2;
            }

            //Debug.Log($"Rolled a {randomStarFraction} resulting in a {randomStar + 1} ability");

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
                //Debug.LogError($"Out of {randomStar + 1} star abilities!");
                continue;
            }

            int randomIndex = UnityEngine.Random.Range(0, availableAbilities.Count);
            Ability randomAbility = availableAbilities[randomIndex];

            _abilityImages[index].sprite = randomAbility.IconSprite;

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

                _abilityImages[index].sprite = randomAbility.IconSprite;

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
        if (PlayerController.Instance.IsCasting) return;

        if (!_isSelectingAbility)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (!CurrentAbilities[0].OnCooldown)
                {
                    SelectedAbility = 0;
                    _isSelectingAbility = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (!CurrentAbilities[1].OnCooldown)
                {
                    SelectedAbility = 1;
                    _isSelectingAbility = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
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

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _isSelectingAbility = false;
            UseAbility(SelectedAbility);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(SelectedAbility == 0)
            {
                _isSelectingAbility = false;
            }
            SelectedAbility = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (SelectedAbility == 1)
            {
                _isSelectingAbility = false;
            }
            SelectedAbility = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (SelectedAbility == 2)
            {
                _isSelectingAbility = false;
            }
            SelectedAbility = 2;
        }
    }

    private void HandleOnAbilityCast(int i)
    {
        if (i == 0) return;

        if(i == 1)
        {
            if(CurrentAbilities[SelectedAbility].AbilityPrefab != null)
            {
                Instantiate(CurrentAbilities[SelectedAbility].AbilityPrefab, transform.position, Quaternion.identity);
            }
        }  
    }

    private void HandleUI()
    {
        _selectedOverlay.gameObject.SetActive(_isSelectingAbility);

        if (_isSelectingAbility)
        {
            if (CurrentAbilities[SelectedAbility].Star == 1) _abilityNameText.color = Color.green;
            if (CurrentAbilities[SelectedAbility].Star == 2) _abilityNameText.color = Color.cyan;
            if (CurrentAbilities[SelectedAbility].Star == 3) _abilityNameText.color = Color.yellow;

            _abilityNameText.text = CurrentAbilities[SelectedAbility].Name;
        }
        else
        {
            _abilityNameText.text = "";
        }

        _abilityStarChanceTexts[0].text = $"1*: {Mathf.Round(_starChances[0]*100f)}%";
        _abilityStarChanceTexts[1].text = $"2*: {Mathf.Round(_starChances[1]*100f)}%";
        _abilityStarChanceTexts[2].text = $"3*: {Mathf.Round(_starChances[2]*100f)}%";

        if (_abilityDescriptionPanel.gameObject.activeSelf)
        {
            _abilityDescriptionPanel.position = Input.mousePosition;

            _abilityDescriptionName.text = $"Name: {CurrentAbilities[_hoveredAbility].Name}";
            _abilityDescriptionStar.text = $"Star: {CurrentAbilities[_hoveredAbility].Star}";
            _abilityDescription.text = CurrentAbilities[_hoveredAbility].Description;

            if (CurrentAbilities[_hoveredAbility].Star == 1)
            {
                _abilityDescriptionName.color = Color.green;
                _abilityDescriptionStar.color = Color.green;
                _abilityDescription.color = Color.green;
            }
            if (CurrentAbilities[_hoveredAbility].Star == 2)
            {
                _abilityDescriptionName.color = Color.cyan;
                _abilityDescriptionStar.color = Color.cyan;
                _abilityDescription.color = Color.cyan;
            }
            if (CurrentAbilities[_hoveredAbility].Star == 3)
            {
                _abilityDescriptionName.color = Color.yellow;
                _abilityDescriptionStar.color = Color.yellow;
                _abilityDescription.color = Color.yellow;
            }
        }
    }

    public void AssignHoveredAbility(int index)
    {
        _hoveredAbility = index;
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
