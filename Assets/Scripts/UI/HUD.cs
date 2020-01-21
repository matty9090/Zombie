using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private List<Text> ResourceTexts = null;
    [SerializeField] private Game Game = null;
    [SerializeField] private UnlockPopup Popup = null;
    [SerializeField] private Transform BuildingsLayout = null;
    [SerializeField] private Transform WeaponsLayout = null;
    
    public GameObject BuildUI = null;
    public GameObject WaveUI = null;

    private Resources mResources;

    void Start()
    {
        Game.BuildingUnlocked.AddListener(UnlockBuilding);
        Game.ToolUnlocked.AddListener(UnlockHarvestTool);
        Game.WeaponUnlocked.AddListener(UnlockWeapon);

        mResources = Game.Resources;
        mResources.ResourcesChangedEvent.AddListener(ResourcesChanged);

        // Disable building and weapons that haven't been unlocked yet

        for (int i = Game.NumBuildingsUnlocked; i < BuildingsLayout.childCount; ++i)
            BuildingsLayout.GetChild(i).gameObject.SetActive(false);

        for (int i = Game.NumWeaponsUnlocked; i < WeaponsLayout.childCount; ++i)
            WeaponsLayout.GetChild(i).gameObject.SetActive(false);
    }

    /* Update UI resource values */
    void ResourcesChanged()
    {
        ResourceTexts[(int)EResource.Wood].text = "" + mResources.Wood;
        ResourceTexts[(int)EResource.Stone].text = "" + mResources.Stone;
    }

    public void UnlockBuilding()
    {
        if (BuildingsLayout.childCount >= Game.NumBuildingsUnlocked)
        {
            // Reveal the building UI
            var buildingLayout = BuildingsLayout.GetChild(Game.NumBuildingsUnlocked - 1);
            buildingLayout.gameObject.SetActive(true);
            buildingLayout.GetComponent<Animator>().SetTrigger("Unlocked");

            // Show fireworks effect behind the UI element
            var fireworks = buildingLayout.Find("Fireworks").gameObject;
            fireworks.SetActive(true);
            Destroy(fireworks, 5.0f);

            // Show unlock popup
            var building = Game.Buildings[Game.NumBuildingsUnlocked - 1].GetComponent<Building>();
            Popup.Name = building.BuildingName;
            Popup.Desc = building.BuildingDesc;
            Popup.GetComponent<Animator>().SetTrigger("Open");
        }
    }

    public void UnlockWeapon()
    {
        if (WeaponsLayout.childCount >= Game.NumWeaponsUnlocked)
        {
            // Reveal the new weapon UI
            var weaponLayout = WeaponsLayout.GetChild(Game.NumWeaponsUnlocked - 1);
            weaponLayout.gameObject.SetActive(true);

            // Show unlock popup
            var weapon = Game.AttackTools[Game.NumWeaponsUnlocked - 1].GetComponent<Weapon>();
            Popup.Name = weapon.ToolName;
            Popup.Desc = weapon.ToolDesc;
            Popup.GetComponent<Animator>().SetTrigger("Open");
        }
    }

    public void UnlockHarvestTool()
    {
        // Show unlock popup
        var tool = Game.HarvestTools[Game.NumToolsUnlocked - 1].GetComponent<HarvestTool>();
        Popup.Name = tool.ToolName;
        Popup.Desc = tool.ToolDesc;
        Popup.GetComponent<Animator>().SetTrigger("Open");
    }
}
