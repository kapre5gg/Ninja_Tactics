using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using kcp2k;

/* LoadingManager에서 수행하는 일
 * 
 * 1. 로딩 상태 표시 (다음 씬 전환)
 * 2. 다음 씬 전환
 */

public class LoadingManager : MonoBehaviour
{
    public Slider slider;
    private float time;
    private float delay = 0.5f;

    private KcpTransport transport;

    // Start is called before the first frame update
    void Start()
    {
        transport = FindObjectOfType<KcpTransport>();

        // 1. 다음 씬 정보에 따라 씬 전환 함수 선택하여 호출
        RunNextScene();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // 2. 실시간으로 씬 전환 상태 슬라이더에 표시
        //CheckProgress();
    }

    void RunNextScene()
    {
        // 씬 넘버 활용
        /* 0 : 0 LOADING씬 (Offline)
         * 1 : 1 TITLE  씬 (Offline)
         * 2 : 2 LOBBY  씬 (Online)
         * 3 : 3 ROOM1  씬 (Online)
         */
        switch (MainManager.Instance.nextSceneNumber)
        {
            case 0:
                StartCoroutine(LoadOfflineScene());
                break;
            case 1:
                StartCoroutine(LoadOfflineScene());
                break;
            case 2:
                StartCoroutine(LoadOnlineScene());
                break;
            case 3:
                StartCoroutine(LoadOnlineScene());
                break;
            default:
                break;
        }
    }

    // 씬 전환 로딩 진행수치 슬라이더에 반환
    void CheckProgress()
    {
        // 1. 로딩이 가능한 상태라면
        if (NetworkManager.loadingSceneAsync == null) return;
        if (NetworkManager.loadingSceneAsync.isDone) return;

        // 2. 로딩 진행수치 슬라이더에 반환
        float speed = .1f;
        slider.value = Mathf.Lerp(slider.value, NetworkManager.loadingSceneAsync.progress, speed);
    }

    IEnumerator LoadOfflineScene()
    {
        // 1. 다음 씬 전환 상태정보 operation 으로 확인
        AsyncOperation operation = SceneManager.LoadSceneAsync(MainManager.Instance.nextSceneNumber);
        operation.allowSceneActivation = false;

        yield return new WaitForSeconds(0.5f);

        // 2. 로딩창은 기본적으로 delay 시간만큼을 가지도록 설계함.
        while (!operation.isDone)
        {
            time += Time.deltaTime;
            slider.value = time / delay;

            if (time > delay)
            {
                operation.allowSceneActivation = true;

                yield return null;
            }
            yield return null;
        }
    }

    IEnumerator LoadOnlineScene()
    {
        //서버이동시 체크
        //1. ip주소
        //2. 이동할 포트
        ServerManager.instance.SetNetworkSetting(MainManager.Instance.moveADDR, MainManager.Instance.MoveScenePort.ToString());
        //3. 이동할 씬이름
        if (MainManager.Instance.nextSceneNumber == 2)
        {
            NetworkManager.singleton.onlineScene = MainManager.Instance.scenes[0];
            NetworkManager.singleton.playerPrefab = MainManager.Instance.LobbyCharacter;
        }
        else if (MainManager.Instance.nextSceneNumber == 3)
        {
            NetworkManager.singleton.onlineScene = MainManager.Instance.scenes[1];
            NetworkManager.singleton.playerPrefab = MainManager.Instance.NinjaCharacter;
        }
        else
            yield break;

        switch (MainManager.Instance.clientStatus)
        {
            case MainManager.ClientStatus.SERVER:
                NetworkManager.singleton.StartServer();
                break;
            case MainManager.ClientStatus.CLIENT:
                NetworkManager.singleton.StartClient();
                break;
            case MainManager.ClientStatus.MASTERCLIENT:
                NetworkManager.singleton.StartHost();
                break;
        }

        // 1. 다음 씬 전환 상태정보 operation 으로 확인
        //AsyncOperation operation = null;
        AsyncOperation operation = NetworkManager.loadingSceneAsync;
        yield return new WaitForSeconds(3f);
        operation.allowSceneActivation = false;
        // 2. 로딩창은 기본적으로 delay 시간만큼을 가지도록 설계함.
        // 참고 링크 : https://mirror-networking.gitbook.io/docs/manual/interest-management/scene
        while (!operation.isDone)
        {
            time += Time.deltaTime;
            slider.value = time / delay;

            if (time > delay)
            {
                operation.allowSceneActivation = true;

                yield return null;
            }
            yield return null;
        }

        /*
        while (!operation.isDone)
        {
            yield return null;
        
            slider.value = operation.progress;

            if (operation.progress >= 0.99f) operation.allowSceneActivation = true;
        }
        */
    }
}
