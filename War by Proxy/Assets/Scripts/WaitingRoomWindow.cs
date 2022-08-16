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
    public GameObject PlayerPrefab;

    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(this.gameObject.GetComponent<Animation>()["SwipeAway2"].length - 0.01f);
        this.gameObject.transform.Find("confirmButton").gameObject.SetActive(true);
    }

    public void InitPlayers()
    {
        Room room = PhotonNetwork.CurrentRoom;
        int[] letters = (int[])room.CustomProperties["Alliances"];
        teams = new char[room.MaxPlayers];
        for(int i = 0; i < room.MaxPlayers; i++)
        {
            teams[i] = System.Convert.ToChar(64 + letters[i]);
        }
    }

    public void Init()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount != PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            this.gameObject.transform.Find("confirmButton").GetComponent<Button>().interactable = false;
        }
        GetCurrentRoomPlayers();
        mapDisplay.GetComponent<Text>().text = PhotonNetwork.CurrentRoom.CustomProperties["MapName"].ToString();
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

    public void Refresh()
    {
        foreach(GameObject panel in playerPanels)
        {
            Destroy(panel);
        }
        PlayersDisplay();
        if(PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            this.gameObject.transform.Find("confirmButton").GetComponent<Button>().interactable = true;
        }
        else
        {
            this.gameObject.transform.Find("confirmButton").GetComponent<Button>().interactable = false;
        }
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
        this.gameObject.transform.Find("returnButton").GetComponent<Button>().onClick.Invoke();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        AddPlayerListing(newPlayer, (int)newPlayer.CustomProperties["Index"]);
        Refresh();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        listings[(int)otherPlayer.CustomProperties["Index"]] = null;
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
        if (listings[index] != null)
        {
            Debug.LogError("Player is already in the room");
        }
        else
        {
            listings[index] = player;
        }
    }

    public void QuitRoom()
    {
        StartCoroutine(this.gameObject.GetComponent<WaitingRoomWindow>().Wait());
        PhotonNetwork.LeaveRoom(true);
        this.Reset();
    }
}
