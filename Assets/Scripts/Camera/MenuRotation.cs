using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRotation : MonoBehaviour
{
    [SerializeField] private float RotationSpeed = 10.0f;

    void Update()
    {
        transform.Rotate(new Vector3(0.0f, RotationSpeed * Time.deltaTime, 0.0f));
    }
}
