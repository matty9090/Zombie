﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocker : MonoBehaviour, IDestroyable
{
    public void DestroyObject()
    {
        if (gameObject)
        {
            transform.parent.GetComponent<EnvironmentTile>().IsAccessible = true;
            Destroy(gameObject);
        }
    }
}
