using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvestable : MonoBehaviour, IDestroyable
{
    public int Amount;
    public EResource Type;
    public string GatherSound = "GatherStone";

    public void DestroyObject()
    {
        var env = GameObject.Find("Environment").GetComponent<Environment>();
        env.Harvest(this);
    }
}
