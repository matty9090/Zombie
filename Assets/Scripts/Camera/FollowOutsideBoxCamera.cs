using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Camera to follow the player only if the player nearly goes off the screen */
public class FollowOutsideBoxCamera : MonoBehaviour, ICamera
{
    [SerializeField] private float EdgeTop = 0.2f;
    [SerializeField] private float EdgeRight = 0.2f;
    [SerializeField] private float EdgeBottom = 0.3f;
    [SerializeField] private float EdgeLeft = 0.2f;

    private Character mCharacter = null;

    public bool IsEnabled()
    {
        return enabled;
    }

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
        Vector3 screen = GetComponent<Camera>().WorldToViewportPoint(mCharacter.transform.position);
        
        if (screen.x < EdgeLeft)
        {
            Vector3 point = GetComponent<Camera>().ViewportToWorldPoint(new Vector3(EdgeLeft, screen.y, screen.z));
            transform.position += new Vector3(mCharacter.transform.position.x - point.x, 0.0f, 0.0f);
        }

        if (screen.x > 1.0f - EdgeRight)
        {
            Vector3 point = GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1.0f - EdgeRight, screen.y, screen.z));
            transform.position += new Vector3(mCharacter.transform.position.x - point.x, 0.0f, 0.0f);
        }

        if (screen.y < EdgeBottom)
        {
            Vector3 point = GetComponent<Camera>().ViewportToWorldPoint(new Vector3(screen.x, EdgeBottom, screen.z));
            transform.position += new Vector3(0.0f, 0.0f, mCharacter.transform.position.y - point.y);
        }

        if (screen.y > 1.0f - EdgeTop)
        {
            Vector3 point = GetComponent<Camera>().ViewportToWorldPoint(new Vector3(screen.x, 1.0f - EdgeTop, screen.z));
            transform.position += new Vector3(0.0f, 0.0f, mCharacter.transform.position.y - point.y);
        }
    }
}
