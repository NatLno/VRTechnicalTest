using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPlatformManager : MonoBehaviour
{
    public TMP_Text textMeshTunneling;
    public TMP_Text textMeshAudioSource;
    public TMP_Text textMeshSmoothStartStop;
    public SplineManager splineManager;

    // Update is called once per frame
    void Update()
    {
        CheckTunneling();
        CheckAudioSource();
        if (textMeshSmoothStartStop != null)
            CheckSmoothStartStop();
    }

    void CheckTunneling()
    {
        textMeshTunneling.text = splineManager.GetTunneling() ? "Enabled" : "Disabled";
    }

    void CheckAudioSource()
    {
        AudioSource audioSource = splineManager.GetAudioSource();
        if (audioSource == null)
            textMeshAudioSource.text = "No Audio Source";
        else if (audioSource.enabled)
            textMeshAudioSource.text = "Enabled";
        else if (!audioSource.enabled)
            textMeshAudioSource.text = "Disabled";
    }

    void CheckSmoothStartStop()
    {
        textMeshSmoothStartStop.text = splineManager.GetSmoothStartStop() ? "Enabled" : "Disabled";
    }
}
