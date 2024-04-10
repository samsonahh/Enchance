using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityCanvasManager : MonoBehaviour
{
    public Image[] AbilityImages;
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
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].Star == 1) AbilityNameText.color = Color.green;
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].Star == 2) AbilityNameText.color = Color.cyan;
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].Star == 3) AbilityNameText.color = Color.yellow;

            AbilityNameText.text = AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].Name;
        }
        else
        {
            AbilityNameText.text = "";
        }

        AbilityStarChanceTexts[0].text = $"1*: {Mathf.Round(AbilityCaster.Instance.StarChances[0] * 100f)}%";
        AbilityStarChanceTexts[1].text = $"2*: {Mathf.Round(AbilityCaster.Instance.StarChances[1] * 100f)}%";
        AbilityStarChanceTexts[2].text = $"3*: {Mathf.Round(AbilityCaster.Instance.StarChances[2] * 100f)}%";

        if (AbilityDescriptionPanel.gameObject.activeSelf)
        {
            AbilityDescriptionPanel.position = Input.mousePosition;

            AbilityDescriptionName.text = $"Name: {AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Name}";
            AbilityDescriptionStar.text = $"Star: {AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Star}";
            AbilityDescription.text = AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Description;

            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Star == 1)
            {
                AbilityDescriptionName.color = Color.green;
                AbilityDescriptionStar.color = Color.green;
                AbilityDescription.color = Color.green;
            }
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Star == 2)
            {
                AbilityDescriptionName.color = Color.cyan;
                AbilityDescriptionStar.color = Color.cyan;
                AbilityDescription.color = Color.cyan;
            }
            if (AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.HoveredAbility].Star == 3)
            {
                AbilityDescriptionName.color = Color.yellow;
                AbilityDescriptionStar.color = Color.yellow;
                AbilityDescription.color = Color.yellow;
            }
        }
    }

    public void SetRectHeight(RectTransform rt, float height)
    {
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
    }
}
