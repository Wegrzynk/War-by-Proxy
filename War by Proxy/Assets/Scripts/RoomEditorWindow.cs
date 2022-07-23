using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomEditorWindow : MonoBehaviour
{
    List<string> gameModes = new List<string>{ "FFA", "KOTH", "Control"};
    List<List<string>> maps = new List<List<string>> { maps1, maps2, maps3};
    static List<string> maps1 = new List<string>{ "Map 1_1","Map 1_2","Map 1_3","Map 1_4" };
    static List<string> maps2 = new List<string>{ "Map 2_1","Map 2_2","Map 2_3","Map 2_4","Map 2_5","Map 2_6","Map 2_7","Map 2_8","Map 2_9","Map 2_10","Map 2_11","Map 2_12", };
    static List<string> maps3 = new List<string>{ "Map 3_1","Map 3_2","Map 3_3" };
    static List<string> descriptions = new List<string>
    {
        "FFA - Standard gamemode. Player is eliminated when they lose all of their units or HQ. Last player/team standing wins",
        "KOTH - Player occupying a special building (hill) in the middle of the map earns points. If it is held for 5 turns, the player occupying the hill wins",
        "Control - Multiple special buildings are set around the map. Controlling any number of them earns points. Once a treshold is reached by a player, that player wins"
    };
    private int selectedIndex = 0;
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
        /*PrintModes();
        PrintMaps(selectedIndex);
        MapShow(maps[selectedIndex][0]);
        ShowDescription(selectedIndex);*/
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
        /*Debug.Log("This text should not be showing until after animation");
        PrintModes();
        PrintMaps(selectedIndex);
        MapShow(maps[selectedIndex][0]);
        ShowDescription(selectedIndex);*/
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

        foreach(string mapName in maps[index])
        {
            GameObject mapButton = Instantiate(mapButtonPrefab, mapsListContent.transform);
            mapButton.transform.Find("Text").GetComponent<Text>().text = mapName;
            mapButton.GetComponent<Button>().onClick.AddListener(() => MapShow(mapName));
        }
    }

    public void ShowDescription(int index)
    {
        informationText.GetComponent<TextMeshProUGUI>().text = descriptions[index];
    }

    public void MapShow(string display)
    {
        mapDisplayName.GetComponent<Text>().text = display;
    }

    public string GetMap()
    {
        return mapDisplayName.GetComponent<Text>().text;
    }
}
