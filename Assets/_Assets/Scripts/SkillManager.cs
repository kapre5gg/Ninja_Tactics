using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public abstract class Skill
{
    [Header("skill info")]
    public float skillRange;
    public float soundRange;
    public float skillCool;
    protected float lastUsedTime;
    protected NinjaController caster;
    protected Transform casterTr;

    public Skill(float skillRange, float soundRange, float skillCool)
    {
        this.skillRange = skillRange;
        this.soundRange = soundRange;
        this.skillCool = skillCool;
        this.lastUsedTime = -skillCool; // 처음부터 스킬을 사용할 수 있도록 설정

        caster = DBManager.instance.myCon;
        casterTr = caster.transform;
    }

    public bool IsTargetInRange(Transform target)
    {
        return Vector3.Distance(casterTr.position, target.position) <= skillRange;
    }
    public bool IsTargetInRange(Vector3 nonTarget)
    {
        return Vector3.Distance(casterTr.position, nonTarget) <= skillRange;
    }
    public bool IsTargetSkill()
    {
        return DBManager.instance.myCon.target != null;
    }
    public bool IsOffCooldown()
    {
        return Time.time >= lastUsedTime + skillCool;
    }

    public abstract void UseSkill();
    public abstract void ApproachUseSkill();


    protected IEnumerator ApproachUseSkillCor()
    {
        yield return null;
        NinjaController casterCon = DBManager.instance.myCon;
        if (casterCon.isSitting)
            casterCon.ChangeAnim("SitWalk");
        else
            casterCon.ChangeAnim("Walk");
        if (IsTargetSkill())
        {
            while (!IsTargetInRange(casterCon.target))
            {
                casterCon.agent.SetDestination(casterCon.target.position);
                casterCon.soundIndicator.transform.position = casterCon.target.position;
                yield return null;
            }
        }
        else
        {
            casterCon.agent.SetDestination(casterCon.nonTargetPos);
            casterCon.soundIndicator.transform.position = casterCon.nonTargetPos;
            while (!IsTargetInRange(casterCon.nonTargetPos))
            {
                yield return null;
            }
        }
        UseSkill();

        lastUsedTime = Time.time;
    }
}

public class MeleeAttack : Skill
{
    public MeleeAttack(float skillRange, float soundRange, float skillCool)
        : base(skillRange, soundRange, skillCool) { }

    public override void UseSkill()
    {
        caster.agent.SetDestination(casterTr.position);
        caster.ChangeAnim("Attack");
        SkillManager.instance.MakeSound(casterTr.position, soundRange);
        caster.target.GetComponent<EnemyAI>().Die();
        caster.skillIndicatorPrefab.SetActive(false);
        SkillManager.instance.SkillCool(0);
        caster.NotUseSkill();
    }

    public override void ApproachUseSkill()
    {
        if (IsOffCooldown() && IsTargetSkill())
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("스킬 쿨 안돌아옴");
    }
}

public class Shurican : Skill
{
    public Shurican(float skillRange, float soundRange, float skillCool)
        : base(skillRange, soundRange, skillCool) { }

    public override void UseSkill()
    {
        caster.agent.SetDestination(casterTr.position);
        caster.ChangeAnim("Throw");
        SkillManager.instance.MakeSound(caster.target.position, soundRange);
        caster.target.GetComponent<EnemyAI>().Die();
        SkillManager.instance.SkillCool(1);
        caster.NotUseSkill();
    }

    public override void ApproachUseSkill()
    {
        if (IsOffCooldown() && IsTargetSkill())
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("스킬 쿨 안돌아옴");
    }
}

public class ThrowSomething : Skill
{
    private int type = -1;
    // _type == 0 == stone / 1 == sand / 2 == sakke / 3 == bomb / 4 == null

    public ThrowSomething(float skillRange, float soundRange, float skillCool, int _type)
        : base(skillRange, soundRange, skillCool) { type = _type; }

    public override void UseSkill()
    {
        caster.agent.SetDestination(casterTr.position);
        caster.ChangeAnim("Throw");
        SkillManager.instance.MakeSound(caster.nonTargetPos, soundRange);
        SkillManager.instance.SkillCool(2);
        caster.GetComponent<MonoBehaviour>().StartCoroutine(WaitforAct());
        caster.NotUseSkill();
    }
    private IEnumerator WaitforAct()
    {
        yield return new WaitForSeconds(0.6f);
        Collider[] colls = Physics.OverlapSphere(caster.nonTargetPos, soundRange);
        foreach (Collider coll in colls)
        {
            EnemyAI enemyAI = coll.GetComponent<EnemyAI>();
            if (enemyAI == null)
                yield break;
            switch (type)
            {
                case 0:
                    Debug.Log("돌던짐");
                    //target.GetComponent <EnemyAI>().Alarm(0);

                    break;
                case 1:
                    Debug.Log("모래던짐");
                    //target.GetComponent <EnemyAI>().Alarm();
                    break;
                case 2:
                    if (SkillManager.instance.lostSakke)
                        yield break;
                    Debug.Log("술던짐");
                    skillCool = 999999f;
                    //target.GetComponent <EnemyAI>().Alarm();
                    break;
                default:
                    break;
            }
        }
    }

    public override void ApproachUseSkill()
    {
        if (IsOffCooldown())
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("스킬 쿨 안돌아옴");
    }

}

public class SlashBlade : Skill
{
    public SlashBlade(float skillRange, float soundRange, float skillCool)
        : base(skillRange, soundRange, skillCool) { }

    public override void UseSkill()
    {
        caster.agent.SetDestination(casterTr.position);
        caster.ChangeAnim("Slash");
        SkillManager.instance.MakeSound(caster.nonTargetPos, soundRange);
        Collider[] colls = Physics.OverlapSphere(casterTr.position, soundRange);
        foreach (Collider coll in colls)
        {
            EnemyAI enemyAI = coll.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.Die();
            }
        }
        SkillManager.instance.SkillCool(1);
        caster.NotUseSkill();
    }
    public override void ApproachUseSkill()
    {
        if (IsOffCooldown())
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("스킬 쿨 안돌아옴");
    }
}

public class TalktoEnemy : Skill
{
    public TalktoEnemy(float skillRange, float soundRange, float skillCool)
        : base(skillRange, soundRange, skillCool) { }

    public override void UseSkill()
    {
        caster.agent.SetDestination(casterTr.position);
        caster.ChangeAnim("Idle");
        //talk
        SkillManager.instance.SkillCool(1);
        caster.NotUseSkill();
    }
    public override void ApproachUseSkill()
    {
        if (IsOffCooldown() && IsTargetSkill() && SkillManager.instance.lostKimono)
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("스킬 쿨 안돌아옴");
    }
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
        }
        else
            Destroy(this);
    }
    public Image[] skillIcons = new Image[3];
    private NinjaController ninjaCon = null;
    //public Terrain terrain;
    public Skill[] skillSet = new Skill[3];
    public bool lostSakke = false;
    public bool lostKimono = false;
    public Skill[] GetSkill(int _type)
    {
        ninjaCon = DBManager.instance.myCon;
        switch (_type)
        {
            case 0:
                skillSet[0] = new MeleeAttack(1.2f, 3f, 1.3f);
                skillSet[1] = new Shurican(6f, 6f, 4f);
                skillSet[2] = new ThrowSomething(6f, 5f, 6f, 0);
                SKillIconSet(0);
                break;
            case 1:
                skillSet[0] = new MeleeAttack(1.2f, 3f, 1.3f);
                skillSet[1] = new TalktoEnemy(1.2f, 0f, 4f);
                skillSet[2] = new ThrowSomething(6f, 3f, 2.5f, 1);
                SKillIconSet(1);
                break;
            case 2:
                skillSet[0] = new MeleeAttack(1.2f, 3f, 1.3f);
                skillSet[1] = new SlashBlade(2f, 3f, 18f);
                skillSet[2] = new ThrowSomething(6f, 0f, 999999f, 2);
                SKillIconSet(2);
                break;
            default:
                print("잘못된 스킬 타입입니다.");
                break;
        }
        return skillSet;
    }

    private void SKillIconSet(int _type)
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            skillIcons[i].sprite = Resources.Load<Sprite>($"Skill_Icon{_type}_{i}");
            skillIcons[i].fillAmount = 1;
        }
    }

    public void SkillCool(int _skillNum)
    {
        skillIcons[_skillNum].fillAmount = 0;
        StartCoroutine(FillCool(_skillNum));
    }
    private IEnumerator FillCool(int _skillNum)
    {
        while (skillIcons[_skillNum].fillAmount <= 1)
        {
            skillIcons[_skillNum].fillAmount += Time.deltaTime / skillSet[_skillNum].skillCool;
            yield return null;
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

    public void HasSakke()
    {
        lostSakke = true;
        skillIcons[2].fillAmount = 1;
        skillSet[2].skillCool = 0;
    }
    public void HasKimono()
    {
        lostKimono = true;
        skillIcons[2].fillAmount = 1;
    }
}
