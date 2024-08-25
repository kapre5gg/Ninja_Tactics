using UnityEngine;

public class AvatarChanger : MonoBehaviour
{
    public AvatarHelper avatarHelper;
    private int curIdx = 0;
    public int CurIdx
    {
        get { return curIdx; }
        set
        {
            if (value < 0)
                curIdx = avatarHelper.Avartars.Count - 1;
            else if (value >= avatarHelper.Avartars.Count)
                curIdx = 0;
            else
                curIdx = value;
        }
    }
    private void Start()
    {
        avatarHelper.gameObject.SetActive(true);
        avatarHelper.EnableAvartars(0);
        //avatarHelper.anim = avatarHelper.GetComponent<Animator>();
    }
    public void OnClickLeft()
    {
        CurIdx--;
        avatarHelper.EnableAvartars(CurIdx);
    }
    public void OnClickRight()
    {
        CurIdx++;
        avatarHelper.EnableAvartars(CurIdx);
    }
}
