using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayersEditorWindow : MonoBehaviour
{
    public List<Color> playerColors = new List<Color>{ Color.red, Color.blue, Color.green, Color.yellow };
    public List<string> teams = new List<string>{"None", "A", "B", "C", "D", "E", "F", "G", "H"};
    public MenuManager.Room room;
    public int[] playerIndexes;
    public int previousChosenSpot;
    public int chosenSpot;
    public GameObject playersList;
    public GameObject playerSettingsPrefab;
    public GameObject[] playerSettingsList;
    public GameObject createRoomButton;

    public void Init()
    {
        previousChosenSpot = -1;
        chosenSpot = -1;
        playerIndexes = new int[room.playerSpots];
        playerSettingsList = new GameObject[room.playerSpots];
        for(int i = 0; i < room.playerSpots; i++)
        {
            int copy = i;
            playerSettingsList[i] = Instantiate(playerSettingsPrefab, playersList.transform);
            playerSettingsList[i].transform.Find("PlayerColor").GetComponent<Image>().color = playerColors[i];
            playerSettingsList[i].transform.Find("TeamSelector").Find("SelectorLeft").GetComponent<Button>().onClick.AddListener(() => MoveLeft(copy));
            playerSettingsList[i].transform.Find("TeamSelector").Find("SelectorRight").GetComponent<Button>().onClick.AddListener(() => MoveRight(copy));
            playerSettingsList[i].transform.Find("EnterSpotButton").GetComponent<Button>().onClick.AddListener(() => EnterSpot(copy));
        }
        InteractableCheck();
    }

    public void Reset()
    {
        foreach (Transform child in playersList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        createRoomButton.GetComponent<Button>().interactable = false;
    }

    public void DeactivateSettings()
    {
        for(int i = 0; i < room.playerSpots; i++)
        {
            playerSettingsList[i].transform.Find("TeamSelector").Find("SelectorLeft").GetComponent<Button>().interactable = false;
            playerSettingsList[i].transform.Find("TeamSelector").Find("SelectorRight").GetComponent<Button>().interactable = false;
            playerSettingsList[i].transform.Find("EnterSpotButton").GetComponent<Button>().interactable = false;
        }
    }

    public void ReactivateSettings()
    {
        for(int i = 0; i < room.playerSpots; i++)
        {
            playerSettingsList[i].transform.Find("TeamSelector").Find("SelectorLeft").GetComponent<Button>().interactable = true;
            playerSettingsList[i].transform.Find("TeamSelector").Find("SelectorRight").GetComponent<Button>().interactable = true;
            playerSettingsList[i].transform.Find("EnterSpotButton").GetComponent<Button>().interactable = true;
        }
        this.transform.Find("returnButton").GetComponent<Button>().interactable = true;
        createRoomButton.GetComponent<Button>().interactable = true;
    }

    public void SetRoom(MenuManager.Room newRoom)
    {
        room = newRoom;
    }

    public void PrintSettings(int index)
    {
        playerSettingsList[index].transform.Find("TeamSelector").Find("SelectorText").GetComponent<TextMeshProUGUI>().text = "Team: " + teams[playerIndexes[index]];
    }

    public void MoveLeft(int index)
    {
        if(playerIndexes[index] == 0)
        {
            playerIndexes[index] = 8;
        }
        else
        {
            playerIndexes[index]--;
        }
        InteractableCheck();
        PrintSettings(index);
    }

    public void MoveRight(int index)
    {
        if(playerIndexes[index] == 8)
        {
            playerIndexes[index] = 0;
        }
        else
        {
            playerIndexes[index]++;
        }
        InteractableCheck();
        PrintSettings(index);
    }

    public void EnterSpot(int index)
    {
        if(chosenSpot != -1)
        {
            playerSettingsList[chosenSpot].transform.Find("HostName").gameObject.SetActive(false);
            playerSettingsList[chosenSpot].transform.Find("EnterSpotButton").gameObject.SetActive(true);
        }
        playerSettingsList[index].transform.Find("HostName").gameObject.SetActive(true);
        playerSettingsList[index].transform.Find("HostName").gameObject.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.LocalPlayer.NickName;
        playerSettingsList[index].transform.Find("EnterSpotButton").gameObject.SetActive(false);
        chosenSpot = index;
        InteractableCheck();
    }

    public void InteractableCheck()
    {
        if(chosenSpot != -1)
        {
            int checker = -1;
            for(int i = 0; i < playerIndexes.Length; i++)
            {
                if(checker == -1) checker = playerIndexes[i];
                if(playerIndexes[i] == 0 || playerIndexes[i] != checker) 
                {
                    createRoomButton.GetComponent<Button>().interactable = true;
                    return;
                }
            }
        }
        createRoomButton.GetComponent<Button>().interactable = false;
    }

    public int[] GetArray()
    {
        return playerIndexes;
    }

    public int GetHostSpot()
    {
        return chosenSpot;
    }
}
