using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NinjaProfile : MonoBehaviour
{
    [Header("Components : Script Caching")]
    [SerializeField] private List<GameObject> listProfileGroup = new List<GameObject>();
    public Image[] hpSlides;
    public TMP_Text[] nameText;
    public void SetEnableProfile(int _idx)
    {
        listProfileGroup[_idx].gameObject.SetActive(true);
        hpSlides[_idx].fillAmount = 1;
    }
    public void SetName(string _name)
    {
        foreach (var item in nameText)
        {
            item.text = _name;
        }
    }
    public void UpdateHp(int _idx, int _type, int _hp)
    {
        if (_hp < 0)
            return;
        switch (_type)
        {
            case 0:
            case 1:
                hpSlides[_idx].fillAmount = _hp / 3f;
                break;
            case 2:
                hpSlides[_idx].fillAmount = _hp / 5f;
                break;
            default:
                break;
        }
    }
}
