using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private CanvasGroup Tooltip = null;

    public void OnPointerEnter(PointerEventData ped)
    {
        GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("ButtonHover");

        if (Tooltip != null)
            Tooltip.alpha = 1;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Tooltip != null)
            Tooltip.alpha = 0;
    }

    public void OnPointerDown(PointerEventData ped)
    {
        GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("ButtonClick");
    }
}