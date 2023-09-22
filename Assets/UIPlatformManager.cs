using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPlatformManager : MonoBehaviour
{
    public TMP_Text textMeshTunneling;
    public TMP_Text textMeshAudioSource;
    public SplineManager splineManager;

    // Update is called once per frame
    void Update()
    {
        CheckTunneling();
        CheckAudioSource();
    }

    void CheckTunneling()
    {
        if (splineManager.GetTunneling())
            textMeshTunneling.text = "Enabled";
        else
            textMeshTunneling.text = "Disabled";
    }

    void CheckAudioSource()
    {
        AudioSource audioSource = splineManager.GetAudioSource();
        if (audioSource == null || !audioSource.enabled)
            textMeshAudioSource.text = "No Audio Source";
        else if (audioSource.isPlaying)
            textMeshAudioSource.text = "Enabled";
        else if (!audioSource.isPlaying)
            textMeshAudioSource.text = "Disabled";
    }
}
