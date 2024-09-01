using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameGuideLine : MonoBehaviour
{
    [Header("Flow")]
    public int currentMission = 0;
    [SerializeField] private bool onTriggerPos; //지금 미션이 모이는 미션이면 true
    public List<Transform> camGuideLines = new List<Transform>();
    public List<Transform> effectGuideLines = new List<Transform>();
    public List<string> guideTexts = new List<string>();
    public List<BoxCollider> cameraBoundarys = new List<BoxCollider>();
    private Vector3 currGuideLine;
    private RaycastHit hit;
    private WaitForSeconds waitThree = new WaitForSeconds(3);
    private Vector3 camOrigin;
    private Coroutine cor;
    private bool triggerInUpdate = true;
    [Header("Components")]
    public NinjaTacticsManager tacticsManager;
    public TriggerPos triggerPos;
    public GameObject guidePanel;
    public TMP_Text guideText;
    public CameraController camParent;
    public GameObject guideEffect;

    public Enemy targetEnemy;
    public int targetEnemyIndex = -1;
    public Enemy[] targetEnemys;

    private void Update()
    {
        if(tacticsManager.ISGamePlay)
        {
            if (onTriggerPos)
            {
                targetEnemy = null;
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
    }

    public void NextGuideFlow()
    {
        triggerPos.AllTogether = false;
        triggerPos.inSideCount = 0;
        currentMission++;
        GuideFlow(currentMission);
    }

    public void GuideFlow(int _idx)
    {
        currentMission = _idx;
        guideEffect.SetActive(false);
        currGuideLine = camGuideLines[_idx].position;
        camOrigin = camParent.transform.position;
        UpdateGuideText(_idx);
        guideEffect.transform.position = effectGuideLines[_idx].position;
        SwitchOnTriggerPos(_idx);
        camParent.boundaryCollider = cameraBoundarys[_idx];
        if (cor != null)
            StopCoroutine(cor);
        cor = StartCoroutine(nameof(CameraMove));
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
