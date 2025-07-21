using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InfoBoxVisualManager : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public UnityEngine.UI.Image Image;
    // Update is called once per frame
    void Update()
    {
        //TextMeshProUGUI Text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
       
        if(Text.text == "") Image.enabled = false;
        else Image.enabled = true;
    }
}
