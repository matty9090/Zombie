using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IState
{
    void OnEnter();
    void OnExit();
    void Update();
}
