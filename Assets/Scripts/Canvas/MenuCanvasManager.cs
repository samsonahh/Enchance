using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuCanvasManager : MonoBehaviour
{
    [SerializeField] private AbilityDescriptionHandler _abilityDescPrefab;

    [SerializeField] private GameObject _mainButtons;
    [SerializeField] private GameObject _abilities;

    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _abilitiesButton;
    [SerializeField] private Button _abilitiesBackButton;

    [SerializeField] private Button _oneStarButton;
    [SerializeField] private Button _twoStarButton;
    [SerializeField] private Button _threeStarButton;

    [SerializeField] private GameObject _oneStarScroll;
    [SerializeField] private GameObject _twoStarScroll;
    [SerializeField] private GameObject _threeStarScroll;

    [SerializeField] private Transform _oneStarContent;
    [SerializeField] private Transform _twoStarContent;
    [SerializeField] private Transform _threeStarContent;

    private void Start()
    {
        PopulateAbilityScrolls();
    }

    private void OnEnable()
    {
        ResetMenus();
    }

    private void PopulateAbilityScrolls()
    {
        List<Ability> one = AbilityCaster.Instance.Abilities.StarSortedAbilities[0];
        List<Ability> two = AbilityCaster.Instance.Abilities.StarSortedAbilities[1];
        List<Ability> three = AbilityCaster.Instance.Abilities.StarSortedAbilities[2];

        foreach(Ability a in one)
        {
            AbilityDescriptionHandler desc = Instantiate(_abilityDescPrefab, _oneStarContent);
            desc.AbilityImage.sprite = a.IconSprite;
            desc.AbilityName.text = a.Name;
            desc.AbilityDesc.text = a.Description;

            desc.AbilityName.color = Color.green;
            desc.AbilityDesc.color = Color.green;
        }

        foreach (Ability a in two)
        {
            AbilityDescriptionHandler desc = Instantiate(_abilityDescPrefab, _twoStarContent);
            desc.AbilityImage.sprite = a.IconSprite;
            desc.AbilityName.text = a.Name;
            desc.AbilityDesc.text = a.Description;

            desc.AbilityName.color = Color.cyan;
            desc.AbilityDesc.color = Color.cyan;
        }

        foreach (Ability a in three)
        {
            AbilityDescriptionHandler desc = Instantiate(_abilityDescPrefab, _threeStarContent);
            desc.AbilityImage.sprite = a.IconSprite;
            desc.AbilityName.text = a.Name;
            desc.AbilityDesc.text = a.Description;

            desc.AbilityName.color = Color.yellow;
            desc.AbilityDesc.color = Color.yellow;
        }
    }

    public void ResetMenus()
    {
        _mainButtons.SetActive(true);
        _abilities.SetActive(false);

        _oneStarScroll.SetActive(true);
        _twoStarScroll.SetActive(false);
        _threeStarScroll.SetActive(false);
    }

    public void UnPause()
    {
        GameManager.Instance.UpdateGameState(GameState.Playing);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void ScaleMenuButtonSize(float size)
    {
        LeanTween.scale(_menuButton.gameObject, new Vector3(size, size, size), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    }

    public void ScaleResumeButtonSize(float size)
    {
        LeanTween.scale(_resumeButton.gameObject, new Vector3(size, size, size), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    }

    public void ScaleAbilitiesButtonSize(float size)
    {
        LeanTween.scale(_abilitiesButton.gameObject, new Vector3(size, size, size), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    }

    public void ScaleAbilitiesBackButtonSize(float size)
    {
        LeanTween.scale(_abilitiesBackButton.gameObject, new Vector3(size, size, size), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    }

    public void ScaleOneStarButtonSize(float size)
    {
        LeanTween.scale(_oneStarButton.gameObject, new Vector3(size, size, size), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    }

    public void ScaleTwoStarButtonSize(float size)
    {
        LeanTween.scale(_twoStarButton.gameObject, new Vector3(size, size, size), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    }

    public void ScaleThreeStarButtonSize(float size)
    {
        LeanTween.scale(_threeStarButton.gameObject, new Vector3(size, size, size), 0.25f).setEase(LeanTweenType.easeOutQuint).setIgnoreTimeScale(true);
    }
}
