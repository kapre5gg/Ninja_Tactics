using Cinemachine;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionPlayer : NetworkBehaviour
{
    public static SessionPlayer localPlayer;
    public Transform playerCameraRoot;
    public TMP_Text nickNameText;
    public GameObject floatingInfo;
    public GameObject speechBubble;
    [SyncVar(hook = nameof(OnChangeBubble))]
    private bool isActiveBubble = false;

    public TMP_Text bubbleText;
    [SyncVar(hook = nameof(OnChangeBubbleText))]
    private string bubbleChat = null;

    [SyncVar(hook = nameof(OnChangeNickName))] //��ũ�ٴ� command ���� �ٲܶ��� �۵��Ѵ�?
    public string nickName = null;
    // onenable ���� �����ϸ� ���ð˻縦 ������� ���ϴµ�,
    // �̴� ��Ʈ��ũ�޴������� instantiate �� �ð��� �ϰ������� ����, �׷��� ������ �ٷ� �˻��ϸ� �ȵɶ��� �ִ�.
    // �׷��� callback �Լ��� �����÷��̾� ��ŸƮ�� ����
    // Ȯ�� : https://mirror-networking.gitbook.io/docs/manual/components/networkbehaviour
    public override void OnStartLocalPlayer() 
    {
        GetComponent<UnityEngine.InputSystem.PlayerInput>().enabled = true;
        GetComponent<StarterAssets.ThirdPersonController>().enabled = true;
        CmdSetNickName(DBManager.instance.playerName);
        localPlayer = this;

        CinemachineVirtualCamera cam = FindObjectOfType<CinemachineVirtualCamera>();
        if (cam != null)
            cam.Follow = playerCameraRoot;
    }
    //��ũ�� Ŭ�� �Ѵ�.
    void OnChangeNickName(string _Old, string _New)
    {
        nickNameText.text = _New;
    }
    [Command]
    private void CmdSetNickName(string _nickname)
    {
        nickName = _nickname;
        nickNameText.text = _nickname;
    }

    void OnChangeBubble(bool _Old, bool _New)
    {
        speechBubble.SetActive(_New);
    }
    [Command]
    public void CmdActivateBubble(bool _active)
    {
        isActiveBubble = _active;
    }

    void OnChangeBubbleText(string _Old, string _New)
    {
        bubbleText.text = _New;
    }
    [Command]
    public void CmdBubbleChat(string _chat)
    {
        bubbleChat = _chat;
        bubbleText.text = _chat;
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            // make non-local players run this
            floatingInfo.transform.LookAt(Camera.main.transform);
            return;
        }
    }
}
