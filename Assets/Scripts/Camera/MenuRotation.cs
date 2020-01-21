using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRotation : MonoBehaviour
{
    [SerializeField] private float RotationSpeed = 0.2f;

    void Update()
    {
        transform.Rotate(new Vector3(0.0f, RotationSpeed, 0.0f));
    }
}
