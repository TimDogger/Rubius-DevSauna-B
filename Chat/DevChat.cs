using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using System.Linq;
using Assets.Core.Common;
using Assets.Core.Drawing.Scripts;
using Assets.Core.Drawing.Scripts.BrushDrawing;
using UnityEngine;
using UnityEngine.UI;

public enum Roles { OPERATOR, DISPATCHER };

public class DevChat : MonoBehaviour, IChatClientListener
{
    public ChatClient chatClient;
    /*public Text statusText;
    public InputField chatInputField;
    public Button sendTextButton;
    public GameObject messageContent;
    public Text userNameText;
    public Text roomText;
    public GameObject chatPanel;
    public Scrollbar chatScrollbar;
    public Text messageTemplate;
    public GameObject videoPanel;
    public RawImage rawImage;*/

    public Photon.Realtime.Player targetPlayer;    // собеседник

    private Texture2D texture2D;
    private string chatAppId;
    private string chatAppVersion;
    private ChatState chatState;
    private Roles userRole;
    private byte[] videoFrameBytes;
    private int videoQuality = 75;
    private UI ui;
    private float updateRate = (float)0.1;
    private float videoRate = (float)0.033;
    private float timePassed = 0;

    // Для рисования
    private DrawController _drawController;
    // Associates a draw mode to the prefab to instantiate
    private Dictionary<Glossary.DrawMode, DrawShape> _drawModeToPrefab;

    // Возвращает первого игрока в текущей комнате, который имеет заданную роль
    Photon.Realtime.Player GetFirstPlayerWithRole(Roles role)
    {
        // получаем массив игроков (не включая себя)
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerListOthers.ToArray<Photon.Realtime.Player>();

        // с помощью LINQ выбираем игрока, у которого имя содержит нужную роль
        Photon.Realtime.Player player = players.First(p => p.NickName.Contains(role.ToString()));

        return player;
    }

    public void StartVideoChat()
    {
        DevVideoChat.grab = true;
        UpdateVideoFrameBytes(videoQuality);
        SendVideoFrame(targetPlayer.NickName, videoFrameBytes);
    }
    public void StopVideoChat()
    {
        DevVideoChat.grab = false;
    }

    // Обновяем собеседника
    public void UpdateTargetPlayer()
    {
        targetPlayer = PhotonNetwork.PlayerListOthers[0];
    }

    /*private void UpdateVideoFrame(Texture2D texture)
    {
        ui.rawImage.texture = texture;
    }*/
    private void UpdateVideoFrameBytes(int quality)
    {
        videoFrameBytes = DevVideoChat.texture2D.EncodeToJPG(quality);
    }

    private void SendVideoFrame(string nickname, byte[] bytes)
    {
        chatClient.SendPrivateMessage(nickname, bytes);
    }

    public void TakeScreenShotAndSend()
    {
        /*DevVideoChat.TakeScreenshot();
        //UpdateVideoFrame(DevVideoChat.texture2D);
        if (DevVideoChat.frameBytes != null)
        {
            SendVideoFrame(targetPlayer.NickName, DevVideoChat.frameBytes);
        }
        else
        {
            AddMessageToChat("debug", "null image array");
        }*/
    }


    // Отправка сообщения по нажатию на кнопку
    public void SendTextMessage(Photon.Realtime.Player player, object textToSend)
    {
        // отправка личного сообщения
        chatClient.SendPrivateMessage(player.NickName, textToSend);

        // добавляем сообщением в UI
        AddMessageToChat(PhotonNetwork.NickName, textToSend);
        ui.chatInputField.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (chatClient != null)
        {
            timePassed += Time.deltaTime;

            // Обновление состояния и отладочной информации
            if (timePassed > updateRate)
            {
                chatClient.Service();
                UpdateDebugInfo();
            }

            // Обновление видео по частоте кадров
            if (DevVideoChat.grab && timePassed > videoRate)
            {
                //UpdateVideoFrame(DevVideoChat.texture2D);
                //SendVideoFrame(targetPlayer.NickName, videoFrameBytes);
            }
        }
    }

    void SetUserRole(int role)
    {
        userRole = (Roles)role;
    }

    // Подключение к серверу Photon Chat 
    public void Initialize()
    {
        chatClient = new ChatClient(this);
        ui = GetComponent<UI>();
        ui.nicknameText.text = PhotonNetwork.NickName;
        chatAppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
        chatAppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat;
        
        ui.statusText.color = Color.white;

        Debug.Log($"Connecting to the server  using {chatAppId} , {chatAppVersion} , {PhotonNetwork.NickName}");

        if (chatClient.Connect(chatAppId, chatAppVersion, new AuthenticationValues(PhotonNetwork.NickName)))
        {
            Debug.Log("Connected!");
            texture2D = new Texture2D(Screen.width, Screen.height);
        }
        else
            Debug.Log("Connection failed!");

        ui.chatPanel.SetActive(true);

        //DevVideoChat.TakeScreenshot();

        _drawController = new DrawController(ui.BrushDraw, ui.ObjectDraw);
        _drawModeToPrefab = new Dictionary<Glossary.DrawMode, DrawShape> {
            {Glossary.DrawMode.Rectangle, ui.RectanglePrefab},
            {Glossary.DrawMode.Circle, ui.CirclePrefab},
            {Glossary.DrawMode.Triangle, ui.TrianglePrefab}
        };
    }

    // Обновление отладочной информации
    void UpdateDebugInfo()
    {
        ui.pingText.text = $"Ping: {PhotonNetwork.GetPing()}";
        ui.roomText.text = $"Room: {PhotonNetwork.CurrentRoom.Name.ToString()}";
        ui.usersCount.text = $"Users: {PhotonNetwork.CurrentRoom.PlayerCount.ToString()}";
        ui.nicknameText.text = PhotonNetwork.NickName;
        ui.isOpenText.text = $"Is Room open =  {PhotonNetwork.CurrentRoom.IsOpen.ToString()}";
    }

    public void AddMessageToChat(string sender, object message)
    {
        GameObject newMessage = new GameObject("Message" + ui.messageContent.transform.childCount);
        newMessage.transform.parent = ui.messageContent.transform;

        Text newText = newMessage.AddComponent<Text>();
        newText.text = sender + ":  " + message.ToString();
        newText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        newText.fontSize = ui.fontSize;
        if (sender == ui.userName) newText.color = ui.ownColor;
        else newText.color = ui.rcwdColor;

        LayoutElement layoutElement = newMessage.AddComponent<LayoutElement>();
        Debug.Log(newText.text);
    }

    public void AddMessageToChat(object message)
    {
        GameObject newMessage = new GameObject("Message" + ui.messageContent.transform.childCount);
        newMessage.transform.parent = ui.messageContent.transform;

        Text newText = newMessage.AddComponent<Text>();
        newText.text = message.ToString();
        newText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        newText.fontSize = ui.fontSize;
        newText.color = Color.grey;
        LayoutElement layoutElement = newMessage.AddComponent<LayoutElement>();
        Debug.Log(newText.text);

    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        switch (message.GetType().ToString())
        {
            // Если сообщение с типом byte
            case "System.Byte[]":
                {
                    texture2D.LoadImage((byte[])message);
                    texture2D.Apply();
                    break;
                }
            // Если сообщение с типом string
            case "System.String":
                {
                    Photon.Realtime.Player[] players = PhotonNetwork.PlayerListOthers.ToArray<Photon.Realtime.Player>();
                    if (sender != PhotonNetwork.LocalPlayer.NickName)
                        AddMessageToChat(sender, message);
                    break;
                }
            case "System.Object[]":
                {
                    var messageArray = (object[])message;
                    var drawMode = (Glossary.DrawMode)messageArray[0];

                    if (drawMode == Glossary.DrawMode.Brush)
                    {
                        _drawController.DrawBrush((Vector3[])messageArray[1],
                            (Vector2)messageArray[2],
                            ui.arCamera.GetComponent<Camera>(),
                            ui.Model.transform.position);
                    }
                    else if (drawMode == Glossary.DrawMode.Circle || drawMode == Glossary.DrawMode.Rectangle ||
                             drawMode == Glossary.DrawMode.Triangle)
                    {
                        var shapeToDraw = Instantiate(_drawModeToPrefab[drawMode]);

                        _drawController.DrawObject(drawMode, shapeToDraw,
                            (Vector3[])messageArray[1],
                            (Vector2)messageArray[2],
                            ui.arCamera.GetComponent<Camera>(),
                            ui.Model.transform.position);
                    }

                    break;
                }
            case "System.Int32":
                {
                    var mode = (Glossary.DrawMode)message;
                    if (mode == Glossary.DrawMode.Clear)
                    {
                        _drawController.Clear();
                    }
                    break;
                }
            default:
                {
                    Debug.Log($"Message had an unexpected type {message.GetType().ToString()}");
                    break;
                }
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {

    }

    public void OnChatStateChange(ChatState state)
    {
        chatState = state;
        ui.statusText.text = state.ToString();
        if (!chatClient.CanChat)
        {
            ui.chatInputField.enabled = false;
        }
        else
        {
            ui.chatInputField.enabled = true;
        }
    }

    public void OnConnected()
    {
        ui.statusText.text = "В сети";
        ui.statusText.color = Color.green;
    }

    private byte[] Texture2DToBytes(Texture2D texture)
    {
        var bytes = texture.EncodeToJPG();

        return bytes;
    }

    public void OnDisconnected()
    {
        chatClient.Disconnect();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        throw new System.NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        throw new System.NotImplementedException();
    }

    public void OnUnsubscribed(string[] channels)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    void Start()
    {
    }

}
