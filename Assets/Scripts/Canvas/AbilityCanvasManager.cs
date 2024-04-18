using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityCanvasManager : MonoBehaviour
{
    public Image[] AbilityImages;
    public Image[] AbilityBackgrounds;
    public RectTransform SelectedOverlay;
    public TMP_Text AbilityNameText;
    public TMP_Text[] AbilityStarChanceTexts;
    public RectTransform[] CoolDownOverlays = new RectTransform[4];
    public RectTransform AbilityDescriptionPanel;
    public TMP_Text AbilityDescriptionName;
    public TMP_Text AbilityDescriptionStar;
    public TMP_Text AbilityDescription;

    private void Start()
    {
        InitializeOverlays();
    }

    private void Update()
    {
        HandleUI();   
    }

    private void OnEnable()
    {
        AbilityDescriptionPanel.gameObject.SetActive(false);
    }

    private void InitializeOverlays()
    {
        foreach(var overlay in CoolDownOverlays)
        {
            SetRectHeight(overlay, 0);
        }
    }

    private void HandleUI()
    {
        SelectedOverlay.gameObject.SetActive(AbilityCaster.Instance.IsSelectingAbility);

        if (AbilityCaster.Instance.IsSelectingAbility)
        {
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].Star == 1) AbilityNameText.color = GameManager.Instance.StarColors[0];
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].Star == 2) AbilityNameText.color = GameManager.Instance.StarColors[1];
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].Star == 3) AbilityNameText.color = GameManager.Instance.StarColors[2];

            AbilityNameText.text = AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].Name;
        }
        else
        {
            AbilityNameText.text = "";
        }

        AbilityStarChanceTexts[0].text = $"1*: {Mathf.Round(AbilityCaster.Instance.StarChances[0] * 100f)}%";
        AbilityStarChanceTexts[1].text = $"2*: {Mathf.Round(AbilityCaster.Instance.StarChances[1] * 100f)}%";
        AbilityStarChanceTexts[2].text = $"3*: {Mathf.Round((1 - AbilityCaster.Instance.StarChances[0] - AbilityCaster.Instance.StarChances[1]) * 100f)}%";

        if (AbilityDescriptionPanel.gameObject.activeSelf)
        {
            AbilityDescriptionPanel.position = Input.mousePosition;

            AbilityDescriptionName.text = $"Name: {AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Name}";
            AbilityDescriptionStar.text = $"Star: {AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Star}";
            AbilityDescription.text = AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Description;

            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Star == 1)
            {
                AbilityDescriptionName.color = GameManager.Instance.StarColors[0];
                AbilityDescriptionStar.color = GameManager.Instance.StarColors[0];
                AbilityDescription.color = GameManager.Instance.StarColors[0];
            }
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Star == 2)
            {
                AbilityDescriptionName.color = GameManager.Instance.StarColors[1];
                AbilityDescriptionStar.color = GameManager.Instance.StarColors[1];
                AbilityDescription.color = GameManager.Instance.StarColors[1];
            }
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Star == 3)
            {
                AbilityDescriptionName.color = GameManager.Instance.StarColors[2];
                AbilityDescriptionStar.color = GameManager.Instance.StarColors[2];
                AbilityDescription.color = GameManager.Instance.StarColors[2];
            }
        }
    }

    public void SetRectHeight(RectTransform rt, float height)
    {
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
    }
}
