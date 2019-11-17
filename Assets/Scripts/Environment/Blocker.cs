using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocker : MonoBehaviour, IDestroyable
{
    public void DestroyObject()
    {
        if (gameObject)
        {
            Destroy(gameObject);
        }
    }
}
