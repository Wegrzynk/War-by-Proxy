using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomEditorWindow : MonoBehaviour
{
    List<string> gameModes = new List<string>{ "FFA", "PD"};
    List<List<Map>> maps = new List<List<Map>> { maps1, maps2};
    static List<Map> maps1 = new List<Map>{ new Map("Islander", 16, 14, 2), new Map("Lukewarm", 19, 16, 2), new Map("Mesial", 23, 15, 2)};
    static List<Map> maps2 = new List<Map>{ new Map("Sudetes", 19, 19, 2), new Map("Bridgeburner", 18, 14, 2), new Map("Ringed Lake", 24, 25, 2) };
    static List<string> descriptions = new List<string>
    {
        "FFA - Standard gamemode. Player is eliminated when they lose all of their units or HQ. Players can recruit more units. Last player/team standing wins",
        "PD - Pre-Deployed. All units are already deployed for players. Map lacks any recruitment buildings. Same victory conditions from FFA apply"
    };
    private int selectedIndex = 0;
    private Map selectedMap = null;
    public GameObject GameModesList;
    public GameObject selectedModeText;
    public GameObject previousModeText;
    public GameObject previousDummyText;
    public GameObject nextModeText;
    public GameObject nextDummyText;
    public GameObject mapsListContent;
    public GameObject mapButtonPrefab;
    public GameObject mapDisplayName;
    public GameObject informationText;
    
    public void Start()
    {
        PrintModes();
        PrintMaps(selectedIndex);
        MapShow(maps[selectedIndex][0]);
        ShowDescription(selectedIndex);
    }

    public void SetIndex(int index)
    {
        selectedIndex = index;
    }

    public void Reset()
    {
        selectedIndex = 0;
        PrintModes();
        PrintMaps(selectedIndex);
        MapShow(maps[selectedIndex][0]);
        ShowDescription(selectedIndex);
    }

    public void GameModesScrollRight()
    {
        if(selectedIndex == gameModes.Count - 1)
        {
            selectedIndex = 0;
        }
        else{
            selectedIndex++;
        }
        StartCoroutine(ScrollLeft());
    }

    IEnumerator ScrollLeft()
    {
        float animLength = this.gameObject.GetComponent<Animation>()["ModeScrollLeft"].length;
        this.gameObject.GetComponent<Animation>().Play("ModeScrollLeft");
        yield return new WaitForSeconds(animLength);
        PrintModes();
        PrintMaps(selectedIndex);
        MapShow(maps[selectedIndex][0]);
        ShowDescription(selectedIndex);
    }

    public void GameModesScrollLeft()
    {
        if(selectedIndex == 0)
        {
            selectedIndex = gameModes.Count - 1;
        }
        else{
            selectedIndex--;
        }
        StartCoroutine(ScrollRight());
    }

    IEnumerator ScrollRight()
    {
        float animLength = this.gameObject.GetComponent<Animation>()["ModeScrollRight"].length;
        this.gameObject.GetComponent<Animation>().Play("ModeScrollRight");
        yield return new WaitForSeconds(animLength);
        PrintModes();
        PrintMaps(selectedIndex);
        MapShow(maps[selectedIndex][0]);
        ShowDescription(selectedIndex);
    }

    public void PrintModes()
    {
        selectedModeText.GetComponent<TextMeshProUGUI>().text = gameModes[selectedIndex];
        if(selectedIndex == 0)
        {
            previousModeText.GetComponent<TextMeshProUGUI>().text = gameModes[gameModes.Count - 1];
            previousDummyText.GetComponent<TextMeshProUGUI>().text = gameModes[gameModes.Count - 2];
        }
        else if(selectedIndex == 1)
        {
            previousModeText.GetComponent<TextMeshProUGUI>().text = gameModes[selectedIndex - 1];
            previousDummyText.GetComponent<TextMeshProUGUI>().text = gameModes[gameModes.Count - 1];
        }
        else
        {
            previousModeText.GetComponent<TextMeshProUGUI>().text = gameModes[selectedIndex - 1];
            previousDummyText.GetComponent<TextMeshProUGUI>().text = gameModes[selectedIndex - 2];
        }
        if(selectedIndex == gameModes.Count - 1)
        {
            nextModeText.GetComponent<TextMeshProUGUI>().text = gameModes[0];
            nextDummyText.GetComponent<TextMeshProUGUI>().text = gameModes[1];
        }
        else if(selectedIndex == gameModes.Count - 2)
        {
            nextModeText.GetComponent<TextMeshProUGUI>().text = gameModes[selectedIndex + 1];
            nextDummyText.GetComponent<TextMeshProUGUI>().text = gameModes[0];
        }
        else
        {
            nextModeText.GetComponent<TextMeshProUGUI>().text = gameModes[selectedIndex + 1];
            nextDummyText.GetComponent<TextMeshProUGUI>().text = gameModes[selectedIndex + 2];
        }
    }

    public void PrintMaps(int index)
    {
        if(mapsListContent.transform.childCount > 0)
        {
            foreach(Transform child in mapsListContent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        foreach(Map map in maps[index])
        {
            GameObject mapButton = Instantiate(mapButtonPrefab, mapsListContent.transform);
            mapButton.transform.Find("Text").GetComponent<Text>().text = map.GetName();
            mapButton.GetComponent<Button>().onClick.AddListener(() => MapShow(map));
        }
    }

    public void ShowDescription(int index)
    {
        informationText.GetComponent<TextMeshProUGUI>().text = descriptions[index];
    }

    public void MapShow(Map display)
    {
        mapDisplayName.GetComponent<Text>().text = display.GetName();
        selectedMap = display;
    }

    public Map GetMap()
    {
        return selectedMap;
    }

    public class Map
    {
        private string mapName;
        private int mapWidth;
        private int mapHeight;
        private int mapPlayerSpots;

        public Map(string mapname, int mapwidth, int mapheight, int mapplayerspots)
        {
            this.mapName = mapname;
            this.mapWidth = mapwidth;
            this.mapHeight = mapheight;
            this.mapPlayerSpots = mapplayerspots;
        }

        public string GetName()
        {
            return mapName;
        }

        public int GetWidth()
        {
            return mapWidth;
        }

        public int GetHeight()
        {
            return mapHeight;
        }

        public int GetSpots()
        {
            return mapPlayerSpots;
        }
    }
}
