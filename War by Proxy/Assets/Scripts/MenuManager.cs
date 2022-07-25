using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public GameObject mainMenuWindow;
    public GameObject multiplayerWindow;
    public GameObject optionsWindow;
    public GameObject roomEditorWindow;
    public GameObject waitingRoomWindow;
    public GameObject playersEditorWindow;
    public GameObject roomListWindow;
    public GameObject loadingScreen;

    public TMP_Dropdown resolutionsDropdown;
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public GameObject masterSliderText;
    Resolution[] resolutions;
    public bool showgrid = false; 

    int resolutionOption;
    int fullscreenOption;
    int showgridOption;
    float volumeOption;

    public Room newRoom;
    public List<RoomInfo> listings = new List<RoomInfo>();

    IEnumerator Hold1(GameObject window)
    {
        window.GetComponent<Animation>().Play("SwipeAway");
        yield return new WaitForSeconds(window.GetComponent<Animation>()["SwipeAway"].length);
        window.SetActive(false);
    }

    IEnumerator Hold2(GameObject window)
    {
        yield return new WaitForSeconds(window.GetComponent<Animation>()["SwipeIn"].length);
        window.SetActive(true);
        window.GetComponent<Animation>().Play("SwipeIn");
    }

    IEnumerator Hold3(GameObject window)
    {
        window.GetComponent<Animation>().Play("SwipeAway2");
        yield return new WaitForSeconds(window.GetComponent<Animation>()["SwipeAway2"].length);
        window.SetActive(false);
    }

    IEnumerator Hold4(GameObject window)
    {
        yield return new WaitForSeconds(window.GetComponent<Animation>()["SwipeIn2"].length);
        window.SetActive(true);
        window.GetComponent<Animation>().Play("SwipeIn2");
    }

    IEnumerator Fade1(GameObject window)
    {
        window.GetComponent<Animation>().Play("FadeAway");
        yield return new WaitForSeconds(window.GetComponent<Animation>()["FadeAway"].length);
        window.SetActive(false);
    }

    IEnumerator Fade2(GameObject window)
    {
        yield return new WaitForSeconds(window.GetComponent<Animation>()["FadeIn"].length);
        window.SetActive(true);
        window.GetComponent<Animation>().Play("FadeIn");
    }

    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionsDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for(int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + "@" + resolutions[i].refreshRate + "hz";
            options.Add(option);
            if(PlayerPrefs.GetInt("resolution", -1) == -1 && resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height && resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionsDropdown.AddOptions(options);
        resolutionsDropdown.value = currentResolutionIndex;
        resolutionsDropdown.RefreshShownValue();

        LoadOptions();
        SetMasterVolume(volumeOption);
        masterSlider.value = volumeOption;
        optionsWindow.transform.Find("fullscreenCheckmarkPanel").Find("checkmarkButton").Find("checkmark").gameObject.SetActive(Convert.ToBoolean(fullscreenOption));
        optionsWindow.transform.Find("showGridCheckmarkPanel").Find("checkmarkButton").Find("checkmark").gameObject.SetActive(Convert.ToBoolean(showgridOption));
        if(resolutionOption != -1)
        {
            resolutionsDropdown.value = resolutionOption;
            resolutionsDropdown.RefreshShownValue();
        }
    }

    public void SetMasterVolume(float volume)
    {
        int percentageVolume = Mathf.RoundToInt((volume + 80) * 1.25f);
        masterSliderText.GetComponent<TextMeshProUGUI>().text = "%" + percentageVolume.ToString();
        audioMixer.SetFloat("masterVolume", volume);
        PlayerPrefs.SetFloat("volume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        optionsWindow.transform.Find("fullscreenCheckmarkPanel").Find("checkmarkButton").Find("checkmark").gameObject.SetActive(isFullscreen);
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("fullscreen", Convert.ToInt32(isFullscreen));
    }

    public void SetShowGrid(bool gridIsShown)
    {
        optionsWindow.transform.Find("showGridCheckmarkPanel").Find("checkmarkButton").Find("checkmark").gameObject.SetActive(gridIsShown);
        showgrid = gridIsShown;
        PlayerPrefs.SetInt("showgrid", Convert.ToInt32(gridIsShown));
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("resolution", resolutionIndex);
    }

    public void LoadOptions()
    {
        resolutionOption = PlayerPrefs.GetInt("resolution", -1);
        fullscreenOption = PlayerPrefs.GetInt("fullscreen", 1);
        showgridOption = PlayerPrefs.GetInt("showgrid", 0);
        volumeOption = PlayerPrefs.GetFloat("volume", -40f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SwipeAway(GameObject window)
    {
        StartCoroutine(Hold1(window));
    }

    public void SwipeIn(GameObject window)
    {
        StartCoroutine(Hold2(window));
    }

    public void SwipeAway2(GameObject window)
    {
        StartCoroutine(Hold3(window));
    }

    public void SwipeIn2(GameObject window)
    {
        StartCoroutine(Hold4(window));
    }

    public void FadeAway(GameObject window)
    {
        StartCoroutine(Fade1(window));
    }

    public void FadeIn(GameObject window)
    {
        StartCoroutine(Fade2(window));
    }

    public void Reset()
    {
        roomEditorWindow.GetComponent<RoomEditorWindow>().SetIndex(0);
    }

    public void CreateRoom(bool online)
    {
        newRoom = new Room();
        if(online)
        {
            newRoom.SetOnline(true);
        } 
        else
        {
            newRoom.SetOnline(false);
        }
    }

    public void CreateNewRoom1()
    {
        newRoom.setMap(roomEditorWindow.GetComponent<RoomEditorWindow>().GetMap());
        newRoom.SetSpots(newRoom.roomMap.GetSpots());
        playersEditorWindow.GetComponent<PlayersEditorWindow>().SetRoom(newRoom);
    }

    public void CreateNewRoom2()
    {
        newRoom.SetAlliances(playersEditorWindow.GetComponent<PlayersEditorWindow>().GetArray());
        newRoom.InitializePlayers();
        newRoom.InsertPlayer(playersEditorWindow.GetComponent<PlayersEditorWindow>().GetHostSpot(), PhotonNetwork.LocalPlayer.NickName);
        ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();
        playerCustomProperties["Index"] = playersEditorWindow.GetComponent<PlayersEditorWindow>().GetHostSpot();
        playerCustomProperties["ShowGrid"] = showgrid;
        PhotonNetwork.SetPlayerCustomProperties(playerCustomProperties);

        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        ExitGames.Client.Photon.Hashtable myCustomProperties = new ExitGames.Client.Photon.Hashtable();
        int value = UnityEngine.Random.Range(0, 9999);
        newRoom.setName(newRoom.roomMap + value.ToString());
        RoomOptions options = new RoomOptions();
        myCustomProperties["MapName"] = newRoom.roomMap.GetName();
        myCustomProperties["Width"] = newRoom.roomMap.GetWidth();
        myCustomProperties["Height"] = newRoom.roomMap.GetHeight();
        myCustomProperties["Alliances"] = newRoom.alliances;
        myCustomProperties["PlayerNames"] = newRoom.players;
        options.BroadcastPropsChangeToAll = true;
        options.MaxPlayers = ((byte)newRoom.playerSpots);
        options.IsVisible = true;
        options.CustomRoomProperties = myCustomProperties;
        options.CustomRoomPropertiesForLobby = new string[5]
        {
            "MapName",
            "Width",
            "Height",
            "Alliances",
            "PlayerNames",
        };
        PhotonNetwork.CreateRoom(newRoom.roomMap + value.ToString(), options, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        SwipeAway(playersEditorWindow);
        SwipeIn(waitingRoomWindow);
        waitingRoomWindow.GetComponent<WaitingRoomWindow>().Init();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Room creation failed. " + message);
    }

    public override void OnJoinedLobby()
    {
        print("Joined lobby");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList || info.MaxPlayers == info.PlayerCount)
            {
                int index = listings.FindIndex( x => x.Name == info.Name);
                if (index != -1)
                {
                    listings.RemoveAt(index);
                }
            }
            else
            {
                int index = listings.FindIndex(x => x.Name == info.Name);
                if(index == -1)
                {
                        listings.Add(info);
                }
            }
        }
        if(roomListWindow.activeInHierarchy)
        {
            roomListWindow.GetComponent<RoomListWindow>().ClearList();
            roomListWindow.GetComponent<RoomListWindow>().InitRoomsList();
        }
    }

    public override void OnConnectedToMaster()
    {
        if(!PhotonNetwork.InLobby && !loadingScreen.activeInHierarchy)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public void MatchStarted()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(1);
        }
    }

    public class Room
    {
        public string roomName;
        public RoomEditorWindow.Map roomMap;
        public int playerSpots;
        public string[] players;
        public int[] alliances;

        public bool isCreated;
        public bool isOnline;

        public Room()
        {
            this.roomName = "Test";
            this.roomMap = null;
            this.playerSpots = 2;
            this.players = new string[playerSpots];
            this.alliances = new int[playerSpots];

            this.isCreated = false;
            this.isOnline = false;
        }

        public void SetOnline(bool online)
        {
            this.isOnline = online;
        }

        public void setName(string newName)
        {
            this.roomName = newName;
        }

        public void setMap(RoomEditorWindow.Map newMap)
        {
            this.roomMap = newMap;
        }

        public void SetSpots(int spots)
        {
            this.playerSpots = spots;
        }

        public void InsertPlayer(int index, string playerName)
        {
            players[index] = playerName;
        }

        public void SetAlliances(int[] array)
        {
            for(int i = 0; i < playerSpots; i++)
            {
                alliances[i] = array[i];
            }
        }

        public void InitializePlayers()
        {
            for(int i = 0; i < playerSpots; i++)
            {
                players[i] = "";
            }
        }
    }
}