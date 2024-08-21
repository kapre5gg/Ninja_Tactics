using HighlightPlus;
using Mirror;
using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NinjaController : NetworkBehaviour
{
    [Header("ninja info")]
    public string ninjaName;
    public int ninjaType = -1;
    public bool isSitting = false;
    public int HP;
    public RuntimeAnimatorController ChangeAnimCon;
    private Animator anim;
    [SyncVar]
    public bool isDie = false;
    private Coroutine moveCor;
    private NinjaTacticsManager tacticsManager;
    private int ninjaSpeed = 0;
    private int ninjaRunSpeed = 8;
    private int ninjaStandSpeed = 6;
    private int ninjaSitSpeed = 3;
    public Image localHpImg = null;
    public Image multiHpImg = null;

    [Header("skill")]
    public GameObject skillIndicatorPrefab;
    [HideInInspector] public GameObject soundIndicator;
    public Vector3 nonTargetPos;
    public Transform target;
    private SkillManager skillManager;
    public bool veiwSoundIndicator;
    private Skill[] skillSet = new Skill[3];
    private Skill selectedSkill;
    //public Vector3 targetPos;
    //마우스 클릭
    private float doubleClickTimeLimit = 0.3f;
    private float lastClickTime = 0f;

    [Header("cashing")]
    public HighlightEffect highlightEffect;
    public NavMeshAgent agent;

    public override void OnStartLocalPlayer()
    {
        skillIndicatorPrefab.SetActive(false);
        DBManager.instance.myCon = this;
        isDie = false;
        ninjaType = -1;
        agent = GetComponent<NavMeshAgent>();
        skillManager = FindObjectOfType<SkillManager>();
        //highlightEffect = GetComponent<HighlightEffect>();
        tacticsManager = FindObjectOfType<NinjaTacticsManager>();
        anim = GetComponent<Animator>();
        //StartNinja();
        agent.enabled = false;
        this.enabled = false;
    }
    public void ChangeAnimatorCon()
    {
        RuntimeAnimatorController currCon = anim.runtimeAnimatorController;
        anim.runtimeAnimatorController = ChangeAnimCon; //리모트 애니메이션도 바뀌나? 바뀜
        ChangeAnimCon = currCon;
    }

    private void Update()
    {
        if (isDie)
            return;
        GetMousePos();
        SwitchNinjaSit();
        KeyDownASD();
        UseSkillSet();
    }

    public void StartNinja()
    {
        if (!isLocalPlayer)
            return;
        GetComponent<ThirdPersonController>().enabled = false;
        GetComponent<PlayerInput>().enabled = false;
        ninjaType = DBManager.instance.PlayerNinjaType;
        SetNinjaInfo(ninjaType);
        skillSet = skillManager.GetSkill(ninjaType, agent);
    }

    private void GetMousePos() //이동시 먼저 발동
    {
        if (Input.GetMouseButtonDown(1))
        {
            NotUseSkill();
            StopAllCoroutines();
            if (isSitting)
                anim.SetTrigger("SitWalk");
            else
                anim.SetTrigger("Walk");
            if (Time.time - lastClickTime < doubleClickTimeLimit) // 더블 클릭 감지 (뛰기)
            {
                isSitting = false;
                ninjaSpeed = ninjaRunSpeed;
                anim.SetTrigger("Run");
            }
            else if (!isSitting) // 서 있음
                ninjaSpeed = ninjaStandSpeed;
            else if (isSitting)
                ninjaSpeed = ninjaSitSpeed;

            lastClickTime = Time.time;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPos = hit.point;
                Moveto(targetPos, ninjaSpeed);
            }
        }
    }


    private void SwitchNinjaSit()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //StopCoroutine(moveCor);
            NotUseSkill();
            StopAllCoroutines();
            agent.SetDestination(transform.position);
            isSitting = !isSitting;
            if (isSitting)
                anim.SetTrigger("Sit");
            else
                anim.SetTrigger("Idle");
        }
    }

    private void SetNinjaInfo(int _type)
    {
        isSitting = false;
        switch (ninjaType)
        {
            case 0: // 닌자
                HP = 3;
                ninjaRunSpeed = 9;
                ninjaStandSpeed = 6;
                ninjaSitSpeed = 3;
                break;
            case 1: // 쿠노이치
                HP = 3;
                ninjaRunSpeed = 9;
                ninjaStandSpeed = 5;
                ninjaSitSpeed = 4;
                break;
            case 2: // 사무라이
                HP = 5;
                ninjaRunSpeed = 8;
                ninjaStandSpeed = 5;
                ninjaSitSpeed = 3;
                break;
        }
    }

    #region 이동관련
    public void Moveto(Vector3 _targetPos, int _speed)
    {
        //StopCoroutine(moveCor);
        NotUseSkill();
        StopAllCoroutines();
        moveCor = StartCoroutine(MouseMove(_targetPos, _speed));
    }

    private IEnumerator MouseMove(Vector3 _targetPos, int _speed)
    {
        agent.SetDestination(_targetPos);
        agent.stoppingDistance = 0;
        while (Vector3.Distance(transform.position, _targetPos) > 0.1f)
        {
            agent.speed = ninjaSpeed;
            agent.SetDestination(_targetPos);
            yield return null;
        }
        if (isSitting)
            anim.SetTrigger("Sit");
        else
            anim.SetTrigger("Idle");
    }
    #endregion

    #region 스킬ASD
    private void KeyDownASD()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            selectedSkill = skillSet[0];
            skillIndicatorPrefab.transform.localScale = Vector3.one * skillSet[0].skillRange;
            skillIndicatorPrefab.SetActive(true);
            ViewSoundRange(true, skillSet[0].soundRange);
            print("a선택");
        }

        else if (Input.GetKeyDown(KeyCode.S))
        {
            selectedSkill = skillSet[1];
            skillIndicatorPrefab.transform.localScale = Vector3.one * skillSet[1].skillRange;
            skillIndicatorPrefab.SetActive(true);
            ViewSoundRange(true, skillSet[1].soundRange);
            print("s선택");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selectedSkill = skillSet[2];
            skillIndicatorPrefab.transform.localScale = Vector3.one * skillSet[2].skillRange;
            skillIndicatorPrefab.SetActive(true);
            ViewSoundRange(true, skillSet[2].soundRange);
            print("d선택");
        }
    }

    private void UseSkillSet()
    {
        if (Input.GetMouseButtonDown(0) && selectedSkill != null)
        {
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.gameObject.CompareTag("Enemy"))
                    target = hit.transform;
                else
                    target = null;
                nonTargetPos = hit.point;
                selectedSkill.ApproachUseSkill();
                veiwSoundIndicator = false;
            }
        }
    }


    public void ViewSoundRange(bool _set, float _soundRange)
    {
        veiwSoundIndicator = _set;
        if(soundIndicator == null)
            soundIndicator = Instantiate(skillIndicatorPrefab);
        soundIndicator.SetActive(_set);
        soundIndicator.transform.localScale = Vector3.one * _soundRange;
        StartCoroutine(nameof(TraceMouse));
    }
    private IEnumerator TraceMouse()
    {
        while (veiwSoundIndicator)
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            int terrainLayer = LayerMask.GetMask("Terrain");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                Vector3 worldPos = hit.point;
                soundIndicator.transform.position = new Vector3(worldPos.x, worldPos.y + 0.05f, worldPos.z);
            }
        }
    }

    public void MakeSound(Vector3 _pos, float _soundRange)
    {
        Debug.Log("소리남");
        Collider[] colls = Physics.OverlapSphere(_pos, _soundRange);
        foreach (Collider coll in colls)
        {
            EnemyAI enemyAI = coll.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.Alarm();
            }
        }

        Vector3 newPos = new Vector3(_pos.x, 0.01f, _pos.z);
        GameObject sound = Instantiate(Resources.Load<GameObject>("Sound"), newPos, Quaternion.identity);
        sound.transform.localScale = Vector3.one * _soundRange;
        Destroy(sound, 1f);
    }

    private void NotUseSkill()
    {
        selectedSkill = null;
        skillIndicatorPrefab.SetActive(false);
        ViewSoundRange(false, 1f);
    }
    #endregion

    //귀한 작품
    public void OnDamage()
    {
        if (isDie)
            return;
        HP -= 1;
        if (ninjaType == 2)
        {
            localHpImg.fillAmount -= 0.2f;
            //multiHpImg.fillAmount -= 0.2f;
        }
        else
        {
            localHpImg.fillAmount -= 0.33f;
            //multiHpImg.fillAmount -= 0.33f;
        }

        tacticsManager.CmdUpdateHP(tacticsManager.localPlayerNum, HP);

        if (HP <= 0)
        {
            print("죽음 못움직임");
            isDie = true;
        }
    }

    public void ChangeAnim(string _trigger)
    {
        anim.SetTrigger(_trigger);
    }
}
