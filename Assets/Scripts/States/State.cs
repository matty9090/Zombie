using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* State interface */
interface IState
{
    void OnEnter();
    void OnExit();
    void Update();
}
