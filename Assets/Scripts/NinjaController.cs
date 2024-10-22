using HighlightPlus;
using Mirror;
using StarterAssets;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
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
    //[SyncVar(hook = nameof(DeleteBody))]
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
    public Skill[] skillSet = new Skill[3];
    private Skill selectedSkill;
    private int terrainLayer;
    //public Vector3 targetPos;
    //마우스 클릭
    private float doubleClickTimeLimit = 0.3f;
    private float lastClickTime = 0f;
    [Header("Components")]
    public MiniMapComponent miniMapComponent;
    [Header("Script cashing")]
    public HighlightEffect highlightEffect;
    public NavMeshAgent agent;

    private Transform carriedCorpse = null;  // 현재 들고 있는 시체
    private bool isCarryingCorpse = false;
    [SerializeField] private Transform enemyBox;
    public int killCount;


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
        terrainLayer = LayerMask.GetMask("Terrain");
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeAnimCon()
    {
        LocalChangeAnimCon();
        RpcChangeAnimCon();
    }
    [ClientRpc]
    private void RpcChangeAnimCon()
    {
        LocalChangeAnimCon();
    }
    private void LocalChangeAnimCon()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
        RuntimeAnimatorController currCon = anim.runtimeAnimatorController;
        anim.runtimeAnimatorController = ChangeAnimCon;
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
        PickUpItem();
        float speed = agent.velocity.magnitude;
        anim.SetFloat("MoveSpeed", speed);
        if (speed < 0.1) anim.SetBool("Running", false);
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
        tacticsManager.CmdAddNinjaCon(tacticsManager.localPlayerNum, this);
        miniMapComponent.enabled = true;
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

    private void GetMousePosforMove() //이동시 먼저 발동
    {
        if (Input.GetMouseButtonDown(1))
        {
            AlreadyUseSkill();
            StopAllCoroutines();
            if (isSitting)
            {
                ninjaSpeed = ninjaSitSpeed;
            }
            else
            {
                ninjaSpeed = ninjaStandSpeed;
            }
            if (Time.time - lastClickTime < doubleClickTimeLimit) // 더블 클릭 감지 (뛰기)
            {
                ninjaSpeed = ninjaRunSpeed;
                anim.SetBool("Running", true);
                if (isSitting)
                {
                    isSitting = false;
                    anim.SetBool("Sitting", false);
                }
                if (isCarryingCorpse) { StopCarryingCorpse(); }
            }
            lastClickTime = Time.time;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
                Moveto(hit.point);
        }
    }


    private void SwitchNinjaSit()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //StopCoroutine(moveCor);
            AlreadyUseSkill();
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
            case 0: // 닌자
                maxHP = 3;
                curHP = 3;
                ninjaRunSpeed = 9;
                ninjaStandSpeed = 6;
                ninjaSitSpeed = 3;
                break;
            case 1: // 쿠노이치
                maxHP = 3;
                curHP = 3;
                ninjaRunSpeed = 9;
                ninjaStandSpeed = 5;
                ninjaSitSpeed = 4;
                break;
            case 2: // 사무라이
                maxHP = 5;
                curHP = 5;
                ninjaRunSpeed = 8;
                ninjaStandSpeed = 5;
                ninjaSitSpeed = 3;
                break;
        }
    }

    #region 이동관련
    public void Moveto(Vector3 _targetPos)
    {
        //StopCoroutine(moveCor);
        AlreadyUseSkill();
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
        if (isSitting) //멈출때 애니메이션 바뀌기
            anim.SetTrigger("Sit");
        //else
        //anim.SetTrigger("Idle");
    }
    #endregion

    #region 스킬ASD
    private void KeyDownASD()
    {
        if (Input.GetKeyDown(KeyCode.A))
            ReadySkill(0);
        else if (Input.GetKeyDown(KeyCode.S))
            ReadySkill(1);
        else if (Input.GetKeyDown(KeyCode.D))
            ReadySkill(2);
    }
    public void ReadySkill(int skillIdx)
    {
        selectedSkill = skillSet[skillIdx];
        bool isUnavailable = !selectedSkill.IsOffCooldown() ||
                            (ninjaType == 0 && selectedSkill == skillSet[1]) && SkillManager.instance.lostShuriken ||
                            (ninjaType == 1 && selectedSkill == skillSet[1]) && SkillManager.instance.lostKimono ||
                            (ninjaType == 2 && selectedSkill == skillSet[2]) && SkillManager.instance.lostSakke;
        if (isUnavailable)
        {
            return;
        }
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                Vector3 worldPos = hit.point;
                soundIndicator.transform.position = new Vector3(worldPos.x, worldPos.y + 0.15f, worldPos.z);
            }
        }
    }

    public void AlreadyUseSkill()
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
        anim.SetTrigger("Hit");
        if (ninjaType == 1) SoundManager.instance.PlaySE("피격 2");
        else SoundManager.instance.PlaySE("피격 1");
        curHP -= 1;
        localHpImg.fillAmount = curHP / (float)maxHP;
        tacticsManager.CmdUpdateHP(tacticsManager.localPlayerNum, curHP);

        if (curHP <= 0)
        {
            print("죽음 못 움직임");
            anim.SetTrigger("Die");
            isDie = true;
            tacticsManager.CmdCheackAllDeath();
            tacticsManager.CmdCountAlivePlayers();
        }
    }

    public void ChangeAnim(string _trigger)
    {
        anim.SetTrigger(_trigger);
    }
    public void ChangeAnimBool(string _bool, bool _flag)
    {
        anim.SetBool (_bool, _flag);
    }

    //void DeleteBody(bool _Old, bool _New)
    //{
    //    isDie = true;
    //    Destroy(gameObject, 5f); //5초후 시체가 사라짐
    //}

    private void Teleport()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GHTeleportLocation teleportLocation = hit.transform.GetComponent<GHTeleportLocation>();
                if (teleportLocation != null)
                {
                    agent.SetDestination(hit.point);
                    StartCoroutine(CheckAndTeleport(teleportLocation));
                }
                else
                {
                    Debug.LogWarning("TeleportLocation 스크립트가 이 오브젝트에 없습니다.");
                }
            }
        }
    }

    private IEnumerator CheckAndTeleport(GHTeleportLocation teleportLocation)
    {
        while (agent.pathPending)
        {
            yield return null;
        }

        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
        {
            teleportLocation.Teleport(transform);
        }
    }
    private IEnumerator CheckArrive(Transform transform)
    {
        while (agent.pathPending)
        {
            yield return null;
        }

        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
        {
            StartCarryingCorpse(transform);
        }
    }

    private void MovingCorpse()
    {
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.gameObject.CompareTag("Corpse"))
                {
                    SoundManager.instance.PlaySE("픽업");
                    if (carriedCorpse == null)  // 시체를 들고 있지 않다면
                    {
                        agent.SetDestination(hit.transform.position);
                        StartCoroutine(CheckArrive(hit.transform));
                    }
                    else
                    {
                        StopCarryingCorpse();
                    }
                }
            }
        }
        if (carriedCorpse != null && Input.GetKeyDown(KeyCode.Space))
        {
            StopCarryingCorpse();
        }
    }
    private void StartCarryingCorpse(Transform corpse)
    {
        if (!isCarryingCorpse)  // 현재 시체를 들고 있지 않을 때만 실행
        {
            isCarryingCorpse = true;
            anim.SetBool("Carrying", true);  // 애니메이션 상태 변경
            Enemy enemy = corpse.GetComponent<Enemy>();
            if (enemy != null) { enemy.anim.SetBool("Carried", true); }
            carriedCorpse = corpse;
            carriedCorpse.position = transform.position;  // 플레이어 위에 시체를 배치
            carriedCorpse.parent = transform;  // 플레이어의 자식으로 설정하여 같이 이동
        }
    }

    private void StopCarryingCorpse()
    {
        if (isCarryingCorpse)  // 현재 시체를 들고 있을 때만 실행
        {
            isCarryingCorpse = false;
            anim.SetBool("Carrying", false);  // 애니메이션 상태 변경
            Enemy enemy = carriedCorpse.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.anim.SetBool("Carried", false);
                enemy.fieldOfView.isViewMeshVisible = false;
            }
            carriedCorpse.parent = enemyBox;
            carriedCorpse.position = transform.position;  // 플레이어 위치에 내려놓기
            carriedCorpse = null;  // 들고 있는 시체 초기화
        }
    }

    private void PickUpItem()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.7f);

        foreach (var collider in hitColliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Item"))
            {
                string itemTag = collider.tag;
                bool check = false;
                switch (ninjaType)
                {
                    case 0:
                        if (itemTag == "Shuriken")
                            check = true;
                        break;
                    case 1:
                        if (itemTag == "Kimono")
                            check = true;
                        break;
                    case 2:
                        Enemy enemy = collider.GetComponent<Enemy>();
                        if (enemy == null && itemTag == "Sakke")
                            check = true;
                        break;
                }
                if (!check)
                    break;
                if (SkillManager.instance.itemActions.TryGetValue(itemTag, out Action itemAction))
                {
                    SoundManager.instance.PlaySE("픽업");
                    Debug.Log($"{itemTag} 획득");
                    itemAction.Invoke();
                    collider.gameObject.layer = 0;
                    Destroy(collider.gameObject);
                    SkillManager.instance.skillIcons[1].color = Color.white;
                    break;
                }
            }
        }
    }
}

