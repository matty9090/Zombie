using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIBuilding : MonoBehaviour
{
    public string Name;
    public int Wood = 0;
    public int Stone = 0;
    public int Shortcut = 1;

    public Text ShortcutText = null;
    public GameObject Object = null;

    private Game mGame;

    private void Start()
    {
        GetComponentInChildren<Text>().text = Name;
        ShortcutText.text = "" + Shortcut;

        mGame = GameObject.Find("Game").GetComponent<Game>();
        var btn = GetComponent<Button>();

        if (mGame != null && btn != null)
        {
            btn.onClick.AddListener(delegate { mGame.UIBuildingClicked(this); });
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1 + Shortcut - 1))
        {
            mGame.UIBuildingClicked(this);
            GetComponent<Animator>().SetTrigger("Selected");
        }
    }
}
