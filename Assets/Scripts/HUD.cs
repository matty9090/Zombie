using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private List<Text> ResourceTexts = null;
    [SerializeField] private Game Game = null;

    private Resources Resources;

    void Start()
    {
        Resources = Game.Resources;
        Resources.ResourcesChangedEvent.AddListener(ResourcesChanged);
    }

    void ResourcesChanged()
    {
        ResourceTexts[(int)EResource.Wood].text = "" + Resources.Wood;
        ResourceTexts[(int)EResource.Stone].text = "" + Resources.Stone;
    }
}
