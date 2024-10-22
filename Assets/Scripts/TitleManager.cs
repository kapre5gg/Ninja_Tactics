using Mirror;
using Org.BouncyCastle.Tls;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public GameObject serverPanel;
    public GameObject clientPanel;
    public GameObject loginPanel;
    public GameObject signinPanel;
    private void Start()
    {
        signinPanel.SetActive(false);
        switch (MainManager.Instance.authStatus)
        {
            case MainManager.ClientStatus.SERVER:
                serverPanel.SetActive(true);
                clientPanel.SetActive(false);
                loginPanel.SetActive(false);
                break;
            case MainManager.ClientStatus.MASTERCLIENT:
                serverPanel.SetActive(true);
                clientPanel.SetActive(false);
                loginPanel.SetActive(true);
                break;
            case MainManager.ClientStatus.CLIENT:
                serverPanel.SetActive(false);
                clientPanel.SetActive(true);
                loginPanel.SetActive(true);
                break;
        }
    }
    public void RunStartClient()
    {
        //로그인 시도시 이후 로그인 성공시 접속함수를 실행
        MainManager.Instance.nextSceneNumber = 2;
        MainManager.Instance.clientStatus = MainManager.ClientStatus.CLIENT;
        SceneManager.LoadSceneAsync("00 LOADING");
        // 그냥 로그인 버튼만 누르면 안되는 형상때문에 추가
        ServerManager.instance.serverNum = 2;

        MainManager.Instance.moveADDR = MainManager.IPADDR_Local;
        MainManager.Instance.MoveScenePort = MainManager.LOBBYPORT;
    }

    public void SigninBtn()
    {
        loginPanel.SetActive(false);
        signinPanel.SetActive(true);
    }
    public void RegistBtn()
    {
        loginPanel.SetActive(true);
        signinPanel.SetActive(false);
    }
}
