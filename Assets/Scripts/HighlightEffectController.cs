using HighlightPlus;
using UnityEngine;
using UnityEngine.UI;

public class HighlightEffectController : MonoBehaviour
{
    [SerializeField] private GameObject[] parentsObject;
    [SerializeField] private Image iconImg;
    private bool isActivated;

    private void Update()
    {
        if (isActivated)
        {
            iconImg.sprite = Resources.Load<Sprite>($"activeHighlight");
        }
        else
        {
            iconImg.sprite = Resources.Load<Sprite>($"inactiveHighlight");
        }
    }

    public void HighLightBtn()
    {
        isActivated = !isActivated;
        foreach (GameObject parent in parentsObject)
        {
            HighlightEffect[] childs = parent.GetComponentsInChildren<HighlightEffect>();

            foreach (HighlightEffect child in childs)
            {
                child.highlighted = !child.highlighted;
            }
        }
    }

    void SetHighlight()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            foreach (GameObject parent in parentsObject)
            {
                HighlightEffect[] childs = parent.GetComponentsInChildren<HighlightEffect>();

                foreach (HighlightEffect child in childs)
                {
                    child.highlighted = !child.highlighted;
                }
            }
        }
    }


    
}