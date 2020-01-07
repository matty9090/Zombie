using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private GameObject Tooltip = null;

    public void OnPointerEnter(PointerEventData ped)
    {
        GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("ButtonHover");
        
        if (Tooltip != null)
            Tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Tooltip != null)
            Tooltip.SetActive(false);
    }

    public void OnPointerDown(PointerEventData ped)
    {
        GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("ButtonClick");
    }
}