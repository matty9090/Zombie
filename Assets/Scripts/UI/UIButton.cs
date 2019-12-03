using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public void OnPointerEnter(PointerEventData ped)
    {
        GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("ButtonHover");
    }

    public void OnPointerDown(PointerEventData ped)
    {
        GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("ButtonClick");
    }
}