using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvestable : MonoBehaviour, IDestroyable
{
    [SerializeField] private int Health = 100;
    [SerializeField] private int DamageAmount = 20;

    public int Amount;
    public EResource Type;
    public string GatherSound = "GatherStone";

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
