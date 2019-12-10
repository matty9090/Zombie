using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour, ICamera
{
    private Character mCharacter = null;
    public Vector3 Offset = new Vector3(0.0f, 80.0f, -170.0f);

    public void SetCharacter(Character c)
    {
        mCharacter = c;
    }

    public void SetEnabled(bool e)
    {
        enabled = e;
    }

    void Update()
    {
        if (mCharacter)
        {
            transform.position = mCharacter.transform.position + Offset;
        }
    }
}
