using UnityEngine;
using TMPro;
using Mirror;
using kcp2k;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{
    [HideInInspector]
    public int serverNum;
    private string infoString;
    private KcpTransport transport;

    public static ServerManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        
        transport = FindObjectOfType<KcpTransport>();
    }

    public string SetNetworkSetting (string _ipAddr, string _port)
    {
        // ���� ���� �ѹ� ������ ���ٸ� 2�� Scene ����
        serverNum = 2;

        string str = null;
        str = "SELECT : 2";
        str += System.Environment.NewLine + "IP ADDR : " + _ipAddr;
        NetworkManager.singleton.networkAddress = _ipAddr;

        // Port InputField �Է°� ushort ��ȯ�˻�
        ushort val = 0;
        if (ushort.TryParse(_port, out val))
        {
            transport.Port = val;
        }
        else
        {
            transport.Port = 0;
        }

        str += System.Environment.NewLine + "PORT : " + val.ToString();

        return str;
    }

    
    // ���� ����
    public void RunStartServer ()
    {
        MainManager.Instance.clientStatus = MainManager.ClientStatus.SERVER;

        // �ε��� ����
        SceneManager.LoadSceneAsync("00 LOADING");
    }

    // Ŭ���̾�Ʈ ����
    public void RunStartClient ()
    {
        MainManager.Instance.clientStatus = MainManager.ClientStatus.CLIENT;

        // �ε��� ����
        SceneManager.LoadSceneAsync("00 LOADING");
    }

    public void RunStartHost()
    {
        MainManager.Instance.clientStatus = MainManager.ClientStatus.MASTERCLIENT;
        // �ε��� ����
        SceneManager.LoadSceneAsync("00 LOADING");
    }
}
