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
        // 별도 서버 넘버 지정이 없다면 2번 Scene 지정
        serverNum = 2;

        string str = null;
        str = "SELECT : 2";
        str += System.Environment.NewLine + "IP ADDR : " + _ipAddr;
        NetworkManager.singleton.networkAddress = _ipAddr;

        // Port InputField 입력값 ushort 변환검사
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

    
    // 서버 실행
    public void RunStartServer ()
    {
        MainManager.Instance.clientStatus = MainManager.ClientStatus.SERVER;

        // 로딩씬 실행
        SceneManager.LoadSceneAsync("00 LOADING");
    }

    // 클라이언트 실행
    public void RunStartClient ()
    {
        MainManager.Instance.clientStatus = MainManager.ClientStatus.CLIENT;

        // 로딩씬 실행
        SceneManager.LoadSceneAsync("00 LOADING");
    }

    public void RunStartHost()
    {
        MainManager.Instance.clientStatus = MainManager.ClientStatus.MASTERCLIENT;
        // 로딩씬 실행
        SceneManager.LoadSceneAsync("00 LOADING");
    }
}
