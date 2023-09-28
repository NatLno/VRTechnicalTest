using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OVRSlider : MonoBehaviour
{
    [SerializeField]
    private OneGrabTranslateTransformerIncrement grabTranslateTransformer;

    [SerializeField]
    [Tooltip("The object that is visually grabbed and manipulated")]
    Transform m_Handle = null;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_Value;

    [SerializeField]
    private float m_Increment;

    [SerializeField]
    bool m_IsActivated = false;

    [SerializeField]
    [Tooltip("Events to trigger when the slider activates")]
    UnityEvent m_OnSliderActivate = new UnityEvent();

    [SerializeField]
    [Tooltip("Events to trigger when the slider desactivates")]
    UnityEvent m_OnSliderDesactivate = new UnityEvent();

    private float minValue = -0.04f;
    private float maxValue = 0.04f;

    private float SliderDeadZone = 0.2f;
    private bool previousActivationState = false;

    public float Value
    {
        get => m_Value;
        set => m_Value = value;
    }

    public float increment
    {
        get => m_Increment;
        set => m_Increment = value;
    }

    public bool isActivated
    {
        get => m_IsActivated;
        set => m_IsActivated = value;
    }

    public UnityEvent onSliderActivate => m_OnSliderActivate;

    public UnityEvent onSliderDesactivate => m_OnSliderDesactivate;

    // Start is called before the first frame update
    void Start()
    {
        previousActivationState = isActivated;
        m_Value = (isActivated) ? 1f : 0f;
        SetHandleAngle(GetValue());
        SetTranslateTransformerConstraints();
    }


    // Update is called once per frame
    void Update()
    {
        m_Value = GetNormalizedValue();
        CheckSliderStatus();
    }

    private void SetTranslateTransformerConstraints()
    {
        float val = GetValue();
        grabTranslateTransformer.Constraints.MaxZ.Value = maxValue - val;
        grabTranslateTransformer.Constraints.MinZ.Value = minValue - val;

        grabTranslateTransformer.IncrementAmount = ConvertIncrement();
    }

    private void CheckSliderStatus()
    {
        if (GetNormalizedValue() >= 1f - SliderDeadZone && GetNormalizedValue() <= 1f)
            isActivated = true;
        else if (GetNormalizedValue() >= 0f && GetNormalizedValue() <= 0f + SliderDeadZone)
            isActivated = false;

        if (isActivated != previousActivationState)
        {
            if (isActivated)
                onSliderActivate.Invoke();
            else
                onSliderDesactivate.Invoke();
            previousActivationState = isActivated;
        }
    }

    void SetHandleAngle(float value)
    {
        if (m_Handle != null)
            m_Handle.localPosition = new Vector3(0.0f, 0.0f, value);
    }

    private float GetValue()
    {
        return Mathf.Lerp(minValue, maxValue, m_Value);
    }
    private float GetNormalizedValue()
    {
        return Mathf.InverseLerp(minValue, maxValue, m_Handle.localPosition.z);
    }

    private float ConvertIncrement()
    {
        return Mathf.Lerp(minValue, maxValue, m_Increment) - minValue;
    }

    private void OnValidate()
    {
        SetHandleAngle(GetValue());
        SetTranslateTransformerConstraints();
    }
}
