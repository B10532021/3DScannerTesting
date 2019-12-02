using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjButton : MonoBehaviour
{
    private string Name;
    public Text ButtonText;
    public ObjList scrollView;

    public void SetName(string name)
    {
        Name = name;
        ButtonText.text = name;
    }
    public void ButtonClick()
    {
        scrollView.ButtonClicked(Name);
    }
}
