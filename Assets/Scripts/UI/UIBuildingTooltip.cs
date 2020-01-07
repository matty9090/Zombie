using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingTooltip : MonoBehaviour
{
    [SerializeField] private Text WoodTxt = null;
    [SerializeField] private Text StoneTxt = null;

    [SerializeField] private Color Available = Color.green;
    [SerializeField] private Color Unavailable = Color.red;

    public int Wood {
        set { WoodTxt.text = "" + value; }
    }

    public int Stone {
        set { StoneTxt.text = "" + value; }
    }

    public bool WoodAvailable {
        set { WoodTxt.color = value ? Available : Unavailable; }
    }

    public bool StoneAvailable {
        set { StoneTxt.color = value ? Available : Unavailable; }
    }
}
