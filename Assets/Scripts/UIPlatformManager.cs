using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlatformManager : MonoBehaviour
{
    public TMP_Text textMeshTunneling;
    public TMP_Text textMeshRadiusValue;
    public Slider radiusSlider;
    public TMP_Text textMeshTransparencyValue;
    public Slider transparencySlider;
    
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
        if (textMeshRadiusValue != null)
            textMeshRadiusValue.text = splineManager.GetApertureSize().ToString("0.00");
        if (textMeshTransparencyValue != null)
            textMeshTransparencyValue.text = splineManager.GetFeatheringEffect().ToString("0.00");

        if (radiusSlider != null && transparencySlider != null)
        {
            radiusSlider.value = splineManager.GetApertureSize();
            transparencySlider.value = splineManager.GetFeatheringEffect();
        }
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
