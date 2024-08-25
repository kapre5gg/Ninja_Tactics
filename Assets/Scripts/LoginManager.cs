using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    //회원가입?과 로그인 기능

    public TitleManager titleManager;

    [Header("UI")]
    public TMP_InputField idInput;
    public TMP_InputField pwInput;
    public TMP_InputField registID;
    public TMP_InputField registPW;
    public TMP_InputField registNick;
    public Image errorImage;
    public TMP_Text errorText;
    public TMP_Text avatarName;

    [Header("Canvas")]
    [SerializeField] private GameObject titleCanvas;
    [SerializeField] private GameObject avataCanvas;
    public void LoginBtn()
    {
        DBManager.User user = new DBManager.User();
        //아디,비번 모두 있어야 작동하도록
        if (user.CheckNickName(idInput.text, pwInput.text) && user.nickName.Length > 0)
        {
            DBManager.instance.playerName = user.nickName;
            avatarName.text = user.nickName;
            titleCanvas.SetActive(false);
            avataCanvas.SetActive(true);
            //titleManager.RunStartClient();
        }
        //DBManager.instance.playerName = idInput.text;
        else
        {
            errorText.text = "잘못된 아이디 또는 비밀번호입니다.";
            errorImage.gameObject.SetActive(true);
        }
    }

    public void StartBtn()
    {
        titleManager.RunStartClient();
    }

    public void RegistBtn()
    {
        DBManager.User user = new DBManager.User();
        if (user.CheckNickName(registID.text, registPW.text))//아이디 비번이 이미 있으면 안됨
        {
            errorText.text = "중복된 아이디 또는 비밀번호입니다.";
            errorImage.gameObject.SetActive(true);
            return;
        }
            
        if (registID.text.Length > 0 && registPW.text.Length > 0 && registNick.text.Length > 0)
        {
            user.RegistID(registID.text, registPW.text, registNick.text);
            titleManager.RegistBtn();
        }
    }
}
