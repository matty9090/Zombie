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
        mOffset = transform.position * 0.6f + new Vector3(0.0f, 0.0f, -20.0f);
    }

    void Update()
    {
        transform.position = mCharacter.transform.position + mOffset;
    }
}
