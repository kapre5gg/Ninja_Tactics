using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
public class ChattingManager : NetworkBehaviour
{
    //�X ��� 
    //���� : [�̸�] ä�ó���
    //�ؽ�Ʈ������ ���� �˾Ƽ� ������
    //<color=>, <b>����, <i> ����̱�, <u> ����, 
    [Header("chat")]
    public TMP_InputField chatInput;
    public TMP_Text chatText;
    public Scrollbar chatBar;
    private void Update()
    {
        if (chatInput.isFocused)
        {
            SessionPlayer.localPlayer.GetComponent<PlayerInput>().enabled = false;
            SessionPlayer.localPlayer.CmdActivateBubble(true);
        }
        else
        {
            SessionPlayer.localPlayer.GetComponent<PlayerInput>().enabled = true;
            SessionPlayer.localPlayer.CmdActivateBubble(false);
        }

        if (Input.GetKeyUp(KeyCode.Return)) 
        {
            EnterChat();
        }
    }
    //��ο��� ǥ���ؾ��� �� : ä���ؽ�Ʈ

    public void EnterChat()
    {
        if (chatInput.text.Trim().Length < 1)
            return;
        CmdSendChat(DBManager.instance.playerName, chatInput.text);
        chatInput.text = "";
        chatInput.ActivateInputField();
        StartCoroutine(WaitChatt());
    }

    private IEnumerator WaitChatt()
    {
        yield return null;
        yield return null;
        chatBar.value = 0;
    }

    [Command(requiresAuthority = false)] //��Ư�� �ټ����� ������
    private void CmdSendChat(string _playerName, string _chat)
    {
        //�������� ��û�� ���� : �ؽ�Ʈ�� ��ο��� �߰��ϱ�
        RpcSendChatting(_playerName, _chat);
    }

    [ClientRpc]
    private void RpcSendChatting(string _playerName, string _chat)
    {
        if (DBManager.instance.playerName == _playerName)
            chatText.text += $"\n<color=blue>[{_playerName}]</color> : " + _chat;
        else
            chatText.text += $"\n<color=white>[{_playerName}]</color> : " + _chat;
    }
}
