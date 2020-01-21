using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField] private float DamageTime = 0.4f; // How often to damage a zombie
    [SerializeField] private int DamageAmount = 6;
    private float mDamageTimer = 0.0f;

    private void Start()
    {
        mDamageTimer = DamageTime;
    }

    private void OnTriggerStay(Collider other)
    {
        // Damage zombie every x seconds
        if (other.GetComponent<Zombie>() != null)
        {
            mDamageTimer -= Time.deltaTime;
            
            if (mDamageTimer <= 0)
            {
                GetComponent<Building>().Health -= DamageAmount;
                mDamageTimer = DamageTime;
            }
        }
    }
}
