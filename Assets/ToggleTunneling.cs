using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToggleTunneling : MonoBehaviour
{
    public TMP_Text textMesh;
    public TeleportPlayer player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetTunneling())
            textMesh.text = "Enabled";
        else
            textMesh.text = "Disabled";
    }
}
