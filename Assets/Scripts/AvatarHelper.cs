using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class AvatarHelper : NetworkBehaviour
{
    public List<GameObject> Avartars = new List<GameObject>();
    public List<Avatar> animatorCon = new List<Avatar>();
    public Animator anim;
    [SyncVar(hook = nameof(OnChangeAvatar))]
    public int myAvatar = -1;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void OnStartLocalPlayer()
    {
        //anim = GetComponent<Animator>();
        CmdSetMyavatar(DBManager.instance.avatarIdx);
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
            anim.avatar = animatorCon[_idx]; //�� �ִϸ��̼��� �ȹٲ�°���? (�ʰ� ������ �÷��̾�� ���� ������ �÷��̾��� �ִϸ��̼��� ���� ����)
        else
            print("�ִϸ��̼� ����");
    }

    public void EnableAvartars(int _idx)
    {
        LocalEnableAvartars(_idx);
        DBManager.instance.avatarIdx = _idx;
    }
}
