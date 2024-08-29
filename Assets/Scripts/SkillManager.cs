using System;
using System.Collections;
using System.Collections.Generic;
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
        this.lastUsedTime = -skillCool; // ó������ ��ų�� ����� �� �ֵ��� ����

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

    protected void RotateToFace(Vector3 targetPosition) // Ÿ�� �������� ȸ��
    {
        Vector3 direction = (targetPosition - casterTr.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        casterTr.rotation = Quaternion.Slerp(casterTr.rotation, lookRotation, Time.deltaTime * 10f);
    }
    protected IEnumerator ApproachUseSkillCor()
    {
        yield return null;
        NinjaController casterCon = DBManager.instance.myCon;
        if (casterCon.isSitting)
            casterCon.ChangeAnim("SitWalk");
        else
            casterCon.ChangeAnim("Walk");

        Vector3 targetPosition;
        if (IsTargetSkill())
        {
            targetPosition = casterCon.target.position;
        }
        else
        {
            targetPosition = casterCon.nonTargetPos;
        }

        // Ÿ�� �������� ȸ��
        while (Vector3.Angle(casterTr.forward, (targetPosition - casterTr.position).normalized) > 1f)
        {
            RotateToFace(targetPosition);
            yield return null;
        }

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
        caster.target.GetComponent<Enemy>().Die();
        caster.skillIndicatorPrefab.SetActive(false);
        SkillManager.instance.SkillCool(0);
        caster.NotUseSkill();
    }

    public override void ApproachUseSkill()
    {
        if (IsOffCooldown() && IsTargetSkill())
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("��ų �� �ȵ��ƿ�");
    }
}

public class Shurican : Skill
{
    public Shurican(float skillRange, float soundRange, float skillCool)
        : base(skillRange, soundRange, skillCool) { }

    public override void UseSkill()
    {
        if(SkillManager.instance.lostShuriken) 
            return;
        caster.agent.SetDestination(casterTr.position);
        caster.ChangeAnim("ThrowShuriken");
        caster.GetComponent<MonoBehaviour>().StartCoroutine(WaitforAct());
        SkillManager.instance.SkillCool(1);
        caster.NotUseSkill();
        SkillManager.instance.lostShuriken = true;
        SkillManager.instance.LostSomething(1, false);
    }

    public override void ApproachUseSkill()
    {
        if (IsOffCooldown() && IsTargetSkill())
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("��ų �� �ȵ��ƿ�");
    }
    private IEnumerator WaitforAct()
    {
        yield return new WaitForSeconds(0.6f);
        SkillManager.instance.ThrowShuriken(caster.transform.position, caster.target.gameObject, soundRange);
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
        if (type == 2 && SkillManager.instance.lostSakke)
            return;
        caster.agent.SetDestination(casterTr.position);
        caster.ChangeAnim("Throw");
        caster.GetComponent<MonoBehaviour>().StartCoroutine(WaitforAct());
        SkillManager.instance.SkillCool(2);
        caster.NotUseSkill();
        if (type == 2)
        {
            SkillManager.instance.lostSakke = true;
            SkillManager.instance.LostSomething(2, false);
        }
    }
    private IEnumerator WaitforAct()
    {
        yield return new WaitForSeconds(0.6f);
        switch (type)
        {
            case 0:
                Debug.Log("������");
                //���󰡴� �Լ�
                SkillManager.instance.ThrowSomething("Stone", casterTr.position, caster.nonTargetPos, soundRange, type);
                break;
            case 1:
                Debug.Log("�𷡴���");
                //���󰡴� �Լ�
                SkillManager.instance.ThrowSomething("Sneeze Powder", casterTr.position, caster.nonTargetPos, soundRange, type);
                break;
            case 2:
                Debug.Log("������");
                //���󰡴� �Լ�
                SkillManager.instance.ThrowSomething("Sakke", casterTr.position, caster.nonTargetPos, soundRange, type);
                break;
            default:
                break;
        }
    }

    public override void ApproachUseSkill()
    {
        if (IsOffCooldown())
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("��ų �� �ȵ��ƿ�");
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
        caster.GetComponent<MonoBehaviour>().StartCoroutine(WaitforAct());
        SkillManager.instance.SkillCool(1);
        caster.NotUseSkill();
    }
    private IEnumerator WaitforAct()
    {
        yield return new WaitForSeconds(1f);
        SkillManager.instance.MakeSound(caster.nonTargetPos, soundRange);
        Collider[] colls = Physics.OverlapSphere(casterTr.position, soundRange);
        foreach (Collider coll in colls)
        {
            Enemy enemyAI = coll.GetComponent<Enemy>();
            if (enemyAI != null)
            {
                enemyAI.Die();
            }
        }
    }
    public override void ApproachUseSkill()
    {
        if (IsOffCooldown())
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("��ų �� �ȵ��ƿ�");
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
        caster.target.GetComponent<Enemy>().ChangeStunState(StunType.FixedView, 5f);
        SkillManager.instance.SkillCool(1);
        caster.NotUseSkill();
    }
    public override void ApproachUseSkill()
    {
        if (IsOffCooldown() && IsTargetSkill() && !SkillManager.instance.lostKimono)
            casterTr.GetComponent<MonoBehaviour>().StartCoroutine(ApproachUseSkillCor());
        else
            Debug.Log("��ų �� �ȵ��ƿ�");
    }
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;
    public Dictionary<string, Action> itemActions = new Dictionary<string, Action>
    {
        { "Shuriken", () => SkillManager.instance.HasShuriken() },
        { "Kimono", () => SkillManager.instance.HasKimono() },
        { "Sakke", () => SkillManager.instance.HasSakke() }
    };
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
    public bool lostShuriken = false;
    public bool lostKimono = false;
    public bool lostSakke = false;
    public Color unableColor = new Color(100 / 255f, 100 / 255f, 100 / 255f, 1);
    private Coroutine coolCor = null;
    public Skill[] GetSkill(int _type)
    {
        ninjaCon = DBManager.instance.myCon;
        switch (_type)
        {
            case 0:
                skillSet[0] = new MeleeAttack(1.2f, 3f, 2f ); //1.3f
                skillSet[1] = new Shurican(6f, 6f, 4f);
                skillSet[2] = new ThrowSomething(6f, 5f, 6f, 0);
                SKillIconSet(0);
                break;
            case 1:
                skillSet[0] = new MeleeAttack(1.2f, 3f, 2f);
                skillSet[1] = new TalktoEnemy(1.2f, 0f, 4f);
                skillSet[2] = new ThrowSomething(6f, 3f, 6f, 1);
                SKillIconSet(1);
                break;
            case 2:
                skillSet[0] = new MeleeAttack(1.2f, 3f, 2f); //1.4f
                skillSet[1] = new SlashBlade(2f, 3f, 18f);
                skillSet[2] = new ThrowSomething(6f, 0f, 0f, 2);
                SKillIconSet(2);
                break;
            default:
                print("�߸��� ��ų Ÿ���Դϴ�.");
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
            if (_type == 1 && i == 1)
                lostKimono = true;
        }
    }

    public void SkillCool(int _skillNum)
    {
        skillIcons[_skillNum].fillAmount = 0;
        if (coolCor != null)
            StopCoroutine(coolCor);
        coolCor = StartCoroutine(FillCool(_skillNum));
    }
    private IEnumerator FillCool(int _skillNum)
    {
        while (skillIcons[_skillNum].fillAmount <= 1)
        {
            skillIcons[_skillNum].fillAmount += Time.deltaTime / skillSet[_skillNum].skillCool;
            //for (int i = 0; i < skillSet[_skillNum].skillCool; i++)
            //{
            //    yield return null;
            //}
            yield return null;
        }
    }
    public void LostSomething(int _skillidx, bool _bool)
    {
        float tempCool = skillIcons[_skillidx].fillAmount;
        if (!_bool)
            skillIcons[_skillidx].color = unableColor;
        else
            skillIcons[_skillidx].color = Color.white;
        skillIcons[_skillidx].fillAmount = tempCool;
    }

    public void MakeSound(Vector3 _pos, float _soundRange)
    {
        Debug.Log("�Ҹ���");
        Collider[] colls = Physics.OverlapSphere(_pos, _soundRange);
        foreach (Collider coll in colls)
        {
            Enemy enemy = coll.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ChangeStunState(StunType.RotateView, 5f, _pos);
            }
        }

        Vector3 newPos = new Vector3(_pos.x, 0.01f, _pos.z);

        RaycastHit hit;

        if (Physics.Raycast(_pos + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Terrain")))
        {
            newPos.y = hit.point.y;  // ������ ���̿� ���� ��ġ ����
        }
        else
        {
            newPos.y = 1f; // Raycast�� ������ ��� �⺻ ���̷� ����
        }
        GameObject sound = Instantiate(Resources.Load<GameObject>("Sound"), newPos, Quaternion.identity);
        sound.transform.localScale = Vector3.one * _soundRange;
        Destroy(sound, 1f);
    }
    
    public void HasShuriken()
    {
        lostShuriken = false;
        //skillIcons[1].fillAmount = 1;
        LostSomething(1, true);
    }
    public void HasKimono()
    {
        lostKimono = false;
        skillIcons[1].sprite = Resources.Load<Sprite>($"Skill_Icon1_3");
        skillIcons[1].fillAmount = 1;
    }
    public void HasSakke()
    {
        lostSakke = false;
        skillIcons[2].fillAmount = 1;
        skillSet[2].skillCool = 0;
        LostSomething(2, true);
    }

    public void ThrowShuriken(Vector3 _pos, GameObject _target, float _soundRange)
    {
        // Shuriken prefab�� �ش� ��ġ�� ����
        GameObject shuriken = Instantiate(Resources.Load<GameObject>("Shuriken"), _pos, Quaternion.identity);

        // MoveShurikenToTarget �ڷ�ƾ�� ����
        StartCoroutine(MoveShurikenToTarget(shuriken, _target, _soundRange));
    }

    private IEnumerator MoveShurikenToTarget(GameObject shuriken, GameObject target, float soundRange)
    {
        float speed = 20f;  // ������ �̵� �ӵ�
        float stopDistance = 0.1f;  // ��ǥ ������ �����ߴٰ� �����ϴ� �Ÿ�

        while (true)
        {
            // ��ǥ ��ġ������ �Ÿ� ���
            float distance = Vector3.Distance(shuriken.transform.position, target.transform.position);

            // ��ǥ ������ �����ߴ��� Ȯ��
            if (distance < stopDistance)
            {
                Debug.Log("�������� ��ǥ ������ �����߽��ϴ�!");
                shuriken.gameObject.layer = LayerMask.NameToLayer("Item");
                // �Ҹ� ȿ�� ȣ��
                SkillManager.instance.MakeSound(target.transform.position, soundRange);
                Enemy enemy = target.GetComponent<Enemy>();
                if (enemy != null) enemy.Die();

                yield break;  // �ڷ�ƾ ����
            }

            // ��ǥ �������� �̵�
            shuriken.transform.position = Vector3.MoveTowards(shuriken.transform.position, target.transform.position, speed * Time.deltaTime);

            // ���� �����ӱ��� ���
            yield return null;
        }
    }

    public void ThrowSomething(string _throwObj, Vector3 _startPos, Vector3 _targetPos, float _soundRange, int type)
    {
        GameObject throwObj = Instantiate(Resources.Load<GameObject>(_throwObj), _startPos, Quaternion.identity);

        StartCoroutine(MovingParabola(throwObj, _startPos, _targetPos, 2.0f, _soundRange, type)); // �ִ� ���̸� 2.0���� ����
    }

    // �������� �׸��� �̵���Ű�� �ڷ�ƾ
    private IEnumerator MovingParabola(GameObject throwObj, Vector3 startPos, Vector3 _targetPos, float height, float _soundRange, int type)
    {
        float duration = 1f; // �̵��ϴ� �� �ɸ��� �ð�
        float elapsed = 0f; // ��� �ð�

        Vector3 midPoint = (startPos + _targetPos) / 2; // �߰� ��
        midPoint.y += height; // �߰� ���� �ִ� ���̷� �ø�

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // ������ ��� ���
            Vector3 m1 = Vector3.Lerp(startPos, midPoint, t); // �������� �߰��� ������ ����
            Vector3 m2 = Vector3.Lerp(midPoint, _targetPos, t); // �߰����� ��ǥ�� ������ ����
            throwObj.transform.position = Vector3.Lerp(m1, m2, t); // �� ������ �� ������ ���� ����

            yield return null; // ���� �����ӱ��� ���
        }

        // ��ǥ ������ ��Ȯ�� ��ġ��Ŵ
        throwObj.transform.position = _targetPos;
        throwObj.gameObject.layer = LayerMask.NameToLayer("Item");
        // �Ҹ� ȿ�� ȣ��
        SkillManager.instance.MakeSound(_targetPos, _soundRange);
        if(type != 2) Destroy(throwObj.gameObject);

        // ��ǥ �������� �ֺ��� ���� �����ϰ� ó��
        Collider[] colliders = Physics.OverlapSphere(_targetPos, _soundRange);
        foreach (Collider collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // ���� ���� ȿ�� ����
                switch (type)
                {
                    case 0:
                        // stone�� ���� ȿ��
                        enemy.ChangeStunState(StunType.RotateView, 5f, _targetPos);
                        break;
                    case 1:
                        // sand�� ���� ȿ��
                        enemy.ChangeStunState(StunType.BlockingView, 5f, _targetPos);
                        break;
                    case 2:
                        // sakke�� ���� ȿ��
                        break;
                    default:
                        Debug.Log("�� �� ���� Ÿ��");
                        break;
                }
            }
        }
    }
}
