using TMPro;
using UnityEngine;

public class CustomizationUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown hairDropdown;
    [SerializeField] private TMP_Dropdown faceDropdown;
    [SerializeField] private TMP_Dropdown topDropdown;
    [SerializeField] private TMP_Dropdown bottomDropdown;
    [SerializeField] private TMP_Dropdown balDropdown;

    [Header("Avatar Customizer")]
    [SerializeField] private AvatarCustomizer avatarCustomizer;

    private void Start()
    {
        hairDropdown.onValueChanged.AddListener(delegate { OnHairChange(hairDropdown.value); });
        faceDropdown.onValueChanged.AddListener(delegate { OnFaceChange(faceDropdown.value); });
        topDropdown.onValueChanged.AddListener(delegate { OnTopChange(topDropdown.value); });
        bottomDropdown.onValueChanged.AddListener(delegate { OnBottomChange(bottomDropdown.value); });
        balDropdown.onValueChanged.AddListener(delegate { OnBalChange(balDropdown.value); });
        avatarCustomizer.gameObject.SetActive(true);
    }

    private void OnHairChange(int index)
    {
        avatarCustomizer.ChangeHair(index);
    }

    private void OnFaceChange(int index)
    {
        avatarCustomizer.ChangeFace(index);
    }

    private void OnTopChange(int index)
    {
        avatarCustomizer.ChangeTop(index);
    }

    private void OnBottomChange(int index)
    {
        avatarCustomizer.ChangeBottom(index);
    }

    private void OnBalChange(int index)
    {
        avatarCustomizer.ChangeBal(index);
    }
}
