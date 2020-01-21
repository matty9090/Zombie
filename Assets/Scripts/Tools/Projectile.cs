using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int Damage;
    public int Lifetime = 10;

    private void Start()
    {
        Destroy(gameObject, Lifetime);
    }

    /* Damage an enemy if it collided with one */
    private void OnCollisionEnter(Collision collision)
    {
        var zombie = collision.gameObject.GetComponent<Zombie>();

        if (zombie != null)
        {
            zombie.Damage(Damage);
            Destroy(gameObject);
        }
    }
}
