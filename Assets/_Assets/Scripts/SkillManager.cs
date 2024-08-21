using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Skill : MonoBehaviour
{
    [Header("skill info")]
    public float skillCool;
    public float skillRange;
    public float soundRange;
    private float lastUsedTime = 0f;
    //public Action ActionSkill;
    public virtual void UseSKill() { }

    public virtual GameObject SkillEffect(Vector3 _pos) 
    {
        GameObject sound = Instantiate(Resources.Load<GameObject>("SoundRange"), _pos, Quaternion.Euler(-90,0,0));
        return sound;
    }

    public bool IsOffCooldown()
    {
        return Time.time >= lastUsedTime + skillCool;
    }
    protected void UpdateCooldown()
    {
        lastUsedTime = Time.time;
    }

    public virtual void MakeSound(Vector3 _pos)
    {
        print("소리남");
        Collider[] colls = Physics.OverlapSphere(_pos, soundRange);
        foreach (Collider coll in colls)
        {
            if (coll.GetComponent<EnemyAI>() == null)
                continue;
            coll.GetComponent<EnemyAI>().Alarm();
        }
        Vector3 newPos = new Vector3(_pos.x, 0.01f, _pos.z);
        GameObject sound = Instantiate(Resources.Load<GameObject>("Sound"), newPos, Quaternion.identity);
        sound.transform.localScale = Vector3.one * soundRange;
        Destroy(sound, 1f);
    }
}

public class MeleeAttack : Skill
{
    public MeleeAttack(float _skillRange, float _soundRange, float _Cool)
    {
        skillCool = _Cool;
        skillRange = _skillRange;
        soundRange = _soundRange;
    }
    public override void UseSKill()
    {
        if (!IsOffCooldown())
            return;
        print("근접스킬");
        GameObject target = DBManager.instance.myCon.target;
        DBManager.instance.myCon.Chaseto(target, skillRange);
        DBManager.instance.myCon.WaitforChase(0, DBManager.instance.myCon.gameObject.transform.position);
        //DBManager.instance.myCon.ChangeAnim("Attack");
        if (target.GetComponent<EnemyAI>() != null)
        {
            target.GetComponent<EnemyAI>().Die();
        }
    }


    public override GameObject SkillEffect(Vector3 _pos)
    {
        return base.SkillEffect(_pos);
    }

    public override void MakeSound(Vector3 _pos)
    {
        base.MakeSound(_pos);
        //StartCoroutine(nameof(RTest));
    }
}

public class ThrowShurican : Skill
{
    public ThrowShurican(float _skillRange, float _soundRange, float _cool)
    {
        skillCool = _cool;
        skillRange = _skillRange;
        soundRange = _soundRange;
    }
    public override void UseSKill()
    {
        base.UseSKill();
        print("수리검 던지기");
        if (DBManager.instance.myCon.target.GetComponent<EnemyAI>() != null)
        {
            DBManager.instance.myCon.target.GetComponent<EnemyAI>().Die();
        }
    }
    public override void MakeSound(Vector3 _origin)
    {
        base.MakeSound(_origin);
    }

}

public class SkillManager : MonoBehaviour
{
    public Image[] skillIcon;
    private NinjaController ninjaCon = null;

    public Skill[] GetSkill(int _type)
    {
        ninjaCon = DBManager.instance.myCon;
        Skill[] awef = new Skill[3];
        switch (_type)
        {
            case 0:
                awef[0] = new MeleeAttack(0.5f, 3, 5);
                break; 
            case 1:
                awef[0] = new MeleeAttack(0.5f, 3, 5);
                break;
            case 2:
                awef[0] = new MeleeAttack(0.5f, 3, 5);
                break;
            default:
                print("잘못된 스킬 타입입니다.");
                break;

        }
        return awef;
    }
}
