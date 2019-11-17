using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    [SerializeField]
    public float DamageTime = 0.6f;

    [SerializeField]
    public float DamageTimer = 0.0f;

    [SerializeField]
    public int DamageAmount = 10;
}
