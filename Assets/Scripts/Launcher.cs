using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private TMP_Text errorText;  
    [SerializeField] private TMP_Text roomNameText;

    [SerializeField] private Transform roomList;
    [SerializeField] private GameObject roomButtonPrefab;
    [SerializeField] private Transform playerList;
    [SerializeField] private GameObject playerTextPrefab;

    [SerializeField] private GameObject startGameButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("title");
        PhotonNetwork.NickName = $"Valas {Random.Range(0, 1338)}";
    }

    public void CreateRoom()
    {
        if (string.IsNullOrWhiteSpace(roomNameInputField.text)) return;
        
        PhotonNetwork.CreateRoom(roomNameInputField.text);
    }

    public override void OnJoinedRoom()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        MenuManager.Instance.OpenMenu("room");
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerList.childCount; i++)
        {
            Destroy(playerList.GetChild(i).gameObject);
        }
        
        foreach (var t in players)
        {
            Instantiate(playerTextPrefab, playerList).GetComponent<PlayerListItem>().SetUp(t);
        }
        
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = $"Error: {message}";
        MenuManager.Instance.OpenMenu("error");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> _roomList)
    {
        for (int i = 0; i < roomList.childCount; i++)
        {
            Destroy(roomList.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < _roomList.Count; i++)
        {
            if (_roomList[i].RemovedFromList) continue;
            Instantiate(roomButtonPrefab, roomList).GetComponent<RoomListItem>().SetUp(_roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        Instantiate(playerTextPrefab, playerList).GetComponent<PlayerListItem>().SetUp(player);
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }
}
