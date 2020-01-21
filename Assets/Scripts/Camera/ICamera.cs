using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Camera interface */
interface ICamera
{
    void SetCharacter(Character c);
    void SetEnabled(bool enabled);
    bool IsEnabled();
}
