using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;

public class GameGuideLine : NetworkBehaviour
{
    [Header("Flow")]
    public int currentMission = -1;
    [SerializeField] private bool onTriggerPos; //지금 미션이 모이는 미션이면 true
    public List<Transform> camGuideLines = new List<Transform>();
    public List<Transform> effectGuideLines = new List<Transform>();
    public List<string> guideTexts = new List<string>();
    public List<BoxCollider> cameraBoundarys = new List<BoxCollider>();
    public List<GameObject> EnemySet = new List<GameObject>();
    private Vector3 currGuideLine;
    private RaycastHit hit;
    private WaitForSeconds waitThree = new WaitForSeconds(3);
    private Vector3 camOrigin;
    private Coroutine cor;
    private bool cmdActiveOnlyOne = false;
    [Header("Components")]
    public NinjaTacticsManager tacticsManager;
    public TriggerPos triggerPos;
    public GameObject guidePanel;
    public TMP_Text guideText;
    public CameraController camParent;
    public GameObject guideEffect;

    public Enemy targetEnemy;
    public int targetEnemyIndex = 0;
    public Enemy[] targetEnemys;

    private void Start()
    {
        EnableEnemy(0);
    }

    private void Update()
    {
        if (tacticsManager.ISGamePlay)
        {
            guidePanel.SetActive(true);
            if (onTriggerPos)
            {
                //targetEnemy = null;
                if (triggerPos.AllTogether)
                    NextGuideFlow();
            }
            if (!onTriggerPos)
            {
                if (targetEnemy == null)
                {
                    targetEnemy = targetEnemys[targetEnemyIndex];
                }
                if (targetEnemy != null && targetEnemy.isDead)
                {
                    NextGuideFlow();
                }
            }
            if (currentMission == 6)
            {
                GameWin();
            }
        }
        else
            guidePanel.SetActive(false);
    }

    public void NextGuideFlow()
    {
        triggerPos.AllTogether = false;
        triggerPos.inSideCount = 0;
        currentMission++;
        CmdGuideFlow(currentMission);
        //RpcGuideFlow(currentMission);
        //LocalGuideFlow(currentMission);
    }
    [Command(requiresAuthority = false)]
    private void CmdGuideFlow(int _idx)
    {
        if (cmdActiveOnlyOne)
            return;
        cmdActiveOnlyOne = true;
        LocalGuideFlow(currentMission);
        RpcGuideFlow(_idx);
        StartCoroutine(nameof(ResetCmdActive));
    }
    [ClientRpc]
    private void RpcGuideFlow(int _idx)
    {
        LocalGuideFlow(_idx);
    }
    private void LocalGuideFlow(int _idx)
    {
        currentMission = _idx;
        guideEffect.SetActive(false);
        currGuideLine = camGuideLines[_idx].position;
        camOrigin = camParent.transform.position;
        camParent.transform.rotation = Quaternion.identity;
        UpdateGuideText(_idx);
        guideEffect.transform.position = effectGuideLines[_idx].position;
        onTriggerPos = true;
        SwitchOnTriggerPos(_idx);
        camParent.boundaryCollider = cameraBoundarys[_idx];
        EnableEnemy(_idx);
        if (cor != null)
            StopCoroutine(cor);
        cor = StartCoroutine(nameof(CameraMove));
    }
    private void EnableEnemy(int _idx)
    {
        for (int i = 0; i < EnemySet.Count; i++)
        {
            EnemySet[i].SetActive(false);
        }
        EnemySet[_idx].SetActive(true);
    }

    private IEnumerator ResetCmdActive()
    {
        yield return waitThree;
        cmdActiveOnlyOne = false;
    }

    private IEnumerator CameraMove()
    {
        float _t = 0f;
        while (_t <= 1f)
        {
            _t += Time.deltaTime;
            camParent.transform.position = Vector3.Lerp(camParent.transform.position, currGuideLine, _t);
            yield return null;
        }
        camParent.transform.position = currGuideLine;
        guideEffect.SetActive(true);
        yield return waitThree;
        _t = 0f;
        while (_t <= 1f)
        {
            _t += Time.deltaTime;
            camParent.transform.position = Vector3.Lerp(camParent.transform.position, camOrigin, _t);
            yield return null;
        }
        camParent.transform.position = camOrigin;
    }
    private void UpdateGuideText(int _idx)
    {
        guideText.text = guideTexts[_idx];
    }
    private void SwitchOnTriggerPos(int _idx)
    {
        switch (_idx)
        {
            case 0:
            case 1:
            case 3:
            case 5:
                onTriggerPos = true; //모이는 임무
                break;
            case 2:
            case 4:
                onTriggerPos = false; //죽이는 임무
                targetEnemyIndex++;
                break;
        }
    }

    private void GameWin()
    {
        tacticsManager.MissionClear = true;
        tacticsManager.ISGamePlay = false;
        tacticsManager.RpcDisplayEnding();
        guidePanel.SetActive(false);
    }
}
