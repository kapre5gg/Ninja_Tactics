using Mirror;
using UnityEngine;

public enum ServerNum
{
    //��Ʈ��ȣ ����
    LOADING,    // 0 
    TITLE,      // 1 
    LOBBY,      // 2 7777
    ROOM1       // 3 7778~
}

public class MainManager : MonoBehaviour
{
    private static MainManager _instance;
    public static MainManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<MainManager>();
            return _instance;
        }
    }

    public enum ClientStatus
    {
        SERVER,         // ����
        CLIENT,         // �Ϲ� �����
        MASTERCLIENT    // �����ڿ�
    }

    //����� ���α׷� ��� ��ü ����
    [Header("���� ���� ����")]
    public ClientStatus authStatus;

    //��Ÿ�ӽ� ��� ��ü ����
    [Header("Ŭ���̾�Ʈ ����")]
    public ClientStatus clientStatus;

    [Header("Scene ����")]
    public int nextSceneNumber;
    [Scene]
    public string[] scenes = new string[3];

    [Header("IP ����")]
    public const string IPADDR = "localhost";
    public const string IPADDR_Local = "192.168.100.38";
    public string moveADDR = "localhost";
    [Header("Port ����")]
    public const int DEFAULTPORT = 7777;
    public const int LOBBYPORT = 7777;
    public const int ROOM1PORT = 7778;
    public int MoveScenePort; //������Ʈ
    [Header("Room ����")]
    public int connectionRoomIndex = -1; //������ ������ ��
    public string connectionRoomPort = ""; //�濡 ������ �� ��� �������� port
    public bool isRoomMaster = false; //������ ���� �������� ����Ȯ��
    [Header("���� ĳ����")]
    public GameObject LobbyCharacter;
    public GameObject NinjaCharacter;
    void Awake()
    {
        if (_instance == null) _instance = FindObjectOfType<MainManager>();
        if (_instance != this) Destroy(this);
        else DontDestroyOnLoad(this);
        MoveScenePort = 7777;
    }
}
