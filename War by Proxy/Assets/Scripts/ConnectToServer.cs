using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public GameObject mainMenuWindow;
    public GameObject loadingScreen;
    public MasterManager myMaster;

    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            print("Already connected.");
            if(loadingScreen.activeInHierarchy)
            {
                print("Joined lobby");
                mainMenuWindow.SetActive(true);
                loadingScreen.SetActive(false);
            }
        }
        else
        {
            print("Connecting to server.");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.NickName = myMaster._gameSettings.NickName;
            PhotonNetwork.GameVersion = myMaster._gameSettings.GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        if(loadingScreen.activeInHierarchy)
        {
            print("Connected to server.");
            print(PhotonNetwork.LocalPlayer.NickName);

            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print("Disconnected from server for reason " + cause.ToString());
    }

    public override void OnJoinedLobby()
    {
        if(loadingScreen.activeInHierarchy)
        {
            print("Joined lobby");
            mainMenuWindow.SetActive(true);
            loadingScreen.SetActive(false);
        }
    }
}
