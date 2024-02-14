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

    private void Start()
    {
        InitializeOverlays();
        RandomizeAllAbilities();
    }

    private void Update()
    {
        HandleBumperSelectAbilities();

        if (Input.GetKeyDown(KeyCode.Space)) UseAbility(_selectedAbility);
    }
    private void UseAbility(int index)
    {
        if (_currentAbilities[index].OnCooldown) return;

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
        int randomIndex = Random.Range(0, _abilities.GameAbilities.Length);
        Ability randomAbility = _abilities.GameAbilities[randomIndex];

        _abilityImages[index].sprite = randomAbility.IconSprite;

        _currentAbilities[index] = new Ability(randomAbility);
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

        _selectedBorder.position = _abilityImages[_selectedAbility].rectTransform.position;
    }

    private void InstantiateAbility(string abilityName)
    {
        switch (abilityName)
        {
            case "Fireball":

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
}
