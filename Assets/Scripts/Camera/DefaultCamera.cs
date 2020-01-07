using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultCamera : MonoBehaviour, ICamera
{
    public void SetEnabled(bool e)
    {
        enabled = e;
    }

	public void SetCharacter(Character c)
    {

    }

    public bool IsEnabled()
    {
        return enabled;
    }
}
