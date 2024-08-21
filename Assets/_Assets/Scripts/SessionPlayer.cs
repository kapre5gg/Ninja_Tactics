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

    [SyncVar(hook = nameof(OnChangeNickName))] //싱크바는 command 에서 바꿀때만 작동한다?
    public string nickName = null;
    // onenable 에서 실행하면 로컬검사를 통과하지 못하는데,
    // 이는 네트워크메니저에서 instantiate 시 시간이 일관적이지 않음, 그래서 로컬을 바로 검사하면 안될때가 있다.
    // 그래서 callback 함수인 로컬플레이어 스타트를 쓴다
    // 확인 : https://mirror-networking.gitbook.io/docs/manual/components/networkbehaviour
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
    //후크는 클라만 한다.
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
