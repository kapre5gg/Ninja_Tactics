using kcp2k;
using Mirror;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SessionRoom : NetworkBehaviour
{
    [Header("Session UI")]
    public GameObject roomPanel;
    public GameObject[] sessionItemList;
    public TMP_Text[] sessionName;
    public TMP_Text[] joinPlayerNum;

    private string port = "";
    private DataTable data;
    private DBManager.Session session = new DBManager.Session();
    private KcpTransport transport;

    private int serverIdx = 0;

    [Header("Btn")]
    public Button createBtn;
    public Button[] portChangeBtns;
    [Header("CreateRoom UI")]
    public GameObject CreatePanel;
    public TMP_InputField createInputText;
    public Button realCreateRoomBtn;

    void Awake()
    {
        transport = FindObjectOfType<KcpTransport>();
    }
    private void Start()
    {
        data = session.LoadSessionInfo();
        createBtn.onClick.AddListener(CreateBtn);
        roomPanel.SetActive(false);
        CreatePanel.SetActive(false);
        realCreateRoomBtn.onClick.AddListener(RealCreateRoomBtn);

        for (int i = 0; i < sessionItemList.Length; i++)
        {
            int index = i;
            portChangeBtns[i].onClick.AddListener(() => ChangePortBtn(index));
        }
    }

    private void FixedUpdate()
    {
        data = session.LoadSessionInfo();
    }

    private void OnTriggerEnter(Collider other) //룸 고르기 기능
    {
        data = session.LoadSessionInfo();
        SessionPlayer player = other.GetComponent<SessionPlayer>();
        if (player != null && player.isLocalPlayer)
        {
            //ui 팝업 띄우기
            roomPanel.SetActive(true);
            for (int i = 0; i < sessionItemList.Length; i++)
            {
                if (i < data.Rows.Count && (int)data.Rows[i][2] <= 5)
                {
                    sessionItemList[i].SetActive(true);
                    sessionName[i].text = data.Rows[i][1].ToString();
                    joinPlayerNum[i].text = data.Rows[i][2].ToString() + " / 5 명";
                }
            }
        }
    }

    public void ChangePortBtn(int _index)
    {
        port = data.Rows[_index][3].ToString();
        MainManager.Instance.connectionRoomPort = port;
    }

    public void JoinRoomBtn()
    {
        //ip주소, 포트번호, 어느씬인지 3가지 정보가 필요하다
        //같은 ip, 같은 씬에 있어도 다른 포트면 다른 방이다.
        //서버를 열 때는 포트 하나당 한 개씩만 열 수 있다.
        // const 는 instance 없이 가져온다.?
        if (port == "")
            return;
        MainManager.Instance.MoveScenePort = int.Parse(port);
        if (session.CheckOverFive(port))
            return;
        //서버, 호스트는 이동이 불다능 > 이동하면 이전 씬의 서버가 닫히므로
        //클라만 이동
        MainManager.Instance.nextSceneNumber = 3;
        //스탑을 하면 서버 연결을 끊고 오프라인 씬으로 넘어가는데, 오프라인에는 로딩씬이 등록되어있음
        //그래서 로딩씬으로 넘어갔다가 돌아가야함
        // 방의 인원수 추가하기
        session.IncreasePlayerNum(port);
        
        serverIdx++;
        Debug.Log(serverIdx);
        
        //선택한 룸의 uuid를 불러와야함(port를 받도록 수정)
        MainManager.Instance.connectionRoomPort = port;

        NetworkManager.singleton.StopClient();
        SceneManager.LoadSceneAsync("00 LOADING");

    }

    private void CreateBtn()
    {
        try
        {
            int emptyRoomNum = session.LoadEmptyRoomNum();
            CreatePanel.SetActive(true);
        }
        catch 
        {
            print("모든 포트를 사용중");
            CreatePanel.SetActive(false);
        }
        
    }
    private void RealCreateRoomBtn()
    {
        #region 새로운 포트 방 만들기
        //ServerManager.instance.serverNum = 3;
        //NetworkManager.singleton.networkAddress = "localhost";
        //transport.Port = ushort.Parse(SelectPort());

        //MainManager.Instance.nextSceneNumber = 3;
        //NetworkManager.singleton.onlineScene = MainManager.Instance.scenes[1];
        //string roomName = createInputText.text;
        //DBManager.instance.sqlManager.SqlSendCmd($"INSERT INTO `metaversedb`.`session` (`uuid`, `RoomName`, `PlayerNum`, `RoomPort`) VALUES ('{SelectPort()}','{roomName}', '1', '{SelectPort()}');");
        //NetworkManager.singleton.StopClient();
        //NetworkManager.singleton.StartHost();
        #endregion

        //빈 방 구해서 만들기
        int emptyRoomNum = session.LoadEmptyRoomNum();
        string newRoomName = createInputText.text;
        if (newRoomName.Length < 1)
            return;
        MainManager.Instance.isRoomMaster = true;
        session.CreateNewRoom(emptyRoomNum, newRoomName);
        session.IncreasePlayerNum(emptyRoomNum);
        MainManager.Instance.MoveScenePort = int.Parse(session.GetPort(emptyRoomNum));
        MainManager.Instance.connectionRoomPort = session.GetPort(emptyRoomNum);
        MainManager.Instance.nextSceneNumber = 3;
        NetworkManager.singleton.StopClient();
        //NetworkManager.singleton.StartClient();
        SceneManager.LoadSceneAsync("00 LOADING");
    }
    private string SelectPort() //새로운 포트로 방 만들 시 필요한 함수
    {
        int port = 7778 + data.Rows.Count;
        return port.ToString();
    }
}
