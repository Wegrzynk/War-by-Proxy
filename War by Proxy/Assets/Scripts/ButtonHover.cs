using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string info;
    string defaultText = "Hover over a label for description";
    public GameObject matchEditorWindow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        matchEditorWindow.GetComponent<MatchEditorWindow>().Description(info);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        matchEditorWindow.GetComponent<MatchEditorWindow>().Description(defaultText);
    }
}