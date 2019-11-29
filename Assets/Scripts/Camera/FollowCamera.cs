using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour, ICamera
{
    private Character mCharacter = null;
    public Vector3 Offset = new Vector3(0.0f, 160.0f, -240.0f);

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
        transform.position = mCharacter.transform.position + Offset;
    }
}
