using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTooltip : MonoBehaviour
{
    [SerializeField] private Image skillImg;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillRangeText;
    [SerializeField] private TextMeshProUGUI soundRangeText;
    [SerializeField] private TextMeshProUGUI skillCoolText;
    private Skill[] skillset;
    private int ninjaIndex = -1;
    public int currentSkillIndex = -1;

    private void Start()
    {
        ninjaIndex = DBManager.instance.myCon.ninjaType;
        skillset = DBManager.instance.myCon.skillSet;
    }
    void Update()
    {
        if (currentSkillIndex >= 0 && currentSkillIndex < skillset.Length)
        {
            skillImg.sprite = Resources.Load<Sprite>($"Skill_Icon{ninjaIndex}_{currentSkillIndex}");
            skillNameText.text = skillset[currentSkillIndex].skillName;
            skillRangeText.text = skillset[currentSkillIndex].skillRange.ToString();
            soundRangeText.text = skillset[currentSkillIndex].soundRange.ToString();
            skillCoolText.text = skillset[currentSkillIndex].skillCool.ToString();
        }
    }

    public void SetCurrentSkillIndex(int index)
    {
        currentSkillIndex = index;
    }
}
