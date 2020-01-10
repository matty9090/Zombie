using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIBuilding : MonoBehaviour
{
    [SerializeField] private UIBuildingTooltip Tooltip = null;

    public int Wood = 0;
    public int Stone = 0;
    public int Shortcut = 1;

    public Text ShortcutText = null;
    public GameObject Object = null;

    private Game mGame;

    private void Start()
    {
        var buildingName = Object.GetComponent<Building>().BuildingName;

        GetComponentInChildren<Text>().text = buildingName;
        ShortcutText.text = "" + Shortcut;

        mGame = GameObject.Find("Game").GetComponent<Game>();
        var btn = GetComponent<Button>();

        if (mGame != null && btn != null)
        {
            btn.onClick.AddListener(delegate { mGame.UIBuildingClicked(this); });
        }

        Tooltip.Wood = Wood;
        Tooltip.Stone = Stone;
        Tooltip.BuildingName = buildingName;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1 + Shortcut - 1))
        {
            mGame.UIBuildingClicked(this);
            GetComponent<Animator>().SetTrigger("Selected");
        }

        Tooltip.WoodAvailable = mGame.Resources.Wood >= Wood;
        Tooltip.StoneAvailable = mGame.Resources.Stone >= Stone;
    }
}
