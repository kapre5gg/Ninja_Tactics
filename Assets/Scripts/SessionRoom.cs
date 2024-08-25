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

    private void OnTriggerEnter(Collider other) //�� ���� ���
    {
        data = session.LoadSessionInfo();
        SessionPlayer player = other.GetComponent<SessionPlayer>();
        if (player != null && player.isLocalPlayer)
        {
            //ui �˾� ����
            roomPanel.SetActive(true);
            for (int i = 0; i < sessionItemList.Length; i++)
            {
                if (i < data.Rows.Count && (int)data.Rows[i][2] <= 5)
                {
                    sessionItemList[i].SetActive(true);
                    sessionName[i].text = data.Rows[i][1].ToString();
                    joinPlayerNum[i].text = data.Rows[i][2].ToString() + " / 5 ��";
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
        //ip�ּ�, ��Ʈ��ȣ, ��������� 3���� ������ �ʿ��ϴ�
        //���� ip, ���� ���� �־ �ٸ� ��Ʈ�� �ٸ� ���̴�.
        //������ �� ���� ��Ʈ �ϳ��� �� ������ �� �� �ִ�.
        // const �� instance ���� �����´�.?
        if (port == "")
            return;
        MainManager.Instance.MoveScenePort = int.Parse(port);
        if (session.CheckOverFive(port))
            return;
        //����, ȣ��Ʈ�� �̵��� �Ҵٴ� > �̵��ϸ� ���� ���� ������ �����Ƿ�
        //Ŭ�� �̵�
        MainManager.Instance.nextSceneNumber = 3;
        //��ž�� �ϸ� ���� ������ ���� �������� ������ �Ѿ�µ�, �������ο��� �ε����� ��ϵǾ�����
        //�׷��� �ε������� �Ѿ�ٰ� ���ư�����
        // ���� �ο��� �߰��ϱ�
        session.IncreasePlayerNum(port);
        
        serverIdx++;
        Debug.Log(serverIdx);
        
        //������ ���� uuid�� �ҷ��;���(port�� �޵��� ����)
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
            print("��� ��Ʈ�� �����");
            CreatePanel.SetActive(false);
        }
        
    }
    private void RealCreateRoomBtn()
    {
        #region ���ο� ��Ʈ �� �����
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

        //�� �� ���ؼ� �����
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
    private string SelectPort() //���ο� ��Ʈ�� �� ���� �� �ʿ��� �Լ�
    {
        int port = 7778 + data.Rows.Count;
        return port.ToString();
    }
}
