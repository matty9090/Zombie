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

    private Resources Resources;

    void Start()
    {
        Game.BuildingUnlocked.AddListener(UnlockBuilding);
        Game.ToolUnlocked.AddListener(UnlockHarvestTool);
        Game.WeaponUnlocked.AddListener(UnlockWeapon);

        Resources = Game.Resources;
        Resources.ResourcesChangedEvent.AddListener(ResourcesChanged);

        for (int i = Game.NumBuildingsUnlocked; i < BuildingsLayout.childCount; ++i)
            BuildingsLayout.GetChild(i).gameObject.SetActive(false);

        for (int i = Game.NumWeaponsUnlocked; i < WeaponsLayout.childCount; ++i)
            WeaponsLayout.GetChild(i).gameObject.SetActive(false);
    }

    void ResourcesChanged()
    {
        ResourceTexts[(int)EResource.Wood].text = "" + Resources.Wood;
        ResourceTexts[(int)EResource.Stone].text = "" + Resources.Stone;
    }

    public void UnlockBuilding()
    {
        if (BuildingsLayout.childCount >= Game.NumBuildingsUnlocked)
        {
            var buildingLayout = BuildingsLayout.GetChild(Game.NumBuildingsUnlocked - 1);
            buildingLayout.gameObject.SetActive(true);
            buildingLayout.GetComponent<Animator>().SetTrigger("Unlocked");

            var fireworks = buildingLayout.Find("Fireworks").gameObject;
            fireworks.SetActive(true);
            Destroy(fireworks, 5.0f);

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
            var weaponLayout = WeaponsLayout.GetChild(Game.NumWeaponsUnlocked - 1);
            weaponLayout.gameObject.SetActive(true);

            var weapon = Game.AttackTools[Game.NumWeaponsUnlocked - 1].GetComponent<Weapon>();
            Popup.Name = weapon.ToolName;
            Popup.Desc = weapon.ToolDesc;
            Popup.GetComponent<Animator>().SetTrigger("Open");
        }
    }

    public void UnlockHarvestTool()
    {
        var tool = Game.HarvestTools[Game.NumToolsUnlocked - 1].GetComponent<HarvestTool>();
        Popup.Name = tool.ToolName;
        Popup.Desc = tool.ToolDesc;
        Popup.GetComponent<Animator>().SetTrigger("Open");
    }
}
