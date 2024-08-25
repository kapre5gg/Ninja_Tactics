using HighlightPlus;
using Mirror;
using Mysqlx.Crud;
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
    public int curHP;
    private int maxHP;
    public RuntimeAnimatorController ChangeAnimCon;
    private Animator anim;
    [SyncVar(hook = nameof(DeleteBody))]
    public bool isDie = false;
    //private Coroutine moveCor;
    private NinjaTacticsManager tacticsManager;
    private int ninjaSpeed = 0;
    private int ninjaRunSpeed = 8;
    private int ninjaStandSpeed = 6;
    private int ninjaSitSpeed = 3;
    [HideInInspector] public Image localHpImg = null;
    [HideInInspector] public Image multiHpImg = null;
    public Image playerclass;
    [Header("skill")]
    public GameObject skillIndicatorPrefab;
    [HideInInspector] public GameObject soundIndicator;
    public Vector3 nonTargetPos;
    public Transform target;
    public bool veiwSoundIndicator;
    private Skill[] skillSet = new Skill[3];
    private Skill selectedSkill;
    //public Vector3 targetPos;
    //���콺 Ŭ��
    private float doubleClickTimeLimit = 0.3f;
    private float lastClickTime = 0f;

    [Header("cashing")]
    public HighlightEffect highlightEffect;
    public NavMeshAgent agent;

    private Transform carriedCorpse = null;  // ���� ��� �ִ� ��ü
    private bool isCarryingCorpse = false;

    public override void OnStartLocalPlayer()
    {
        playerclass.gameObject.SetActive(false);
        skillIndicatorPrefab.SetActive(false);
        DBManager.instance.myCon = this;
        isDie = false;
        ninjaType = -1;
        agent = GetComponent<NavMeshAgent>();
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
        anim.runtimeAnimatorController = ChangeAnimCon; //����Ʈ �ִϸ��̼ǵ� �ٲ? �ٲ�
        ChangeAnimCon = currCon;
    }

    private void Update()
    {
        if (tacticsManager == null)
            return;
        if (isDie || !tacticsManager.ISGamePlay)
            return;
        GetMousePosforMove();
        SwitchNinjaSit();
        KeyDownASD();
        UseSkillSet();
        Teleport();
        MovingCorpse();
        float speed = agent.velocity.magnitude;
        anim.SetFloat("MoveSpeed", speed);
        if(speed < 0.1) anim.SetBool("Running", false);
    }

    public void StartNinja()
    {
        if (!isLocalPlayer)
            return;
        GetComponent<ThirdPersonController>().enabled = false;
        GetComponent<PlayerInput>().enabled = false;
        ninjaType = DBManager.instance.PlayerNinjaType;
        SetNinjaInfo(ninjaType);
        skillSet = SkillManager.instance.GetSkill(ninjaType);
        CmdChangNinjaSprite(ninjaType);
    }

    [Command]
    public void CmdChangNinjaSprite(int _type)
    {
        RpcChangNinjaSprite(_type);
    }
    [ClientRpc]
    private void RpcChangNinjaSprite(int _type)
    {
        playerclass.sprite = Resources.Load<Sprite>($"NinjaIcon{_type}");
        playerclass.gameObject.SetActive(true);
    }

    private void GetMousePosforMove() //�̵��� ���� �ߵ�
    {
        if (Input.GetMouseButtonDown(1))
        {
            NotUseSkill();
            StopAllCoroutines();
            if (isSitting)
            {
                ninjaSpeed = ninjaSitSpeed;
            }
            else
            {
                ninjaSpeed = ninjaStandSpeed;
            }
            if (Time.time - lastClickTime < doubleClickTimeLimit) // ���� Ŭ�� ���� (�ٱ�)
            {
                if(!isSitting && !isCarryingCorpse)
                {
                    ninjaSpeed = ninjaRunSpeed;
                    anim.SetBool("Running", true);
                }
            }
            lastClickTime = Time.time;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                Moveto(hit.point);
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
            agent.speed = ninjaSitSpeed;
            isSitting = !isSitting;
            if (isSitting)
                anim.SetBool("Sitting", true);
            else
                anim.SetBool("Sitting", false);
        }
    }

    private void SetNinjaInfo(int _type)
    {
        isSitting = false;
        switch (ninjaType)
        {
            case 0: // ����
                maxHP = 3;
                curHP = 3;
                ninjaRunSpeed = 9;
                ninjaStandSpeed = 6;
                ninjaSitSpeed = 3;
                break;
            case 1: // �����ġ
                maxHP = 3;
                curHP = 3;
                ninjaRunSpeed = 9;
                ninjaStandSpeed = 5;
                ninjaSitSpeed = 4;
                break;
            case 2: // �繫����
                maxHP = 5;
                curHP = 5;
                ninjaRunSpeed = 8;
                ninjaStandSpeed = 5;
                ninjaSitSpeed = 3;
                break;
        }
    }

    #region �̵�����
    public void Moveto(Vector3 _targetPos)
    {
        //StopCoroutine(moveCor);
        NotUseSkill();
        StopAllCoroutines();
        agent.SetDestination(_targetPos);
        agent.speed = ninjaSpeed;
        StartCoroutine(MoveEnd(_targetPos));
    }
    private IEnumerator MoveEnd(Vector3 _targetPos)
    {
        while (Vector3.Distance(transform.position, _targetPos) >= 0.1f)
        {
            yield return null;
        }
        if (isSitting) //���⶧ �ִϸ��̼� �ٲ��
            anim.SetTrigger("Sit");
        //else
            //anim.SetTrigger("Idle");
    }
    #endregion

    #region ��ųASD
    private void KeyDownASD()
    {
        if (Input.GetKeyDown(KeyCode.A))
            ReadySkill(0);
        else if (Input.GetKeyDown(KeyCode.S))
            ReadySkill(1);
        else if (Input.GetKeyDown(KeyCode.D))
            ReadySkill(2);
    }
    private void ReadySkill(int skillIdx)
    {
        selectedSkill = skillSet[skillIdx];
        skillIndicatorPrefab.transform.localScale = Vector3.one * skillSet[skillIdx].skillRange;
        skillIndicatorPrefab.SetActive(true);
        ViewSoundRange(true, skillSet[skillIdx].soundRange);
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
        if (soundIndicator == null)
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

    public void NotUseSkill()
    {
        selectedSkill = null;
        skillIndicatorPrefab.SetActive(false);
        ViewSoundRange(false, 1f);
    }
    #endregion

    public void OnDamage()
    {
        if (isDie)
            return;
        curHP -= 1;
        localHpImg.fillAmount = curHP / (float)maxHP;
        tacticsManager.CmdUpdateHP(tacticsManager.localPlayerNum, curHP);

        if (curHP <= 0)
        {
            print("���� �� ������");
            anim.SetTrigger("Die");
            isDie = true;
            tacticsManager.CmdCheackAllDeath();
        }
    }

    public void ChangeAnim(string _trigger)
    {
        anim.SetTrigger(_trigger);
    }

    void DeleteBody(bool _Old, bool _New)
    {
        isDie = true;
        Destroy(gameObject, 5f); //5���� ��ü�� �����
    }

    private void Teleport()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TeleportLocation teleportLocation = hit.transform.GetComponent<TeleportLocation>();
                if (teleportLocation != null)
                {
                    agent.SetDestination(hit.point); // Set the agent destination to the hit point
                    StartCoroutine(CheckAndTeleport(teleportLocation));
                }
                else
                {
                    Debug.LogWarning("TeleportLocation ��ũ��Ʈ�� �� ������Ʈ�� �����ϴ�.");
                }
            }
        }
    }

    private IEnumerator CheckAndTeleport(TeleportLocation teleportLocation)
    {
        // Wait until the agent has finished calculating the path
        while (agent.pathPending)
        {
            yield return null;
        }

        // Wait until the agent reaches the destination
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // Once reached, check if there is no remaining path and the agent has stopped moving
        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
        {
            agent.enabled = false;
            teleportLocation.Teleport(transform);
            agent.enabled = true;
        }
    }
    private void MovingCorpse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.gameObject.CompareTag("Corpse"))
                {
                    if (carriedCorpse == null)  // ��ü�� ��� ���� �ʴٸ�
                    {
                        StartCarryingCorpse(hit.transform);
                    }
                    else
                    {
                        StopCarryingCorpse();
                    }
                }
            }
        }
    }
    private void StartCarryingCorpse(Transform corpse)
    {
        if (!isCarryingCorpse)  // ���� ��ü�� ��� ���� ���� ���� ����
        {
            isCarryingCorpse = true;
            anim.SetBool("Carrying", true);  // �ִϸ��̼� ���� ����
            Enemy enemy = corpse.GetComponent<Enemy>();
            if (enemy != null) { enemy.anim.SetBool("Carried", true); }
            carriedCorpse = corpse;
            carriedCorpse.position = transform.position;  // �÷��̾� ���� ��ü�� ��ġ
            carriedCorpse.parent = transform;  // �÷��̾��� �ڽ����� �����Ͽ� ���� �̵�
        }
    }

    private void StopCarryingCorpse()
    {
        if (isCarryingCorpse)  // ���� ��ü�� ��� ���� ���� ����
        {
            isCarryingCorpse = false;
            anim.SetBool("Carrying", false);  // �ִϸ��̼� ���� ����
            Enemy enemy = carriedCorpse.GetComponent<Enemy>();
            if (enemy != null) { enemy.anim.SetBool("Carried", false); }
            carriedCorpse.parent = null;  // �θ� ���� ����
            carriedCorpse.position = transform.position;  // �÷��̾� ��ġ�� ��������
            carriedCorpse = null;  // ��� �ִ� ��ü �ʱ�ȭ
        }
    }
}

