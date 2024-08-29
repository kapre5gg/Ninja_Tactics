using Mirror;
using UnityEngine;

public enum ServerNum
{
    //포트번호 고정
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
        SERVER,         // 서버
        CLIENT,         // 일반 사용자
        MASTERCLIENT    // 개발자용
    }

    //빌드시 프로그램 사용 주체 설정
    [Header("실행 권한 상태")]
    public ClientStatus authStatus;

    //런타임시 사용 주체 설정
    [Header("클라이언트 상태")]
    public ClientStatus clientStatus;

    [Header("Scene 정보")]
    public int nextSceneNumber;
    [Scene]
    public string[] scenes = new string[3];

    [Header("IP 정보")]
    public const string IPADDR = "localhost";
    public const string IPADDR_Local = "192.168.100.38";
    public string moveADDR = "localhost";
    [Header("Port 정보")]
    public const int DEFAULTPORT = 7777;
    public const int LOBBYPORT = 7777;
    public const int ROOM1PORT = 7778;
    public int MoveScenePort; //동적포트
    [Header("Room 정보")]
    public int connectionRoomIndex = -1; //지금은 못쓰는 중
    public string connectionRoomPort = ""; //방에 접속중 일 경우 접속중인 port
    public bool isRoomMaster = false; //본인이 방의 방장인지 여부확인
    [Header("게임 캐릭터")]
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
