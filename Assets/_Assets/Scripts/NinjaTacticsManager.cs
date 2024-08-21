using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NinjaTacticsManager : NetworkBehaviour
{
    [Header("패널")]
    public GameObject startPanel;
    public GameObject multiPlayerParents;
    public GameObject localprofiles;
    public GameObject[] profiles = new GameObject[5]; //멀티 인원 체력 표시
    [Header("server")]
    [SyncVar]
    public int serverPlayerNum = 0;
    public int localPlayerNum = -1;

    public List<int> playerSelects = new List<int>(5);
    public List<string> playerNames = new List<string>(5);
    public List<int> playerHPs = new List<int>(5) { -1, -1, -1, -1, -1 };
    public int selectType = -1;
    public float multiHp = 1;

    public override void OnStartServer()
    {
        base.OnStartServer();
        startPanel.SetActive(false);
        localprofiles.SetActive(false);
    }

    public override void OnStartClient()
    {
        startPanel.SetActive(true);
        localprofiles.SetActive(true);
        CmdAddPlayerNum();
        localPlayerNum = serverPlayerNum;
    }

    public void OnClickSelectNinja(int type)
    {
        selectType = type;
    }
    public void OnClickReady()
    {
        if (selectType >= 3)
            return;
        DBManager.instance.myCon.agent.enabled = true;
        DBManager.instance.myCon.enabled = true;
        DBManager.instance.PlayerNinjaType = selectType;
        startPanel.SetActive(false);
        CmdUpdateSelects(selectType);
        CmdUpdateName(DBManager.instance.playerName);
        CmdMakeProfile();
        LocalSetNinja();
        DBManager.instance.myCon.ChangeAnimatorCon();
        CmdUpdateHP(localPlayerNum, DBManager.instance.myCon.HP);
    }


    #region Cmd+Rpc
    


    [Command(requiresAuthority = false)]
    private void CmdUpdateSelects(int _type)
    {
        RpcUpdateSelects(_type);
    }

    [ClientRpc]
    private void RpcUpdateSelects(int _type)
    {
        playerSelects[serverPlayerNum - 1] = _type;
    }

    [Command(requiresAuthority = false)]
    private void CmdUpdateName(string _name)
    {
        RpcUpdateName(_name);
    }

    [ClientRpc]
    private void RpcUpdateName(string _name)
    {
        playerNames[serverPlayerNum - 1] = _name;
    }


    [Command(requiresAuthority = false)]
    public void CmdMakeProfile()
    {
        RpcMakeProfile();
    }

    [ClientRpc]
    private void RpcMakeProfile()
    {
        //====================
        // 1. 플레이어를 찾는다
        // 2. NinjaProfile 할당한다.
        // 3. 
        //====================

        for (int i = 0; i < serverPlayerNum; i++)
        {
            profiles[i].GetComponent<NinjaProfile>().SetEnableProfile(playerSelects[i]);
            profiles[i].GetComponent<NinjaProfile>().SetName(playerNames[i]);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateHP(int _playerNum,int _hp)
    {
        RpcUpdateHP(_playerNum, _hp);
    }

    [ClientRpc]
    private void RpcUpdateHP(int _playerNum, int _hp)
    {
        playerHPs[_playerNum] = _hp;
        profiles[_playerNum].GetComponent<NinjaProfile>().UpdateHp(_playerNum, playerSelects[_playerNum], playerHPs[_playerNum]);
    }
    #endregion

    [Command(requiresAuthority = false)]
    private void CmdAddPlayerNum()
    {
        serverPlayerNum++;
    }

    private void LocalSetNinja()
    {
        if (DBManager.instance.myCon == null)
            return;
        DBManager.instance.myCon.StartNinja();
        localprofiles.GetComponent<NinjaProfile>().SetEnableProfile(selectType);
        DBManager.instance.myCon.localHpImg = localprofiles.GetComponent<NinjaProfile>().hpSlides[selectType];
        DBManager.instance.myCon.multiHpImg = profiles[serverPlayerNum - 1].GetComponent<NinjaProfile>().hpSlides[selectType];
    }

}