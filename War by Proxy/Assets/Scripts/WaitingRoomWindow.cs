using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class WaitingRoomWindow : MonoBehaviourPunCallbacks
{
    //public MenuManager.Room room;
    private List<Color> playerColors = new List<Color>{ Color.red, Color.blue, Color.green, Color.yellow };
    private Photon.Realtime.Player[] listings;
    private GameObject[] playerPanels;
    private string[] players;
    private char[] teams;

    public GameObject mapDisplay;
    public GameObject PlayersList;
    public GameObject resourcesPanel;
    public GameObject turnsLimitPanel;
    public GameObject weatherPanel;
    public GameObject texturesPanel;
    public GameObject fogPanel;
    public GameObject dominationPanel;
    public GameObject powersPanel;
    public GameObject PlayerPrefab;

    /*public override void OnEnable()
    {
        base.OnEnable();
        GetCurrentRoomPlayers();
    }*/

    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(this.gameObject.GetComponent<Animation>()["SwipeAway2"].length - 0.01f);
        this.gameObject.transform.Find("confirmButton").gameObject.SetActive(true);
    }

    public void InitPlayers()
    {
        Room room = PhotonNetwork.CurrentRoom;
        int[] letters = (int[])room.CustomProperties["Alliances"];
        //string[] playerNames = (string[])room.CustomProperties["PlayerNames"];
        //players = new string[room.MaxPlayers];
        teams = new char[room.MaxPlayers];
        for(int i = 0; i < room.MaxPlayers; i++)
        {
            /*if(playerNames[i] != "")
            {
                players[i] = playerNames[i];
            }
            else
            {
                players[i] = "Empty Slot";
            }*/
            teams[i] = System.Convert.ToChar(64 + letters[i]);
        }
    }

    public void Init()
    {
        GetCurrentRoomPlayers();
        InformationDisplay();
        InitPlayers();
        PlayersDisplay();
    }

    public void Reset()
    {
        foreach(Transform child in PlayersList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    /*public void SetRoom(MenuManager.Room newRoom)
    {
        room = newRoom;
    }*/

    public string BoolToInfo(bool check)
    {
        if(check) return "On";
        return "Off";
    }

    public void InformationDisplay()
    {
        /*mapDisplay.GetComponent<Text>().text = room.roomMap;
        resourcesPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = room.roomResources.ToString();
        turnsLimitPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = room.roomTurnsLimit.ToString();
        weatherPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = room.roomWeather;
        texturesPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = room.roomTextures;
        fogPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = BoolToInfo(room.roomFog);
        dominationPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = BoolToInfo(room.roomDomination);
        powersPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = BoolToInfo(room.roomPowers);*/

        Debug.Log("Displaying Room Information");
        mapDisplay.GetComponent<Text>().text = PhotonNetwork.CurrentRoom.CustomProperties["Map"].ToString();
        resourcesPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.CustomProperties["Resources"].ToString();
        turnsLimitPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.CustomProperties["TurnsLimit"].ToString();
        weatherPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.CustomProperties["Weather"].ToString();
        texturesPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.CustomProperties["LandTexture"].ToString();
        fogPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.CustomProperties["Fog"].ToString();
        dominationPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.CustomProperties["Domination"].ToString();
        powersPanel.transform.Find("value").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.CustomProperties["Powers"].ToString();
    }

    public void Refresh()
    {
        foreach(GameObject panel in playerPanels)
        {
            Destroy(panel);
        }
        PlayersDisplay();
    }

    public void PlayersDisplay()
    {
        for(int i = 0; i < listings.Length; i++)
        {
            playerPanels[i] = Instantiate(PlayerPrefab, PlayersList.transform);
            if(teams[i] != '@')
            {
                playerPanels[i].transform.Find("Team").gameObject.SetActive(true);
                playerPanels[i].transform.Find("Team").Find("TeamLetter").GetComponent<TextMeshProUGUI>().text = teams[i].ToString();
            }
            playerPanels[i].transform.Find("Color").GetComponent<Image>().color = playerColors[i];
            if(listings[i] != null)
            {
                playerPanels[i].transform.Find("SlotName").GetComponent<TextMeshProUGUI>().text = listings[i].NickName;
            }
            else
            {
                playerPanels[i].transform.Find("SlotName").GetComponent<TextMeshProUGUI>().text = "Empty Slot";
            }
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        //PhotonNetwork.LeaveRoom(true);
        //QuitRoom(/*(int)PhotonNetwork.LocalPlayer.CustomProperties["Spot"]*/);
        this.gameObject.transform.Find("returnButton").GetComponent<Button>().onClick.Invoke();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        AddPlayerListing(newPlayer, (int)newPlayer.CustomProperties["Index"]);
        /*string[] newPlayerNames = (string[])PhotonNetwork.CurrentRoom.CustomProperties["PlayerNames"];
        newPlayerNames[(int)newPlayer.CustomProperties["Index"]] = newPlayer.NickName;
        ExitGames.Client.Photon.Hashtable changedProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        changedProperties["PlayerNames"] = newPlayerNames;
        PhotonNetwork.CurrentRoom.SetCustomProperties(changedProperties);*/
        Refresh();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        listings[(int)otherPlayer.CustomProperties["Index"]] = null;
        /*string[] newPlayerNames = (string[])PhotonNetwork.CurrentRoom.CustomProperties["PlayerNames"];
        newPlayerNames[(int)otherPlayer.CustomProperties["Index"]] = "";
        ExitGames.Client.Photon.Hashtable changedProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        changedProperties["PlayerNames"] = newPlayerNames;
        PhotonNetwork.CurrentRoom.SetCustomProperties(changedProperties);*/
        Refresh();
    }

    private void GetCurrentRoomPlayers()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
        {
            return;
        }
        listings = new Photon.Realtime.Player[PhotonNetwork.CurrentRoom.MaxPlayers];
        playerPanels = new GameObject[PhotonNetwork.CurrentRoom.MaxPlayers];
        foreach (KeyValuePair<int, Photon.Realtime.Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            AddPlayerListing(playerInfo.Value, (int)playerInfo.Value.CustomProperties["Index"]);
        }
    }

    private void AddPlayerListing(Photon.Realtime.Player player, int index)
    {
        //int index = listings.FindIndex(x => x.UserId == player.UserId);
        if (listings[index] != null)
        {
            Debug.Log("Player is already in the room");
        }
        else
        {
            listings[index] = player;
        }
    }

    public void QuitRoom()
    {
        StartCoroutine(this.gameObject.GetComponent<WaitingRoomWindow>().Wait());
        //this.gameObject.GetComponent<WaitingRoomWindow>().room.InsertPlayer(index, null);
        PhotonNetwork.LeaveRoom(true);
        Debug.Log("Did i leave the room?");
        this.Reset();
        //this.gameObject.transform.Find("returnButton").GetComponent<Button>().onClick.RemoveListener(() => QuitRoom());
    }
}
