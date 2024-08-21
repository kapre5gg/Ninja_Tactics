using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
public class ChattingManager : NetworkBehaviour
{
    //®X ±â´É 
    //Çü½Ä : [ÀÌ¸§] Ã¤ÆÃ³»¿ë
    //ÅØ½ºÆ®³»¿ª¿¡ µû¶ó ¾Ë¾Æ¼­ Á¶ÀýµÊ
    //<color=>, <b>±½°Ô, <i> ±â¿ïÀÌ±â, <u> ¹ØÁÙ, 
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
    //¸ðµÎ¿¡°Ô Ç¥½ÃÇØ¾ßÇÒ °Å : Ã¤ÆÃÅØ½ºÆ®

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

    [Command(requiresAuthority = false)] //ºÒÆ¯Á¤ ´Ù¼ö¿¡°Ô º¸³»±â
    private void CmdSendChat(string _playerName, string _chat)
    {
        //¼­¹ö¿¡°Ô ¿äÃ»ÇÒ ³»¿ë : ÅØ½ºÆ®¸¦ ¸ðµÎ¿¡°Ô Ãß°¡ÇÏ±â
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
