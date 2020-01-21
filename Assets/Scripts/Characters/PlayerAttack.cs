using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Helper class to track which enemy colliders are in range of the player */
public class PlayerAttack : MonoBehaviour
{
    public List<Collider> Colliders { get; private set; }

    private void Start()
    {
        Colliders = new List<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Colliders.Contains(other))
        {
            Colliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Colliders.Remove(other);
    }
}
