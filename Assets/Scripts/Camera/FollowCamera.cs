using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Character Character = null;
    private Vector3 mOffset;

    private void Start()
    {
        mOffset = transform.position;
    }

    void Update()
    {
        transform.position = Character.transform.position + mOffset;
    }
}
