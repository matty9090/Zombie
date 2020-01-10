using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWeapon : MonoBehaviour
{
    public int Shortcut = 1;
    public Text ShortcutText = null;
    public GameObject WeaponObject = null;
    
    private Game mGame;

    void Start()
    {
        mGame = GameObject.Find("Game").GetComponent<Game>();
        var btn = GetComponent<Button>();

        if (mGame != null && btn != null)
        {
            btn.onClick.AddListener(delegate { mGame.UIWeaponClicked(this); });
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1 + Shortcut - 1))
        {
            mGame.UIWeaponClicked(this);
            GetComponent<Animator>().SetTrigger("Selected");
        }
    }
}
