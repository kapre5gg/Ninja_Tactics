using kcp2k;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerNetworkManager : MonoBehaviour
{
    public GameObject serverPanel;
    public GameObject serverStatusPanel;
    public GameObject hostStatusPanel;
    public GameObject clientStatusPanel;

    public TMP_Text infoText;
    private KcpTransport transport;
    private void Start()
    {
        switch (MainManager.Instance.authStatus)
        {
            case MainManager.ClientStatus.SERVER:
                serverPanel.SetActive(true);
                break;
            case MainManager.ClientStatus.MASTERCLIENT:
                serverPanel.SetActive(true);
                break;
            case MainManager.ClientStatus.CLIENT:
                serverPanel.SetActive(false);
                break;
        }
        switch (MainManager.Instance.clientStatus)
        {
            case MainManager.ClientStatus.SERVER:
                serverStatusPanel.SetActive(true);
                hostStatusPanel.SetActive(false);
                clientStatusPanel.SetActive(false);
                break;
            case MainManager.ClientStatus.MASTERCLIENT:
                serverStatusPanel.SetActive(false);
                hostStatusPanel.SetActive(true);
                clientStatusPanel.SetActive(false);
                break;
            case MainManager.ClientStatus.CLIENT:
                serverStatusPanel.SetActive(false);
                hostStatusPanel.SetActive(false);
                clientStatusPanel.SetActive(true);
                break;
        }
        transport = FindObjectOfType<KcpTransport>();
    }

    private void Update()
    {
        PrintStatus();
    }
    private void PrintStatus()
    {
        if (NetworkServer.active && NetworkClient.active) // 1. ȣ��Ʈ��� 
        {
            infoText.text = $"<b>HOST<b> : running {Transport.active} port {transport.port}";
        }
        else if (NetworkServer.active) // 2. �����¸����
        {
            infoText.text = $"<b>SERVER<b> : running {Transport.active} port {transport.port}";
        }
        else if (NetworkClient.active) // 3. Ŭ����
        {
            infoText.text = $"<b>CLIENT<b> : connected to {NetworkManager.singleton.networkAddress}" +
                            $"running {Transport.active} port {transport.port}";
        }
        else // ������ ����
        {
            infoText.text = $"running {Transport.active} port {transport.port}";
        }


    }

    public void StopServer()
    {
        MainManager.Instance.nextSceneNumber = 1;
        NetworkManager.singleton.StopServer();
        SceneManager.LoadSceneAsync("00 LOADING");
    }
    public void StopHost()
    {
        MainManager.Instance.nextSceneNumber = 1;
        NetworkManager.singleton.StopHost();
        SceneManager.LoadSceneAsync("00 LOADING");
    }
    public void Stopclient()
    {
        MainManager.Instance.nextSceneNumber = 1;
        NetworkManager.singleton.StopClient();
        SceneManager.LoadSceneAsync("00 LOADING");
    }
    public void StopHostClient()
    {
        //ȣ��Ʈ���� ������ ��ȯ
        MainManager.Instance.clientStatus = MainManager.ClientStatus.SERVER;
        NetworkManager.singleton.StopClient();
        serverStatusPanel.SetActive(true);
        hostStatusPanel.SetActive(false);
        clientStatusPanel.SetActive(false);
    }
}
