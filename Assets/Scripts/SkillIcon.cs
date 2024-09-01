using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SkillTooltip tooltip;
    [SerializeField] private Button useSkillbtn;
    public int skillIndex = 0;
    private void Start()
    {
        useSkillbtn.onClick.AddListener(() => DBManager.instance.myCon.ReadySkill(skillIndex));
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.SetCurrentSkillIndex(skillIndex);
        tooltip.gameObject.SetActive(true);
        tooltip.transform.position = eventData.position + new Vector2(0, 250f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.gameObject.SetActive(false);
    }
}
