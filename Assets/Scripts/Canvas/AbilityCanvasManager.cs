using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityCanvasManager : MonoBehaviour
{
    public static AbilityCanvasManager Instance;

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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeOverlays();
    }

    private void Update()
    {
        if (GameManager.Instance.PlayerControllerInstance == null) return;
        
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
        SelectedOverlay.gameObject.SetActive(GameManager.Instance.AbilityCasterInstance.IsSelectingAbility);

        if (GameManager.Instance.AbilityCasterInstance.IsSelectingAbility)
        {
            if (GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.SelectedAbility].Star == 1) AbilityNameText.color = GameManager.Instance.StarColors[0];
            if (GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.SelectedAbility].Star == 2) AbilityNameText.color = GameManager.Instance.StarColors[1];
            if (GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.SelectedAbility].Star == 3) AbilityNameText.color = GameManager.Instance.StarColors[2];

            AbilityNameText.text = GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.SelectedAbility].Name;
        }
        else
        {
            AbilityNameText.text = "";
        }

        AbilityStarChanceTexts[0].text = $"1*: {Mathf.Round(GameManager.Instance.AbilityCasterInstance.StarChances[0] * 100f)}%";
        AbilityStarChanceTexts[1].text = $"2*: {Mathf.Round(GameManager.Instance.AbilityCasterInstance.StarChances[1] * 100f)}%";
        AbilityStarChanceTexts[2].text = $"3*: {Mathf.Round((1 - GameManager.Instance.AbilityCasterInstance.StarChances[0] - GameManager.Instance.AbilityCasterInstance.StarChances[1]) * 100f)}%";

        if (AbilityDescriptionPanel.gameObject.activeSelf)
        {
            AbilityDescriptionPanel.position = Input.mousePosition;

            AbilityDescriptionName.text = $"Name: {GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.HoveredAbility].Name}";
            AbilityDescriptionStar.text = $"Star: {GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.HoveredAbility].Star}";
            AbilityDescription.text = GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.HoveredAbility].Description;

            if (GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.HoveredAbility].Star == 1)
            {
                AbilityDescriptionName.color = GameManager.Instance.StarColors[0];
                AbilityDescriptionStar.color = GameManager.Instance.StarColors[0];
                AbilityDescription.color = GameManager.Instance.StarColors[0];
            }
            if (GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.HoveredAbility].Star == 2)
            {
                AbilityDescriptionName.color = GameManager.Instance.StarColors[1];
                AbilityDescriptionStar.color = GameManager.Instance.StarColors[1];
                AbilityDescription.color = GameManager.Instance.StarColors[1];
            }
            if (GameManager.Instance.AbilityCasterInstance.CurrentAbilities[GameManager.Instance.AbilityCasterInstance.HoveredAbility].Star == 3)
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
