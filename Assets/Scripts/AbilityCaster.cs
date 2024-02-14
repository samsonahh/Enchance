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
    private int[] _abilityUseCounts = new int[3];
    private string[] _abilityNames = new string[3];
    private float[] _abilityCooldowns = new float[3];
    private bool[] _abilitiesCanBeUsed = new bool[3];

    private void Start()
    {
        InitializeOverlays();
        RandomizeAllAbilities();
    }

    private void Update()
    {
        HandleBumperSelectAbilities();

        if (Input.GetButtonDown("RightTrigger")) UseAbility(_selectedAbility);
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

    private void UseAbility(int index)
    {
        if (!_abilitiesCanBeUsed[index]) return;

        StartCoroutine(AbilityCooldown(index));
    }

    private void RandomizeAbility(int index)
    {
        int randomIndex = Random.Range(0, _abilities.GameAbilities.Length);
        Ability randomAbility = _abilities.GameAbilities[randomIndex];

        _abilityImages[index].sprite = randomAbility.Sprite;
        _abilityUseCounts[index] = randomAbility.MaxUseCount;
        _abilityNames[index] = randomAbility.Name;
        _abilityCooldowns[index] = randomAbility.Cooldown;
        _abilitiesCanBeUsed[index] = true;
    }

    private void RandomizeAllAbilities()
    {
        RandomizeAbility(0);
        RandomizeAbility(1);
        RandomizeAbility(2);
    }

    private IEnumerator AbilityCooldown(int index)
    {
        _abilitiesCanBeUsed[index] = false;

        float timer = _abilityCooldowns[index];

        while(timer > 0)
        {
            timer -= Time.deltaTime;

            SetRectHeight(_coolDownOverlays[index], (timer / _abilityCooldowns[index]) * 150);

            yield return null;
        }

        SetRectHeight(_coolDownOverlays[index], 0);
        _abilityUseCounts[index]--;

        if (_abilityUseCounts[index] == 0) RandomizeAbility(index);

        _abilitiesCanBeUsed[index] = true;
    }

    private void HandleBumperSelectAbilities()
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

        _selectedBorder.position = _abilityImages[_selectedAbility].rectTransform.position;
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
