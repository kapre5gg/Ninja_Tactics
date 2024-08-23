using HighlightPlus;
using System.Collections;
using UnityEngine;

public class HighlightEffectController : MonoBehaviour
{
    public GameObject[] parentsObject;

    public void HighLightBtn()
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
