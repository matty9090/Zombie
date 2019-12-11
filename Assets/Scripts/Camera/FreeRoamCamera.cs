using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeRoamCamera : MonoBehaviour, ICamera
{
    [SerializeField] private float Speed = 50.0f;
    [SerializeField] private Vector3 Offset = new Vector3(0.0f, 160.0f, -180.0f);

    public void SetCharacter(Character c)
    {

    }

    public void SetEnabled(bool e)
    {
        enabled = e;

        if (enabled)
        {
            Game g = GameObject.Find("Game").GetComponent<Game>();
            transform.position = g.CharacterInst.transform.position + Offset;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W)) transform.position += new Vector3(0.0f, 0.0f, Speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S)) transform.position -= new Vector3(0.0f, 0.0f, Speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) transform.position -= new Vector3(Speed * Time.deltaTime, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.D)) transform.position += new Vector3(Speed * Time.deltaTime, 0.0f, 0.0f);
    }
}
