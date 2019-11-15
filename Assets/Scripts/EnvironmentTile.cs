using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentTile : MonoBehaviour
{
    public List<EnvironmentTile> Connections { get; set; }
    public EnvironmentTile Parent { get; set; }
    public Vector3 Position { get; set; }
    public float Global { get; set; }
    public float Local { get; set; }
    public bool Visited { get; set; }

    private bool mIsAccessible;

    public bool IsAccessible
    { 
        get {
            if (mIsAccessible)
            {
                int mask = 1 << LayerMask.NameToLayer("Character");
                bool isChar = Physics.Raycast(Position + Vector3.up * 10.0f, Vector3.down, 10.0f, mask);
                
                if (isChar)
                    Debug.Log("Character in the way!");

                return !isChar;
            }

            //Debug.DrawRay(Position + Vector3.up * 800.0f, Vector3.down, Color.red, 2.0f);
            //Debug.Log(Position + Vector3.up * 80.0f);

            return mIsAccessible;
        }
        set {
            mIsAccessible = value;
        }
    }
}
