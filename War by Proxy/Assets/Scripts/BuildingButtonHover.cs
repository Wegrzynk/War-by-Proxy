using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class BuildingButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject canvas;
    public City cityupgrade;
    private Dictionary<TilemapObject.TilemapSprite, string> descriptions = new Dictionary<TilemapObject.TilemapSprite, string>
    {
        { TilemapObject.TilemapSprite.Radio, "Provides additional 2 vision for any unit stationed on this building. Useless without fog of war." },
        { TilemapObject.TilemapSprite.Lab, "Allows any unit ending its turn on this building and selecting proper action to be upgraded once per turn, up to 3 upgrades. Each upgrade provides addition +10% attack power" },
        { TilemapObject.TilemapSprite.Outpost, "Has terrain defense of 4, which is an additional defense point from regular city's 3 defense points" }
    };

    public void OnPointerEnter(PointerEventData eventData)
    {
        canvas.transform.Find("BuildingUpgradeMenu").Find("NoBuildingInfo").gameObject.SetActive(false);
        Texture2D unitthumbnail = RuntimePreviewGenerator.GenerateModelPreview(cityupgrade.GetTileVisual().transform);
        Transform buildinginfo = canvas.transform.Find("BuildingUpgradeMenu").Find("BuildingInfo");
        buildinginfo.Find("BI_Title").Find("Text").GetComponent<TextMeshProUGUI>().text = "Building: " + cityupgrade.ToString();
        buildinginfo.Find("BI_Icon").Find("Image").GetComponent<Image>().sprite = Sprite.Create(unitthumbnail, new Rect(0.0f, 0.0f, unitthumbnail.width, unitthumbnail.height), new Vector2(0.5f, 0.5f), 100f);
        buildinginfo.Find("BI_Description").Find("Text").GetComponent<TextMeshProUGUI>().text = descriptions[cityupgrade.GetTilemapSprite()];
        buildinginfo.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        canvas.transform.Find("BuildingUpgradeMenu").Find("BuildingInfo").gameObject.SetActive(false);
        canvas.transform.Find("BuildingUpgradeMenu").Find("NoBuildingInfo").gameObject.SetActive(true);
    }
}