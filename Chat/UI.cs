using Photon.Pun;
using Assets.Core.Drawing.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public string userName;
    public Roles roleName = Roles.OPERATOR;

    // SaunaChat
    public Text statusText;                 // текст отображения статуса подключения
    public InputField chatInputField;       // поле введения текста для чата
    public Button sendTextButton;           // кнопка отправки сообщения   
    public GameObject messageContent;
    public GameObject chatPanel;            // панель чата
    public Scrollbar chatScrollbar;         // скроллбар чата
    public Text messageTemplate;
    public Text nicknameText;
    public int fontSize;
    public Color ownColor;
    public Color rcwdColor;

    public Button menuButton;
    public GameObject menuPanel;

    public float reconnectTime = 5f;
    private float timer = 0;

    public Text pingText;
    public Text usersCount;
    public Text roomText;
    public Text isOpenText;

    //UserLogin
    public GameObject arCamera;

    private DevChat chat;
    private DevLobby lobby;
    public Roles role;
    public static string userNickname;

    // Рисование
    public GameObject BrushDraw;
    public GameObject Model;
    public Material DrawMaterial;
    public GameObject ObjectDraw;
    public DrawShape RectanglePrefab;
    public DrawShape CirclePrefab;
    public DrawShape TrianglePrefab;

    public void Update()
    {
        timer += Time.deltaTime;
    }

    // Обновляем информацию о пользователях в комнате, задаем собеседника
    public void UpdatePlayers()
    {
        chat.UpdateTargetPlayer();
    }

    public void OnSendTextButtonClicked()
    {
        if (chatInputField.text == "") return;
        chat.SendTextMessage(chat.targetPlayer, chatInputField.text);
        chatInputField.text = "";
    }

    public void OnMenuButtonClicked()
    {
        Animator animator = menuPanel.GetComponent<Animator>();
        if (animator != null)
        {
            menuPanel.SetActive(!menuPanel.active);
            
        }
    }

    void InitializeChat()
    {
        chat = GetComponent<DevChat>();
        chat.Initialize();
    }

    // Включение\выключение видеочата
    public void OnTakePhotoButtonClicked()
    {
        //chat.TakeScreenShotAndSend();
        chat.StartVideoChat();
        // запоминаем положение модели в синглтон
        PositionService.getInstance().ModelPosition = Model.transform.position;
    }
    
    private void Start()
    {
        statusText.text = PhotonNetwork.NetworkClientState.ToString();

        role = Roles.OPERATOR;

        string userNickname = userName + " | " + role;

        PhotonNetwork.NickName = userNickname;
        lobby = GetComponent<DevLobby>();
        lobby.ConnectToMaster();
    }
}
