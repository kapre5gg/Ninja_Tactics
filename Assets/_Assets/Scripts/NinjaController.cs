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
    [SerializeField] private int ninjaSpeed = 0;
    private int ninjaRunSpeed = 8;
    private int ninjaStandSpeed = 6;
    private int ninjaSitSpeed = 3;
    public Image localHpImg = null;
    public Image multiHpImg = null;

    [Header("skill")]
    private SkillManager skillManager;
    private Skill[] skillSet = new Skill[3];
    [SerializeField] private bool isAattack;
    [SerializeField] private bool isSattack;
    [SerializeField] private bool isDattack;
    public GameObject target;
    public Vector3 targetPos;
    public bool isClose = false; //스킬을 쓰고 스킬 범위만큼 충분히 다가갔을때 true를 보냄
    //마우스 클릭
    private float doubleClickTimeLimit = 0.3f;
    private float lastClickTime = 0f;

    [Header("cashing")]
    public HighlightEffect highlightEffect;
    public NavMeshAgent agent;

    public override void OnStartLocalPlayer()
    {
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
        //AlawysHighlight();
        GetMousePos();
        SwitchNinjaSit();
        Isattack();
        UseSkillSet();
    }

    private void AlawysHighlight()
    {
        if (highlightEffect != null)
            highlightEffect.highlighted = true;
    }

    public void StartNinja()
    {
        if (!isLocalPlayer)
            return;
        GetComponent<ThirdPersonController>().enabled = false;
        GetComponent<PlayerInput>().enabled = false;
        ninjaType = DBManager.instance.PlayerNinjaType;
        SetNinjaInfo(ninjaType);
        skillSet = skillManager.GetSkill(ninjaType);
        //NinjaTacticsManager ninjaManager = FindObjectOfType<NinjaTacticsManager>();
        //ninjaManager.multiPlayers.SetActive(true);
        //ninjaManager.map.SetActive(true);
        //ninjaManager.skillInfo.SetActive(true);
        //if (ninjaType == 2) //사무라이만 체력이 5칸
        //{
        //    ninjaManager.profill[1].SetActive(true);
        //    hpImage = ninjaManager.profill[1].GetComponent<Image>();
        //    //Instantiate(Resources.Load<GameObject>("MultiProfill5"), multiPlayers.transform); //멀티작업해야됨
        //    //Instantiate(Resources.Load<GameObject>("MultiProfill" + 5.ToString()), multiPlayers.transform);
        //    ninjaManager.CmdMakeProfill(5);
        //}
        //else
        //{
        //    ninjaManager.profill[0].SetActive(true);
        //    hpImage = ninjaManager.profill[0].GetComponent<Image>();
        //    //Instantiate(Resources.Load<GameObject>("MultiProfill3"), multiPlayers.transform);
        //    ninjaManager.CmdMakeProfill(3);
        //}
    }

    private void GetMousePos() //이동시 먼저 발동
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAattack = false; //이동하면 진행중인 스킬 다 무시
            isSattack = false;
            isDattack = false;
            StopAllCoroutines();
            if (isSitting)
                anim.SetTrigger("SitWalk");
            else
                anim.SetTrigger("Walk");
            if (Time.time - lastClickTime < doubleClickTimeLimit) // 더블 클릭 감지
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
        StopAllCoroutines();
        moveCor = StartCoroutine(MouseMove(_targetPos, _speed));
    }

    public void Chaseto(GameObject _target, float _range)
    {
        //StopCoroutine(moveCor);
        StopAllCoroutines();
        moveCor = StartCoroutine(ChaseMove(_target, _range));
    }
    private IEnumerator ChaseMove(GameObject _target, float _range)
    {
        isClose = false;
        if (isSitting)
            anim.SetTrigger("SitWalk");
        else
            anim.SetTrigger("Run");
        agent.stoppingDistance = _range;
        while (Vector3.Distance(transform.position, _target.transform.position) > skillSet[0].skillRange)
        {
            agent.speed = ninjaSpeed;
            agent.SetDestination(_target.transform.position);
            yield return null;
        }
        print("공격체크");
        isClose = true;
        anim.SetTrigger("Idle");
        //skillset[0].UseSKill(_target);
        //skillset[0].MakeSound(transform.position);
        //skillset[0].MakeSoundEffect(transform.position);
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
    private void Isattack()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            isAattack = true;
            isSattack = false;
            isDattack = false;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            isAattack = false;
            isSattack = true;
            isDattack = false;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            isAattack = false;
            isSattack = false;
            isDattack = true;
        }
    }

    public void WaitforChase(int _skillset, Vector3 _pos)
    {
        WaitforChaseCor(_skillset, _pos);
    }
    public IEnumerator WaitforChaseCor(int _skillset, Vector3 _pos)
    {
        while (!isClose)
        {
            yield return null;
        }
        DBManager.instance.myCon.ChangeAnim("Attack");
        skillSet[_skillset].SkillEffect(_pos);
        skillSet[_skillset].MakeSound(_pos);
    }

    private void UseSkillSet()
    {
        if (isAattack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GetMouseTarget();
                skillSet[0].UseSKill();
            }
        }
        else if (isSattack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GetMouseTarget();
                skillSet[1].UseSKill();
            }
        }
        else if (isDattack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GetMouseTarget();
                skillSet[2].UseSKill();
            }
        }
    }

    private void GetMouseTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.CompareTag("Enemy"))
            {
                target = hit.collider.gameObject;
                targetPos = hit.collider.transform.position;
            }
            else
            {
                target = null;
                targetPos = hit.point;
            }
        }
    }

    #endregion

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
            isDie = true;
        }
    }

    public void ChangeAnim(string _trigger)
    {
        anim.SetTrigger(_trigger);
    }
}
