using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
public class RecruitButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject canvas;
    public Unit unit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        canvas.transform.Find("RecruitMenu").Find("NoUnitInfo").gameObject.SetActive(false);
        Texture2D unitthumbnail = RuntimePreviewGenerator.GenerateModelPreview(unit.GetUnitInstance().transform);
        Transform unitinfo = canvas.transform.Find("RecruitMenu").Find("UnitInfo");
        unitinfo.Find("UI_Title").Find("Text").GetComponent<TextMeshProUGUI>().text = "Unit: " + unit.ToString();
        unitinfo.Find("UI_Icon").Find("Image").GetComponent<Image>().sprite = Sprite.Create(unitthumbnail, new Rect(0.0f, 0.0f, unitthumbnail.width, unitthumbnail.height), new Vector2(0.5f, 0.5f), 100f);
        unitinfo.Find("UI_Health").Find("Text").GetComponent<TextMeshProUGUI>().text = "Health: 100";
        unitinfo.Find("UI_Movement").Find("Text").GetComponent<TextMeshProUGUI>().text = "Movement: " + unit.GetStringFromMovementType() + " " + unit.GetMovementDistance();
        unitinfo.Find("UI_Vision").Find("Text").GetComponent<TextMeshProUGUI>().text = "Vision: " + unit.GetVision();
        unitinfo.Find("UI_Fuel").Find("Text").GetComponent<TextMeshProUGUI>().text = "Fuel/Rations: " + unit.GetCurrentFuel() + "/" + unit.GetFuel();
        unitinfo.Find("UI_Ammo").Find("Text").GetComponent<TextMeshProUGUI>().text = "Ammunition: " + unit.GetCurrentAmmo() + "/" + unit.GetAmmo();
        unitinfo.Find("UI_AttackRange").Find("Text").GetComponent<TextMeshProUGUI>().text = "Attack Range: " + unit.GetMinRange() + "-" + unit.GetMaxRange();
        unitinfo.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        canvas.transform.Find("RecruitMenu").Find("UnitInfo").gameObject.SetActive(false);
        canvas.transform.Find("RecruitMenu").Find("NoUnitInfo").gameObject.SetActive(true);
    }
}