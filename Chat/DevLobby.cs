using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class DevLobby : MonoBehaviourPunCallbacks
{
    public static DevLobby lobby;

    private string currentRoomName;
    UI ui;

    // Конструктор класса, где мы задаем имя пользователя и его роль, и подключаемся к Серверу Photon'a
    public void ConnectToMaster()
    {
        lobby = this;
        PhotonNetwork.AuthValues = new AuthenticationValues(PhotonNetwork.LocalPlayer.UserId);
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log($"Connecting to the Master server...");
        ui = GetComponent<UI>();
        ui.statusText.text = PhotonNetwork.NetworkClientState.ToString();
    }
    
    // При успешном подключении к Серверу Photon'a
    public override void OnConnectedToMaster()
    {
        Debug.Log($"{ui.role} connected to the Master server.");
        switch (ui.role)
        {
            case Roles.DISPATCHER:
                {
                    CreateRoom(PhotonNetwork.NickName);                 // Если пользователь ДИСПЕТЧЕР, то создаем комнату
                    break;
                }
            case Roles.OPERATOR:
                {
                    Debug.Log(PhotonNetwork.CountOfRooms);
                    PhotonNetwork.JoinRandomRoom();                     // Если пользователь ОПЕРАТОР, то подключаемся к случайной комнате
                    break;
                }
        }
    }

    

    // При ошибке подключения к случайной комнате
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"ERROR:  Failed to join room. {message}");
    }

    // Создание комнаты
    public void CreateRoom(string roomName)
    {
        currentRoomName = "Room " + roomName;
        Debug.Log("Trying to create new room " + "'" + currentRoomName + "'");
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(currentRoomName, roomOptions);
    }

    // При ошибке создания комнаты
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("ERROR:   Failed to create room " + "'" + currentRoomName + "'.  " + message);
    }

    // Отключение от сервера
    public void Disconnect()
    {

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} joined room.");
        if (ui.role == Roles.DISPATCHER)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                Debug.Log($"Room CLOSED");
            }
        }
        ui.UpdatePlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} left room.");
        if (ui.role == Roles.DISPATCHER)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                PhotonNetwork.CurrentRoom.IsOpen = true;
                Debug.Log($"Room OPENED");
            }
        }
        ui.UpdatePlayers();
    }

    // При подключении к комнате
    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room {currentRoomName}");
        gameObject.SendMessage("InitializeChat");
        if (ui.role != Roles.DISPATCHER) ui.UpdatePlayers();
    }
}

