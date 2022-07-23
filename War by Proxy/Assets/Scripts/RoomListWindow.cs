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
    //private List<RoomInfo> listings = new List<RoomInfo>();
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
        //this.gameObject.SetActive(false);
        waitingRoomWindow.SetActive(true);
        waitingRoomWindow.GetComponent<WaitingRoomWindow>().Init();
        waitingRoomWindow.transform.Find("confirmButton").gameObject.SetActive(false);
        //waitingRoomWindow.transform.Find("returnButton").GetComponent<Button>().onClick.AddListener(() => waitingRoomWindow.GetComponent<WaitingRoomWindow>().QuitRoom());
        waitingRoomWindow.GetComponent<Animation>().Play("SwipeIn");
        this.gameObject.SetActive(false);
    }

    IEnumerator Hold2()
    {
        yield return new WaitForSeconds(waitingRoomWindow.GetComponent<Animation>()["SwipeAway2"].length);
    }


    /*public void InitRoomsList()
    {
        foreach(MenuManager.Room room in onlineRooms)
        {
            GameObject roomInstance = Instantiate(roomPrefab, roomsListContent.transform);
            PlayersDisplay(roomInstance, room);
            InformationDisplay(roomInstance, room);
            roomInstance.transform.Find("RoomName").Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = "Room #" + counter + ": Standard on " + room.roomMap;
            counter++;
        }
    }*/

    /*public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("There was an update on rooms list");
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                int index = listings.FindIndex( x => x.Name == info.Name);
                if (index != -1)
                {
                    Destroy(GameObject.Find(info.Name));
                    listings.RemoveAt(index);
                }
            }
            else
            {
                int index = listings.FindIndex(x => x.Name == info.Name);
                if(index == -1)
                {
                    GameObject listing = Instantiate(roomPrefab, roomsListContent.transform);
                    if (listing != null)
                    {
                        //listing.SetRoomInfo(info);
                        listings.Add(info);
                    }
                }
            }
        }
    }*/

    public void ClearList()
    {
        foreach(Transform child in roomsListContent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void InitRoomsList()
    {
        Debug.Log("Starting init. " + menuManager.GetComponent<MenuManager>().listings.Count);
        foreach(RoomInfo room in menuManager.GetComponent<MenuManager>().listings)
        {
            Debug.Log("Testing if room exists");
            GameObject roomInstance = Instantiate(roomPrefab, roomsListContent.transform);
            roomInstance.name = room.Name;
            InformationDisplay(roomInstance, room);
            PlayersDisplay(roomInstance, room);
            roomInstance.transform.Find("RoomName").Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = "Room #" + counter + ": Standard on " + room.CustomProperties["Map"].ToString();
            counter++;
        }
    }

    public void ProvideSampleRooms(List<MenuManager.Room> sampleList)
    {
        onlineRooms.AddRange(sampleList);
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

    public string BoolToInfo(bool check)
    {
        if(check) return "On";
        return "Off";
    }

    public void InformationDisplay(GameObject roomInstance, RoomInfo room)
    {
        Transform informationPanel = roomInstance.transform.Find("informationPanel");
        roomInstance.transform.Find("mapDisplay").Find("mapDisplayName").GetComponent<Text>().text = room.CustomProperties["Map"].ToString();
        informationPanel.Find("resourcesPanel").Find("value").GetComponent<TextMeshProUGUI>().text = room.CustomProperties["Resources"].ToString();
        informationPanel.Find("turnsLimitPanel").Find("value").GetComponent<TextMeshProUGUI>().text = room.CustomProperties["TurnsLimit"].ToString();
        informationPanel.Find("weatherPanel").Find("value").GetComponent<TextMeshProUGUI>().text = room.CustomProperties["Weather"].ToString();
        informationPanel.Find("texturesPanel").Find("value").GetComponent<TextMeshProUGUI>().text = room.CustomProperties["LandTexture"].ToString();
        informationPanel.Find("fogPanel").Find("value").GetComponent<TextMeshProUGUI>().text = room.CustomProperties["Fog"].ToString();
        informationPanel.Find("dominationPanel").Find("value").GetComponent<TextMeshProUGUI>().text = room.CustomProperties["Domination"].ToString();
        informationPanel.Find("powersPanel").Find("value").GetComponent<TextMeshProUGUI>().text = room.CustomProperties["Powers"].ToString();
    }

    public void PlayersDisplay(GameObject roomInstance, RoomInfo room)
    {
        int[] letters = (int[])room.CustomProperties["Alliances"];
        for (int i = 0; i < room.MaxPlayers; i++)
        {
            Debug.Log(letters[i]);
        }
        string[] playerNames = (string[])room.CustomProperties["PlayerNames"];
        for (int i = 0; i < room.MaxPlayers; i++)
        {
            Debug.Log(playerNames[i]);
        }
        
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
        PhotonNetwork.SetPlayerCustomProperties(myCustomProperties);
        PhotonNetwork.JoinRoom(room.Name);
        //room.InsertPlayer(index, "Me");
        //waitingRoomWindow.GetComponent<WaitingRoomWindow>().SetRoom(room);
        //StartCoroutine(Hold1(index));
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
