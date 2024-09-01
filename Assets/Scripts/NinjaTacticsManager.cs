using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NinjaTacticsManager : NetworkBehaviour
{
    [Header("�г�")]
    public GameObject readyPanel;
    public GameObject multiPlayerParents;
    public GameObject localprofiles;
    public GameObject startPanel;
    public GameObject endingPanel;
    public Button startBtn;
    public TMP_Text startText;
    public GameObject[] profiles = new GameObject[5]; //��Ƽ �ο� �����ʵ�

    [Header("local")]
    public int localPlayerNum = -1;
    public float multiHp = 1;
    public int selectType = -1;
    public CameraController myCamera;

    [Header("server")]
    [SyncVar]
    public int serverPlayerNum = 0;
    public int AlivePlayers = 0;
    public List<int> playerSelects = new List<int>();
    public List<string> playerNames = new List<string>();
    public List<int> playerHPs = new List<int>(5);
    public List<NinjaController> playerNinjaCons = new List<NinjaController>();
    public bool ISGamePlay = false;
    public bool MissionClear = false;

    [Header("Timer")]
    public TMP_Text timerText;
    private int PlayTime = -1;
    private WaitForSeconds waitOneSecond = new WaitForSeconds(1);

    [Header("Ending")]
    [SerializeField] private TMP_Text lastTime;
    [SerializeField] private TMP_Text endingTitleText;
    [SerializeField] private TMP_Text endingText;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image endingImage;

    [Header("Flow")]
    public GameGuideLine gameGuideLine;

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
            startText.text += "\r\n������ ������ �غ� �� �Դϴ�.";
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
        endingPanel.SetActive(false);
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
        AvatarHelper avataHelper = DBManager.instance.myCon.gameObject.GetComponent<AvatarHelper>();
        avataHelper.EnableEquipment(selectType);
        readyPanel.SetActive(false);
        CmdUpdateSelects(selectType);
        CmdUpdateName(DBManager.instance.playerName);
        CmdMakeProfile();
        LocalSetNinja();
        //DBManager.instance.myCon.CmdChangeAnimCon();
        CmdUpdateHP(localPlayerNum, DBManager.instance.myCon.curHP);
        CmdCountAlivePlayers();
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
        gameGuideLine.GuideFlow(0);
    }

    [Command(requiresAuthority = false)]
    private void CmdUpdateSelects(int _type)
    {
        playerSelects.Add(_type);
        RpcUpdateSelects(playerSelects);
    }
    [ClientRpc]
    private void RpcUpdateSelects(List<int> _typeList)
    {
        playerSelects = _typeList;
    }

    [Command(requiresAuthority = false)]
    private void CmdUpdateName(string _name)
    {
        playerNames.Add(_name);
        RpcUpdateName(playerNames);
    }
    [ClientRpc]
    private void RpcUpdateName(List<string> _nameList)
    {
        playerNames = _nameList;
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
        // 1. �÷��̾ ã�´�
        // 2. NinjaProfile �Ҵ��Ѵ�.
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

    [Command(requiresAuthority = false)]
    public void CmdCountAlivePlayers()
    {
        AlivePlayers = playerHPs.Count(n => n > 0); //���� ����ִ� ����� �� ����
        RpcCountAlivePlayers(AlivePlayers);
    }
    [ClientRpc]
    private void RpcCountAlivePlayers(int _count)
    {
        AlivePlayers = _count;
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

    #region MissionSys
    //gameGuideLine ��ũ��Ʈ�� 
    #endregion

    #region EndingSys
    public bool _bol = true;
    [Command(requiresAuthority = false)]
    public void CmdCheackAllDeath()
    {
        print("���� ����");
        _bol = true;
        foreach (int p in playerHPs)
        {
            if (p > 0)
                _bol = false;
        }
        if (_bol)
        {
            print("��� ����");
            MissionClear = false;
            ISGamePlay = false;
            RpcDisplayEnding();
        }
    }
    [ClientRpc]
    public void RpcDisplayEnding()
    {
        endingPanel.SetActive(true);
        lastTime.text = FormatTime(PlayTime);
        SetEndingText();
        SetEndingScore(PlayTime, DBManager.instance.myCon.killCount);
    }
    private void SetEndingText()
    {
        if (MissionClear)
        {
            endingTitleText.text = "��         ��";
            endingText.text = "��ǥ�� ���ְ� ��ȭ�� �����.";
            endingImage.sprite = Resources.Load<Sprite>($"Win_Image");
        }
        else
        {
            endingTitleText.text = "��         ��";
            endingText.text = "��� �����̱���. ��.";
            endingImage.sprite = Resources.Load<Sprite>($"Lose_Image");
        }   
    }
    private void SetEndingScore(int _playTime, int _killCount)
    {
        killCountText.text = _killCount.ToString();
        switch (_playTime)
        {
            case int n when (n < 20):
                scoreText.text = "�����Ӱ���";
                break;

            case int n when (n >= 20) && (n < 60):
                scoreText.text = "������";
                break;

            case int n when (n >= 60) && (n < 120):
                scoreText.text = "������";
                break;

            case int n when (n >= 120):
                scoreText.text = "��������";
                break;

            default:
                break;
        }

        if (_killCount >= 10)
        {
            scoreText.text += " ���� �л���";
        }
        else if (_killCount >= 5)
        {
            scoreText.text += " ���õ� ����";
        }
        else if (_killCount > 0)
        {
            scoreText.text += " ������";
        }
        else
        {
            scoreText.text += " ��ȭ������";
        }
    }

    #endregion
}