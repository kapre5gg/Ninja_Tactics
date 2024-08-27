using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentSet
{
    public List<GameObject> equipments = new List<GameObject>();
}
public class AvatarHelper : NetworkBehaviour
{
    public List<GameObject> Avartars = new List<GameObject>();
    public List<Avatar> animatorCon = new List<Avatar>();
    public List<EquipmentSet> equipmentLists = new List<EquipmentSet>();
    public Animator anim;
    [SyncVar(hook = nameof(OnChangeAvatar))]
    public int myAvatar = -1;
    [SyncVar(hook = nameof(OnChangeEquipment))]
    public int myEquip = -1;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void OnStartLocalPlayer()
    {
        //anim = GetComponent<Animator>();
        CmdSetMyavatar(DBManager.instance.avatarIdx);
        CmdSetMyEquip(DBManager.instance.PlayerNinjaType);
    }

    [Command]
    public void CmdSetMyavatar(int _idx)
    {
        myAvatar = _idx;
    }

    void OnChangeAvatar(int _Old, int _New)
    {
        LocalEnableAvartars(_New);
    }

    private void LocalEnableAvartars(int _idx)
    {
        for (int i = 0; i < Avartars.Count; i++)
        {
            Avartars[i].SetActive(false);
        }
        Avartars[_idx].SetActive(true);
        if (anim != null)
            anim.avatar = animatorCon[_idx]; //왜 애니메이션이 안바뀌는거지? (늦게 입장한 플레이어는 먼저 입장한 플레이어의 애니메이션이 동작 안함)
        else
            print("애니메이션 없음");
    }

    public void EnableAvartars(int _idx)
    {
        LocalEnableAvartars(_idx);
        DBManager.instance.avatarIdx = _idx;
    }
    [Command]
    public void CmdSetMyEquip(int _idx)
    {
        myEquip = _idx;
    }
    void OnChangeEquipment(int _Old, int _New)
    {
        LocalEnableEquipment(_New);
    }

    private void LocalEnableEquipment(int _idx)
    {
        // 모든 장비 비활성화
        foreach (var equipmentSet in equipmentLists)
        {
            foreach (var equipment in equipmentSet.equipments)
            {
                equipment.SetActive(false);
            }
        }

        // 선택한 장비 활성화
        foreach (var equipment in equipmentLists[_idx].equipments)
        {
            equipment.SetActive(true);
        }
    }

    public void EnableEquipment(int _idx)
    {
        LocalEnableEquipment(_idx);
        DBManager.instance.PlayerNinjaType = _idx;
    }
}
