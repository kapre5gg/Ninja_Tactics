using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using kcp2k;

/* LoadingManager���� �����ϴ� ��
 * 
 * 1. �ε� ���� ǥ�� (���� �� ��ȯ)
 * 2. ���� �� ��ȯ
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

        // 1. ���� �� ������ ���� �� ��ȯ �Լ� �����Ͽ� ȣ��
        RunNextScene();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // 2. �ǽð����� �� ��ȯ ���� �����̴��� ǥ��
        //CheckProgress();
    }

    void RunNextScene()
    {
        // �� �ѹ� Ȱ��
        /* 0 : 0 LOADING�� (Offline)
         * 1 : 1 TITLE  �� (Offline)
         * 2 : 2 LOBBY  �� (Online)
         * 3 : 3 ROOM1  �� (Online)
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

    // �� ��ȯ �ε� �����ġ �����̴��� ��ȯ
    void CheckProgress()
    {
        // 1. �ε��� ������ ���¶��
        if (NetworkManager.loadingSceneAsync == null) return;
        if (NetworkManager.loadingSceneAsync.isDone) return;

        // 2. �ε� �����ġ �����̴��� ��ȯ
        float speed = .1f;
        slider.value = Mathf.Lerp(slider.value, NetworkManager.loadingSceneAsync.progress, speed);
    }

    IEnumerator LoadOfflineScene()
    {
        // 1. ���� �� ��ȯ �������� operation ���� Ȯ��
        AsyncOperation operation = SceneManager.LoadSceneAsync(MainManager.Instance.nextSceneNumber);
        operation.allowSceneActivation = false;

        yield return new WaitForSeconds(0.5f);

        // 2. �ε�â�� �⺻������ delay �ð���ŭ�� �������� ������.
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
        //�����̵��� üũ
        //1. ip�ּ�
        //2. �̵��� ��Ʈ
        ServerManager.instance.SetNetworkSetting(MainManager.Instance.moveADDR, MainManager.Instance.MoveScenePort.ToString());
        //3. �̵��� ���̸�
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

        // 1. ���� �� ��ȯ �������� operation ���� Ȯ��
        //AsyncOperation operation = null;
        AsyncOperation operation = NetworkManager.loadingSceneAsync;
        yield return new WaitForSeconds(3f);
        operation.allowSceneActivation = false;
        // 2. �ε�â�� �⺻������ delay �ð���ŭ�� �������� ������.
        // ���� ��ũ : https://mirror-networking.gitbook.io/docs/manual/interest-management/scene
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
