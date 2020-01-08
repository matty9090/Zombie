using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private List<Text> ResourceTexts = null;
    [SerializeField] private Game Game = null;
    [SerializeField] private Transform BuildingsLayout = null;

    private Resources Resources;

    void Start()
    {
        Resources = Game.Resources;
        Resources.ResourcesChangedEvent.AddListener(ResourcesChanged);

        for (int i = Game.NumBuildingsUnlocked; i < BuildingsLayout.childCount; ++i)
        {
            BuildingsLayout.GetChild(i).gameObject.SetActive(false);
        }
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
            var building = BuildingsLayout.GetChild(Game.NumBuildingsUnlocked - 1);
            building.gameObject.SetActive(true);
            building.GetComponent<Animator>().SetTrigger("Unlocked");
            building.Find("Fireworks").gameObject.SetActive(true);
        }
    }
}
