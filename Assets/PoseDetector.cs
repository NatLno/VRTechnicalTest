using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class PoseDetector : MonoBehaviour
{
    public List<ActiveStateSelector> poses;
    public TMPro.TextMeshPro text;
    
    void Start()
    {
        foreach (var item in poses){
            item.WhenSelected += () => SetTextToPoseName(item.gameObject.name);
            item.WhenUnselected += () => SetTextToPoseName("");
        }    
    }

    private void SetTextToPoseName(string newText){
        text.text = newText;
    } 
}
