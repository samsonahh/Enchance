using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AbilityCaster : NetworkBehaviour
{
    private PlayerController _playerController;

    [Header("Global Abilities")]
    public Abilities Abilities;
    [SerializeField] private Ability _autoAttackAbility;

    [HideInInspector] public bool IsSelectingAbility = false;
    [HideInInspector] public int SelectedAbility;
    [HideInInspector] public Ability[] CurrentAbilities;

    [Header("Canvas")]
    [HideInInspector] public int HoveredAbility;

    [Header("Indicators")]
    [SerializeField] private Transform _projectileArrowPivotTransform;
    [SerializeField] private Transform _castRadiusTransform;
    public Transform CircleCastTransform;

    [Header("Chances")]
    public float[] StarChances = { 0.75f, 0.2f };

    [HideInInspector] public float CooldownReductionMultiplier = 1f;
    [HideInInspector] public float CastTimeReductionMultiplier = 1f;

    public static event Action<int, int> OnAbilityCast; // 0 - casting, 1 - finished cast

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        SortAbilitiesByStar();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            return;
        }

        GameManager.Instance.AbilityCasterInstance = this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void Start()
    {
        if (!IsOwner) return;

        CurrentAbilities = new Ability[4];
        SelectedAbility = 3;

        RandomizeAllAbilities();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        if (!IsOwner) return;

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
        if (GameManager.Instance.PlayerControllerInstance.IsCasting) return;
        if (GameManager.Instance.PlayerControllerInstance.IsStunned) return;
        if (!GameManager.Instance.PlayerControllerInstance.CanCast) return;
        if (AbilityCanvasManager.Instance.AbilityDescriptionPanel.gameObject.activeSelf) return;

        if (CurrentAbilities[index].AbilityType == AbilityType.Closest)
        {
            if (GameManager.Instance.PlayerControllerInstance.Target == null)
            {
                return;
            }
        }

        StartCoroutine(UseAbilityCoroutine(index));
    }

    private void SortAbilitiesByStar()
    {
        if (Abilities.GameAbilities.Length == 0) return;
        if (Abilities.StarSortedAbilities[0].Count != 0) return;

        foreach(Ability a in Abilities.GameAbilities)
        {
            Abilities.StarSortedAbilities[a.Star - 1].Add(a);
        }
    }

    private void RandomizeAbility(int index)
    {
        if(index == 3)
        {
            AbilityCanvasManager.Instance.AbilityImages[index].sprite = _autoAttackAbility.IconSprite;
            CurrentAbilities[index] = Ability.CopyAbility(_autoAttackAbility);
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

            AbilityCanvasManager.Instance.AbilityImages[index].sprite = randomAbility.IconSprite;

            Color starColor = GameManager.Instance.StarColors[randomAbility.Star - 1];
            AbilityCanvasManager.Instance.AbilityBackgrounds[index].color = new Color(starColor.r, starColor.g, starColor.b, 0.2f);

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

                AbilityCanvasManager.Instance.AbilityImages[index].sprite = randomAbility.IconSprite;

                Color starColor = GameManager.Instance.StarColors[randomAbility.Star - 1];
                AbilityCanvasManager.Instance.AbilityBackgrounds[index].color = new Color(starColor.r, starColor.g, starColor.b, 0.2f);

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

        CurrentAbilities[index].Timer = CurrentAbilities[index].Cooldown * CooldownReductionMultiplier;

        while(CurrentAbilities[index].Timer > 0)
        {
            CurrentAbilities[index].Timer -= Time.deltaTime;

            AbilityCanvasManager.Instance.SetRectHeight(AbilityCanvasManager.Instance.CoolDownOverlays[index], (CurrentAbilities[index].Timer / (CurrentAbilities[index].Cooldown * CooldownReductionMultiplier)) * AbilityCanvasManager.Instance.AbilityImages[index].rectTransform.rect.height);

            yield return null;
        }

        AbilityCanvasManager.Instance.SetRectHeight(AbilityCanvasManager.Instance.CoolDownOverlays[index], 0);

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
            GameManager.Instance.PlayerControllerInstance.IsCasting = true;
        }
        OnAbilityCast?.Invoke(0, index);

        yield return new WaitForSeconds(CurrentAbilities[index].CastTime * CastTimeReductionMultiplier);

        GameManager.Instance.PlayerControllerInstance.IsCasting = false;

        OnAbilityCast?.Invoke(1, index);
        HandleOnAbilityCast(index);

        StartCoroutine(AbilityCooldown(index));
    }

    private void SelectAbility()
    {
        if (GameManager.Instance.PlayerControllerInstance.IsCasting) return;

        if (!IsSelectingAbility)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && GameManager.Instance.PlayerControllerInstance.Target != null)
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
        AbilityCanvasManager.Instance.SelectedOverlay.position = AbilityCanvasManager.Instance.CoolDownOverlays[SelectedAbility].transform.parent.position;

        if (Input.GetKeyDown(KeyCode.Mouse0) && !AbilityCanvasManager.Instance.AbilityDescriptionPanel.gameObject.activeSelf)
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

    public void UISelectAbility(int index)
    {
        if (GameManager.Instance.PlayerControllerInstance.IsCasting) return;

        if (!IsSelectingAbility)
        {
            if (!CurrentAbilities[index].OnCooldown)
            {
                SelectedAbility = index;
                IsSelectingAbility = true;
            }
            return;
        }

        if (SelectedAbility == index)
        {
            IsSelectingAbility = false;
            return;
        }
        if (!CurrentAbilities[index].OnCooldown)
        {
            SelectedAbility = index;
        }
    }

    private void HandleOnAbilityCast(int index)
    {
        if (CurrentAbilities[index].AbilityPrefab != null)
        {
            if(index == 3)
            {
                RequestAbilityCastServerRpc(-1);

                FireAbility(-1);

                return;
            }

            RequestAbilityCastServerRpc(CurrentAbilities[index].ID);

            FireAbility(CurrentAbilities[index].ID);
        }
    }

    [ServerRpc]
    private void RequestAbilityCastServerRpc(int ID)
    {
        AbilityCastClientRpc(ID);
    }

    [ClientRpc]
    private void AbilityCastClientRpc(int ID)
    {
        if (!IsOwner) FireAbility(ID);
    }

    private void FireAbility(int ID)
    {
        if(ID == -1)
        {
            var auto = Instantiate(_autoAttackAbility.AbilityPrefab, transform.position, Quaternion.identity);

            auto.GetComponent<AbilityComponent>().Init(_playerController, _playerController.LastForwardDirection, _playerController.LastMouseWorldPosition,
                _playerController.LastCircleWorldPosition, _playerController.LastTarget, _autoAttackAbility.CastRadius, _autoAttackAbility.CircleCastRadius);
            return;
        }

        var ability = Instantiate(Abilities.GameAbilities[ID].AbilityPrefab, transform.position, Quaternion.identity);

        ability.GetComponent<AbilityComponent>().Init(_playerController, _playerController.LastForwardDirection, _playerController.LastMouseWorldPosition,
            _playerController.LastCircleWorldPosition, _playerController.LastTarget, Abilities.GameAbilities[ID].CastRadius, Abilities.GameAbilities[ID].CircleCastRadius);
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

        CircleCastTransform.position = GameManager.Instance.PlayerControllerInstance.MouseWorldPosition;
        CircleCastTransform.localPosition = Vector3.ClampMagnitude(CircleCastTransform.localPosition, CurrentAbilities[SelectedAbility].CastRadius);
    }

    private void HandleCastRadius()
    {
        _castRadiusTransform.gameObject.SetActive((CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Circle || CurrentAbilities[SelectedAbility].AbilityType == AbilityType.Closest) && IsSelectingAbility);
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
    Cone
}
