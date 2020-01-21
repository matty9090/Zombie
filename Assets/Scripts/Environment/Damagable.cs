using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This component is used as a trigger to damage zombies  */
public class Damagable : MonoBehaviour
{
    [SerializeField]
    public float DamageTime = 0.6f;

    [SerializeField]
    public float DamageTimer = 0.0f;

    [SerializeField]
    public int DamageAmount = 10;
}
