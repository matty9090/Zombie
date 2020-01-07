using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocker : MonoBehaviour, IDestroyable
{
    [SerializeField] private int DamageAmount = 20;

    public bool DamageObject()
    {
        var building = GetComponent<Building>();

        if (building != null)
        {
            building.Health -= DamageAmount;

            if (building.Health <= 0)
            {
                transform.parent.GetComponent<EnvironmentTile>().IsAccessible = true;
                return true;
            }
        }

        return false;
    }
}
