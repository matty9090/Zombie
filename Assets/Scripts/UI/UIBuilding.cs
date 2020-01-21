using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/* Building UI element */
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

        // Set the name of the building and shortcut number
        GetComponentInChildren<Text>().text = buildingName;
        ShortcutText.text = "" + Shortcut;

        // Set the tooltip values
        Tooltip.Wood = Wood;
        Tooltip.Stone = Stone;
        Tooltip.BuildingName = buildingName;

        // Click event listener
        mGame = GameObject.Find("Game").GetComponent<Game>();
        var btn = GetComponent<Button>();

        if (mGame != null && btn != null)
        {
            btn.onClick.AddListener(delegate { mGame.UIBuildingClicked(this); });
        }
    }

    private void Update()
    {
        // Check if shortcut is pressed
        if (Input.GetKeyDown(KeyCode.Alpha1 + Shortcut - 1))
        {
            mGame.UIBuildingClicked(this);
            GetComponent<Animator>().SetTrigger("Selected");
        }

        // Update tooltip so it knows what colour to render the resource texts
        Tooltip.WoodAvailable = mGame.Resources.Wood >= Wood;
        Tooltip.StoneAvailable = mGame.Resources.Stone >= Stone;
    }
}
