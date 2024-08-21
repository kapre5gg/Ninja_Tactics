using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[Serializable]
public abstract class Skill
{
    [Header("skill info")]
    public float skillRange;
    public float soundRange;
    public float skillCool;
    protected float lastUsedTime;
    protected Transform caster;

    public Skill(float skillRange, float soundRange, float skillCool)
    {
        this.skillRange = skillRange;
        this.soundRange = soundRange;
        this.skillCool = skillCool;
        this.lastUsedTime = -skillCool; // 처음부터 스킬을 사용할 수 있도록 설정

        this.caster = DBManager.instance.myCon.transform;
    }

    public bool IsTargetInRange(Transform target)
    {
        return Vector3.Distance(caster.position, target.position) <= skillRange;
    }
    public bool IsTargetInRange(Vector3 nonTarget)
    {
        return Vector3.Distance(caster.position, nonTarget) <= skillRange;
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
            DBManager.instance.myCon.MakeSound(casterCon.target.position, soundRange);
        }
        else
        {
            casterCon.agent.SetDestination(casterCon.nonTargetPos);
            casterCon.soundIndicator.transform.position = casterCon.nonTargetPos;
            while (!IsTargetInRange(casterCon.nonTargetPos))
            {
                yield return null;
            }
            DBManager.instance.myCon.MakeSound(casterCon.nonTargetPos, soundRange);
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
        DBManager.instance.myCon.agent.SetDestination(caster.position);
        DBManager.instance.myCon.ChangeAnim("Attack");
        DBManager.instance.myCon.target.GetComponent<EnemyAI>().Die();
        DBManager.instance.myCon.skillIndicatorPrefab.SetActive(false);
    }

    public override void ApproachUseSkill()
    {
        if (IsOffCooldown() && IsTargetSkill())
            caster.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("스킬 쿨 안돌아옴");
    }
}

public class ThrowShurican : Skill
{
    public ThrowShurican(float skillRange, float soundRange, float skillCool)
        : base(skillRange, soundRange, skillCool) { }

    public override void UseSkill()
    {
        DBManager.instance.myCon.agent.SetDestination(caster.position);
        DBManager.instance.myCon.ChangeAnim("Throw");
        DBManager.instance.myCon.target.GetComponent<EnemyAI>().Die();
    }

    public override void ApproachUseSkill()
    {
        if (IsOffCooldown() && IsTargetSkill())
            caster.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("스킬 쿨 안돌아옴");
    }
}

public class ThrowStone : Skill
{
    public ThrowStone(float skillRange, float soundRange, float skillCool)
        : base(skillRange, soundRange, skillCool) { }

    public override void UseSkill()
    {
        DBManager.instance.myCon.agent.SetDestination(caster.position);
        DBManager.instance.myCon.ChangeAnim("Throw");

        //target.GetComponent <EnemyAI>().Alarm();
    }
    public override void ApproachUseSkill()
    {
        if (IsOffCooldown())
            caster.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("스킬 쿨 안돌아옴");
    }
}

public class SkillManager : MonoBehaviour
{
    public Image[] skillIcon;
    private NinjaController ninjaCon = null;
    public Terrain terrain;

    public Skill[] GetSkill(int _type, NavMeshAgent _agent)
    {
        ninjaCon = DBManager.instance.myCon;
        Skill[] skillSet = new Skill[3];
        switch (_type)
        {
            case 0:
                skillSet[0] = new MeleeAttack(1.2f, 3f, 1.3f);
                skillSet[1] = new ThrowShurican(6f, 6f, 4f);
                skillSet[2] = new ThrowStone(6f, 5f, 6f);
                break;
            case 1:
                skillSet[0] = new MeleeAttack(1.2f, 3f, 1.3f);
                skillSet[1] = new ThrowShurican(6f, 6f, 4f);
                skillSet[2] = new ThrowStone(6f, 5f, 6f);
                break;
            case 2:
                skillSet[0] = new MeleeAttack(1.2f, 3f, 1.3f);
                skillSet[1] = new ThrowShurican(6f, 6f, 4f);
                skillSet[2] = new ThrowStone(6f, 5f, 6f);
                break;
            default:
                print("잘못된 스킬 타입입니다.");
                break;

        }
        return skillSet;
    }
}
