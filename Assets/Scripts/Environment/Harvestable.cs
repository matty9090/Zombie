using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Something which can be harvested */
public class Harvestable : MonoBehaviour, IDestroyable
{
    [SerializeField] private int Health = 100;
    [SerializeField] private int DamageAmount = 20;

    public int Amount;
    public EResource Type;
    public string GatherSound = "GatherStone";

    // IDestroyable
    public bool DamageObject()
    {
        Health -= DamageAmount;

        if (Health <= 0)
        {
            Health = 0;

            var env = GameObject.Find("Environment").GetComponent<Environment>();
            env.Harvest(this);

            return true;
        }

        return false;
    }
}
