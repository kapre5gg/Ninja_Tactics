using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NinjaTacticsManager : NetworkBehaviour
{
    [Header("패널")]
    public GameObject readyPanel;
    public GameObject multiPlayerParents;
    public GameObject localprofiles;
    public GameObject startPanel;
    public GameObject EndingPanel;
    public Button startBtn;
    public TMP_Text startText;
    public GameObject[] profiles = new GameObject[5]; //멀티 인원 체력 표시

    [Header("local")]
    public int localPlayerNum = -1;
    public float multiHp = 1;
    public int selectType = -1;
    public CameraController myCamera;

    [Header("server")]
    [SyncVar]
    public int serverPlayerNum = 0;
    public List<int> playerSelects = new List<int>(5);
    public List<string> playerNames = new List<string>(5);
    public List<int> playerHPs = new List<int>(5) { -1, -1, -1, -1, -1 };
    public List<NinjaController> playerNinjaCons = new List<NinjaController>(5);
    public bool ISGamePlay = false;
    public bool MissionClear = false;

    [Header("Timer")]
    public TMP_Text timerText;
    private int PlayTime = -1;
    private WaitForSeconds waitOneSecond = new WaitForSeconds(1);

    [Header("Ending")]
    public TMP_Text lastTime;
    public TMP_Text endingText;
    //========================================//

    public override void OnStartServer()
    {
        base.OnStartServer();
        OnOffPanel(false);
    }

    public override void OnStartClient()
    {
        OnOffPanel(true);
        CmdAddPlayerNum();
        localPlayerNum = serverPlayerNum;
        if (localPlayerNum == 0)
        {
            startBtn.gameObject.SetActive(true);
        }
        else
        {
            startBtn.gameObject.SetActive(false);
            startText.text += "\r\n방장이 게임을 준비 중 입니다.";
        }
    }
    public void GameStart()
    {
        CmdGameStart();
        DBManager.Session session = new DBManager.Session();
        session.StartNinjaTactics(MainManager.Instance.connectionRoomPort);
    }
    private void OnOffPanel(bool _bool)
    {
        readyPanel.SetActive(_bool);
        localprofiles.SetActive(_bool);
        startPanel.SetActive(_bool);
        EndingPanel.SetActive(false);
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
        readyPanel.SetActive(false);
        CmdUpdateSelects(selectType);
        CmdUpdateName(DBManager.instance.playerName);
        CmdMakeProfile();
        LocalSetNinja();
        DBManager.instance.myCon.ChangeAnimatorCon();
        CmdUpdateHP(localPlayerNum, DBManager.instance.myCon.curHP);
    }


    #region Cmd+Rpc

    [Command(requiresAuthority = false)]
    public void CmdGameStart()
    {
        RpcGameStart();
    }
    [ClientRpc]
    public void RpcGameStart()
    {
        startPanel.SetActive(false);
        ISGamePlay = true;
        SetTimer();
    }

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
    public void CmdUpdateHP(int _playerNum, int _hp)
    {
        playerHPs[_playerNum] = _hp;
        RpcUpdateHP(_playerNum, _hp);
    }
    [ClientRpc]
    private void RpcUpdateHP(int _playerNum, int _hp)
    {
        playerHPs[_playerNum] = _hp;
        profiles[_playerNum].GetComponent<NinjaProfile>().UpdateHp(playerSelects[_playerNum], playerHPs[_playerNum]);
    }


    [Command(requiresAuthority = false)]
    public void CmdAddNinjaCon(int _num, NinjaController _con)
    {
        playerNinjaCons.Add(_con);
        RpcAddNinjaCon(playerNinjaCons);
    }
    [ClientRpc]
    public void RpcAddNinjaCon(List<NinjaController> input)
    {
        playerNinjaCons = input;
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

    #region TimerSys

    public void SetTimer()
    {
        PlayTime = 0;
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        while (ISGamePlay)
        {
            yield return waitOneSecond;
            PlayTime++;
            DisplayTimer(PlayTime);
        }
    }

    private void DisplayTimer(int _time)
    {
        timerText.text = FormatTime(_time);
    }

    private string FormatTime(int _time)
    {
        int minutes = _time / 60;
        int seconds = _time % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    #endregion

    #region EndingSys
    public bool _bol = true;
    [Command(requiresAuthority = false)]
    public void CmdCheackAllDeath()
    {
        print("누가 죽음");
        _bol = true;
        foreach (int p in playerHPs)
        {
            if (p > 0)
                _bol = false;
        }
        if (_bol)
        {
            print("모두 죽음");
            MissionClear = false;
            ISGamePlay = false;
            RpcDisplayEnding();
        }
    }
    [ClientRpc]
    public void RpcDisplayEnding()
    {
        EndingPanel.SetActive(true);
        lastTime.text = FormatTime(PlayTime);
        SetEndingText();
    }
    private void SetEndingText()
    {
        if (MissionClear)
            endingText.text = "목표를 없애고 평화를 얻었다.";
        else
            endingText.text = "모두 죽으셨군요. 쯧.";
    }

    #endregion
}