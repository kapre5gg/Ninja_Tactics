using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class AvatarCustomizer : NetworkBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator anim;

    [Header("Hair Parts")]
    [SerializeField] private List<GameObject> hairParts;

    [Header("Face Parts")]
    [SerializeField] private List<GameObject> faceParts;

    [Header("Top Parts")]
    [SerializeField] private List<GameObject> topParts;

    [Header("Bottom Parts")]
    [SerializeField] private List<GameObject> bottomParts;

    [Header("Bal Parts")]
    [SerializeField] private List<GameObject> balParts;

    [SyncVar(hook = nameof(OnChangeHair))]
    public int currentHairIndex = -1;
    [SyncVar(hook = nameof(OnChangeTop))]
    public int currentTopIndex = -1;
    [SyncVar(hook = nameof(OnChangeBottom))]
    public int currentBotIndex = -1;
    [SyncVar(hook = nameof(OnChangeFace))]
    public int currentFaceIndex = -1;
    [SyncVar(hook = nameof(OnChangeBal))]
    public int currentBalIndex = -1;

    public override void OnStartLocalPlayer()
    {
        //InitAvata();
        CmdUpdateAvatar(DBManager.instance.hairIndex, DBManager.instance.faceIndex, DBManager.instance.topIndex, DBManager.instance.botIndex, DBManager.instance.botIndex);
    }

    [Command]
    public void CmdUpdateAvatar(int hair, int face, int top, int bot, int bal)
    {
        //매개변수 잘 받아오는지 (에디터로 서버 열 때만 디버그가 뜬다)
        currentHairIndex = hair;
       // ChangeHair(hair);
        currentFaceIndex = face;
       // ChangeFace(face);
        currentTopIndex = top;
       // ChangeTop(top);
        currentBotIndex = bot;
       // ChangeBottom(bot);
        currentBalIndex = bal;
       // ChangeBal(bal);
    }

    private void InitAvata()
    {
        hairParts[0].SetActive(true);
        faceParts[0].SetActive(true);
        topParts[0].SetActive(true);
        bottomParts[0].SetActive(true);
        balParts[0].SetActive(true);
        //currentHairIndex = 0;
        //currentFaceIndex = 0;
        //currentTopIndex = 0;
        //currentBotIndex = 0;
        //currentTopIndex = 0;
    }
    
    void OnChangeHair(int _Old, int _New)
    {
        ChangeHair_(_New);
    }
    private void ChangeHair_(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < hairParts.Count; i++)
        {
            hairParts[i].SetActive(i == index);
        }
    }
    public void ChangeHair(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < hairParts.Count; i++)
        {
            hairParts[i].SetActive(i == index);
            DBManager.instance.hairIndex = index;
        }
    }
    
    void OnChangeTop(int _Old, int _New)
    {
        ChangeTop_(_New);
    }
    private void ChangeTop_(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < topParts.Count; i++)
        {
            topParts[i].SetActive(i == index);
        }
    }
    public void ChangeTop(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < topParts.Count; i++)
        {
            topParts[i].SetActive(i == index);
            DBManager.instance.topIndex = index;
        }
    }
    
    void OnChangeBottom(int _Old, int _New)
    {
        ChangeBottom_(_New);
    }
    private void ChangeBottom_(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < bottomParts.Count; i++)
        {
            bottomParts[i].SetActive(i == index);
        }
    }
    public void ChangeBottom(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < bottomParts.Count; i++)
        {
            bottomParts[i].SetActive(i == index);
            DBManager.instance.botIndex = index;
        }
    }
    
    void OnChangeFace(int _Old, int _New)
    {
        ChangeFace_(_New);
    }
    private void ChangeFace_(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < faceParts.Count; i++)
        {
            faceParts[i].SetActive(i == index);
        }
    }
    public void ChangeFace(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < faceParts.Count; i++)
        {
            faceParts[i].SetActive(i == index);
            DBManager.instance.faceIndex = index;
        }
    }
    
    void OnChangeBal(int _Old, int _New)
    {
        ChangeBal_(_New);
    }
    private void ChangeBal_(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < balParts.Count; i++)
        {
            balParts[i].SetActive(i == index);
        }
    }
    public void ChangeBal(int index)
    {
        anim.SetTrigger("Clapping");
        for (int i = 0; i < balParts.Count; i++)
        {
            balParts[i].SetActive(i == index);
            DBManager.instance.balIndex = index;
        }
    }
}
