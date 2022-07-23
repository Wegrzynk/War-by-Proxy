using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public GameObject mainMenuWindow;
    public GameObject campaignWindow;
    public GameObject multiplayerWindow;
    public GameObject optionsWindow;
    public GameObject extrasWindow;
    public GameObject generalOptionsWindow;
    public GameObject videoOptionsWindow;
    public GameObject audioOptionsWindow;
    public GameObject roomEditorWindow;
    public GameObject matchEditorWindow;
    public GameObject waitingRoomWindow;
    public GameObject playersEditorWindow;
    public GameObject roomListWindow;
    public GameObject navigationMap;
    public GameObject loadingScreen;

    public AudioMixer audioMixer;
    public GameObject masterSliderText;
    public GameObject musicSliderText;
    public GameObject effectsSliderText;

    public GameObject matchEditorCreateRoom;
    public GameObject matchEditorStartMatch;

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

    public void SetMasterVolume(float volume)
    {
        int percentageVolume = Mathf.RoundToInt((volume + 80) * 1.25f);
        masterSliderText.GetComponent<TextMeshProUGUI>().text = "%" + percentageVolume.ToString();
        audioMixer.SetFloat("masterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        int percentageVolume = Mathf.RoundToInt((volume + 80) * 1.25f);
        musicSliderText.GetComponent<TextMeshProUGUI>().text = "%" + percentageVolume.ToString();
        audioMixer.SetFloat("musicVolume", volume);
    }

    public void SetEffectsVolume(float volume)
    {
        int percentageVolume = Mathf.RoundToInt((volume + 80) * 1.25f);
        effectsSliderText.GetComponent<TextMeshProUGUI>().text = "%" + percentageVolume.ToString();
        audioMixer.SetFloat("effectsVolume", volume);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeMode(GameObject checkmark)
    {
        if(checkmark.activeInHierarchy)
        {
            checkmark.SetActive(false);
        } 
        else 
        {
            checkmark.SetActive(true);
        }
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
        matchEditorWindow.GetComponent<MatchEditorWindow>().SetIndex(0);
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

    public void SampleRooms()
    {
        Room sampleRoom1 = new Room();
        int[] sampleAlliances1 = new int[]{ 0, 0, 0, 0 };
        sampleRoom1.setMap("Two Islands");
        sampleRoom1.SetOnline(true);
        sampleRoom1.setData(1000, 20, false, true, false, "Clear", "Regular", "Balanced");
        sampleRoom1.SetAlliances(sampleAlliances1);
        sampleRoom1.InsertPlayer(0, "Host");
        sampleRoom1.InsertPlayer(1, "Random_Player");

        Room sampleRoom2 = new Room();
        int[] sampleAlliances2 = new int[]{ 1, 1, 2, 2 };
        sampleRoom2.setMap("Desolation");
        sampleRoom2.SetOnline(true);
        sampleRoom2.setData(1000, 15, true, false, true, "Rain", "Wasteland", "Balanced");
        sampleRoom2.SetAlliances(sampleAlliances2);
        sampleRoom2.InsertPlayer(0, "Host");

        Room sampleRoom3 = new Room();
        int[] sampleAlliances3 = new int[]{ 3, 2, 2, 3 };
        sampleRoom3.setMap("Tundra");
        sampleRoom3.SetOnline(true);
        sampleRoom3.setData(2000, 0, true, true, false, "Blizzard", "Snowlands", "Balanced");
        sampleRoom3.SetAlliances(sampleAlliances3);
        sampleRoom3.InsertPlayer(0, "Host");

        List<Room> sampleRoomsList = new List<Room>{ sampleRoom1, sampleRoom2, sampleRoom3};
        roomListWindow.GetComponent<RoomListWindow>().ProvideSampleRooms(sampleRoomsList);
    }

    public void MatchEditorOnlineCheck()
    {
        if(newRoom.isOnline)
        {
            matchEditorWindow.transform.Find("aiGameplanPanel").gameObject.SetActive(false);
            matchEditorWindow.transform.Find("aiGameplanList").gameObject.SetActive(false);
            matchEditorCreateRoom.SetActive(true);
            matchEditorStartMatch.SetActive(false);
        }
        else{
            matchEditorWindow.transform.Find("aiGameplanPanel").gameObject.SetActive(true);
            matchEditorWindow.transform.Find("aiGameplanList").gameObject.SetActive(true);
            matchEditorCreateRoom.SetActive(false);
            matchEditorStartMatch.SetActive(true);
        }
    }

    public void CreateNewRoom1()
    {
        newRoom.setMap(roomEditorWindow.GetComponent<RoomEditorWindow>().GetMap());
        newRoom.SetSpots(2);
        playersEditorWindow.GetComponent<PlayersEditorWindow>().SetRoom(newRoom);
    }

    public void CreateNewRoom2()
    {
        newRoom.SetAlliances(playersEditorWindow.GetComponent<PlayersEditorWindow>().GetArray());
        newRoom.InitializePlayers();
        newRoom.InsertPlayer(playersEditorWindow.GetComponent<PlayersEditorWindow>().GetHostSpot(), PhotonNetwork.LocalPlayer.NickName);
        ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();
        playerCustomProperties["Index"] = playersEditorWindow.GetComponent<PlayersEditorWindow>().GetHostSpot();
        PhotonNetwork.SetPlayerCustomProperties(playerCustomProperties);
    }

    public void CreateNewRoom3(bool online)
    {
        MatchEditorWindow helper = matchEditorWindow.GetComponent<MatchEditorWindow>();
        newRoom.setData
        (
            int.Parse(helper.resourcesInput.GetComponent<TMP_InputField>().text),
            int.Parse(helper.turnsInput.GetComponent<TMP_InputField>().text),
            helper.fogCheckmark.activeSelf,
            helper.dominationCheckmark.activeSelf,
            helper.powersCheckmark.activeSelf,
            helper.selectedWeatherText.GetComponent<TextMeshProUGUI>().text,
            helper.selectedTexturesText.GetComponent<TextMeshProUGUI>().text,
            helper.selectedAIText.GetComponent<TextMeshProUGUI>().text
        );
        if(online)
        {
            if (!PhotonNetwork.IsConnected)
            {
                return;
            }
            ExitGames.Client.Photon.Hashtable myCustomProperties = new ExitGames.Client.Photon.Hashtable();
            int value = Random.Range(0, 9999);
            newRoom.setName(newRoom.roomMap + value.ToString());
            RoomOptions options = new RoomOptions();
            myCustomProperties["Map"] = newRoom.roomMap;
            myCustomProperties["Resources"] = newRoom.roomResources;
            myCustomProperties["TurnsLimit"] = newRoom.roomTurnsLimit;
            myCustomProperties["Weather"] = newRoom.roomWeather;
            myCustomProperties["LandTexture"] = newRoom.roomTextures;
            myCustomProperties["Fog"] = newRoom.roomFog;
            myCustomProperties["Domination"] = newRoom.roomDomination;
            myCustomProperties["Powers"] = newRoom.roomPowers;
            myCustomProperties["Alliances"] = newRoom.alliances;
            myCustomProperties["PlayerNames"] = newRoom.players;
            options.BroadcastPropsChangeToAll = true;
            options.MaxPlayers = 2;
            options.IsVisible = true;
            options.CustomRoomProperties = myCustomProperties;
            options.CustomRoomPropertiesForLobby = new string[10]
            {
                "Map",
                "Resources",
                "TurnsLimit",
                "Weather",
                "LandTexture",
                "Fog",
                "Domination",
                "Powers",
                "Alliances",
                "PlayerNames",
            };
            PhotonNetwork.CreateRoom(newRoom.roomMap + value.ToString(), options, TypedLobby.Default);
        }
    }

    public override void OnCreatedRoom()
    {
        SwipeAway(matchEditorWindow);
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
                    //Destroy(GameObject.Find(info.Name));
                    listings.RemoveAt(index);
                }
            }
            else
            {
                int index = listings.FindIndex(x => x.Name == info.Name);
                if(index == -1)
                {
                    //GameObject listing = Instantiate(roomPrefab, roomsListContent.transform);
                    //if (listing != null)
                    //{
                        //listing.SetRoomInfo(info);
                        listings.Add(info);
                    //}
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
        public string roomMap;
        public int playerSpots;
        public string[] players;
        public int[] alliances;
        public int roomResources;
        public int roomTurnsLimit;
        public bool roomFog;
        public bool roomDomination;
        public bool roomPowers;
        public string roomWeather;
        public string roomTextures;
        public string roomAI;

        public bool isCreated;
        public bool isOnline;

        public Room()
        {
            this.roomName = "Test";
            this.roomMap = "Map 2_1";
            this.playerSpots = 2;
            this.players = new string[playerSpots];
            this.alliances = new int[playerSpots];
            this.roomResources = 1000;
            this.roomTurnsLimit = 15;
            this.roomFog = false;
            this.roomDomination = false;
            this.roomPowers = false;
            this.roomWeather = "Clear";
            this.roomTextures = "Regular";
            this.roomAI = "Balanced";

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

        public void setMap(string newMap)
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

        public void setData(int newResources, int newTurnsLimit, bool newFog, bool newDomination, bool newPowers, string newWeather, string newTextures, string newAI)
        {
            this.roomResources = newResources;
            this.roomTurnsLimit = newTurnsLimit;
            this.roomFog = newFog;
            this.roomDomination = newDomination;
            this.roomPowers = newPowers;
            this.roomWeather = newWeather;
            this.roomTextures = newTextures;
            this.roomAI = newAI;
        }
    }
}