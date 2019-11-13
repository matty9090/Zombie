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
    public GameObject Object = null;

    private void Start()
    {
        GetComponent<Text>().text = Name;

        var game = GameObject.Find("Game").GetComponent<Game>();
        var btn = GetComponent<Button>();

        if (game != null && btn != null)
        {
            btn.onClick.AddListener(delegate { game.UIBuildingClicked(this); });
        }
    }
}
