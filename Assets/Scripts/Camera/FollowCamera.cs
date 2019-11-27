using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour, ICamera
{
    private Character mCharacter = null;
    private Vector3 mOffset;

    public void SetCharacter(Character c)
    {
        mCharacter = c;
    }

    public void SetEnabled(bool e)
    {
        enabled = e;
    }

    private void Start()
    {
        mOffset = transform.position;
    }

    void Update()
    {
        transform.position = mCharacter.transform.position + mOffset;
    }
}
