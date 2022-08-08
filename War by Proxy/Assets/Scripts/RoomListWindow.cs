using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomListWindow : MonoBehaviourPunCallbacks
{
    private List<MenuManager.Room> onlineRooms = new List<MenuManager.Room>();
    private List<Player> players = new List<Player>();
    List<Color> playerColors = new List<Color>{ Color.red, Color.blue, Color.green, Color.yellow };
    private int counter = 1;

    public GameObject roomPrefab;
    public GameObject playerPrefab;
    public GameObject roomsListContent;
    public GameObject waitingRoomWindow;
    public GameObject menuManager;

    IEnumerator Hold1()
    {
        this.gameObject.GetComponent<Animation>().Play("SwipeAway");
        yield return new WaitForSeconds(this.gameObject.GetComponent<Animation>()["SwipeAway"].length);
        waitingRoomWindow.SetActive(true);
        waitingRoomWindow.GetComponent<WaitingRoomWindow>().Init();
        waitingRoomWindow.transform.Find("confirmButton").gameObject.SetActive(false);
        waitingRoomWindow.GetComponent<Animation>().Play("SwipeIn");
        this.gameObject.SetActive(false);
    }

    IEnumerator Hold2()
    {
        yield return new WaitForSeconds(waitingRoomWindow.GetComponent<Animation>()["SwipeAway2"].length);
    }

    public void ClearList()
    {
        foreach(Transform child in roomsListContent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void InitRoomsList()
    {
        foreach(RoomInfo room in menuManager.GetComponent<MenuManager>().listings)
        {
            GameObject roomInstance = Instantiate(roomPrefab, roomsListContent.transform);
            roomInstance.name = room.Name;
            PlayersDisplay(roomInstance, room);
            roomInstance.transform.Find("RoomName").Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = "Room #" + counter + ": " + room.CustomProperties["MapName"].ToString();
            roomInstance.transform.Find("mapDisplay").Find("mapDisplayName").GetComponent<Text>().text = room.CustomProperties["MapName"].ToString();
            counter++;
        }
    }

    public void Reset()
    {
        foreach(Transform child in roomsListContent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        onlineRooms.Clear();
        counter = 1;
    }

    public void PlayersDisplay(GameObject roomInstance, RoomInfo room)
    {
        int[] letters = (int[])room.CustomProperties["Alliances"];
        string[] playerNames = (string[])room.CustomProperties["PlayerNames"];
        
        for(int i = 0; i < room.MaxPlayers; i++)
        {
            GameObject playerPanel = Instantiate(playerPrefab, roomInstance.transform.Find("playersList").Find("Viewport").Find("Content"));
            if(System.Convert.ToChar(64 + letters[i]) != '@')
            {
                playerPanel.transform.Find("Team").gameObject.SetActive(true);
                playerPanel.transform.Find("Team").Find("TeamLetter").GetComponent<TextMeshProUGUI>().text = System.Convert.ToChar(64 + letters[i]).ToString();
            }
            playerPanel.transform.Find("Color").GetComponent<Image>().color = playerColors[i];
            if(playerNames[i] == "")
            {
                int copy = i;
                playerPanel.transform.Find("SlotName").GetComponent<TextMeshProUGUI>().text = "Empty Slot";
                playerPanel.transform.Find("SlotName").gameObject.SetActive(false);
                playerPanel.transform.Find("SlotJoinButton").gameObject.SetActive(true);
                playerPanel.transform.Find("SlotJoinButton").GetComponent<Button>().onClick.AddListener(() => EnterRoom(copy, room));
            }
            else
            {
                playerPanel.transform.Find("SlotName").GetComponent<TextMeshProUGUI>().text = playerNames[i];
            }
        }
    }

    public void EnterRoom(int index, RoomInfo room)
    {
        ExitGames.Client.Photon.Hashtable myCustomProperties = new ExitGames.Client.Photon.Hashtable();
        myCustomProperties["Index"] = index;
        myCustomProperties["ShowGrid"] = menuManager.GetComponent<MenuManager>().showgrid;
        PhotonNetwork.SetPlayerCustomProperties(myCustomProperties);
        PhotonNetwork.JoinRoom(room.Name);
    }

    public override void OnJoinedRoom()
    {
        menuManager.GetComponent<MenuManager>().listings.Clear();
        StartCoroutine(Hold1());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Joining room failed. " + message);
    }
}
