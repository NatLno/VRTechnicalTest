using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public TMP_Text textMeshTunneling;
    public TMP_Text textMeshSmoothStartStop;
    public TMP_Text textMeshAudioSource;
    public TeleportPlayer player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetTunneling())
            textMeshTunneling.text = "Enabled";
        else if (!player.GetTunneling())
            textMeshTunneling.text = "Disabled";
        
        if (player.GetSmoothStartStop())
            textMeshSmoothStartStop.text = "Enabled";
        else if (!player.GetSmoothStartStop())
            textMeshSmoothStartStop.text = "Disabled";

        if (textMeshAudioSource != null)
        {
            AudioSource audioSource = player.GetAudioSource();
            if (audioSource == null || !audioSource.enabled)
                textMeshAudioSource.text = "No Audio Source";
            else if (audioSource.isPlaying)
                textMeshAudioSource.text = "Enabled";
            else if (!audioSource.isPlaying)
                textMeshAudioSource.text = "Disabled";
        }
    }
}
