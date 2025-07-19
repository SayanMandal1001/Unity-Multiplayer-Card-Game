using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameTextModifier : MonoBehaviour
{
    public int maximumLength = 10;

    // Update is called once per frame
    void Update()
    {
        TextMeshPro Text = this.gameObject.GetComponent<TextMeshPro>();
        if (Text.text.Length > maximumLength)
        {
            string name = Text.text.Substring(0, maximumLength);
            Text.text = name + "...";
        }
    }
}
