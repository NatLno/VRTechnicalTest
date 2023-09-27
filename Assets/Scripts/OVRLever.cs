using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class OVRLever : MonoBehaviour
{
    [SerializeField]
    private OneGrabRotateTransformer grabRotateTransformer;
    
    [SerializeField]
    [Tooltip("The object that is visually grabbed and manipulated")]
    Transform m_Handle = null;

    [SerializeField]
    [Range(0f, 1f)]
    float m_Value;

    [SerializeField]
    [Tooltip("Angle of the lever in the 'on' position")]
    [Range(-90.0f, 90.0f)]
    float m_MaxAngle = 90.0f;

    [SerializeField]
    [Tooltip("Angle of the lever in the 'off' position")]
    [Range(-90.0f, 90.0f)]
    float m_MinAngle = -90.0f;

    [SerializeField]
    bool m_IsActivated = false;


    [SerializeField]
    UnityEvent m_OnStart = new UnityEvent();

    [SerializeField]
    [Tooltip("Events to trigger when the lever activates")]
    UnityEvent m_OnLeverActivate = new UnityEvent();

    [SerializeField]
    [Tooltip("Events to trigger when the lever deactivates")]
    UnityEvent m_OnLeverDeactivate = new UnityEvent();

    private float LeverDeadZoneAudio = 0.3f;
    private bool previousActivationState = false;
    public float value
    {
        get => m_Value;
        set => m_Value = value;
    }

    public float maxAngle
    {
        get => m_MaxAngle;
        set => m_MaxAngle = value;
    }

    public float minAngle
    {
        get => m_MinAngle;
        set => m_MinAngle = value;
    }

    public bool isActivated
    {
        get => m_IsActivated;
        set => m_IsActivated = value;
    }
    public UnityEvent onStart => m_OnStart;

    public UnityEvent onLeverActivate => m_OnLeverActivate;

    public UnityEvent onLeverDeactivate => m_OnLeverDeactivate;


    private void Start()
    {
        onStart.Invoke();
        previousActivationState = isActivated;
        m_Value = (isActivated) ? 1f: 0f;
        SetHandleAngle(GetValue());
        SetRotateTransformerConstraints();
    }

    private void Update()
    {
        m_Value = GetNormalizedValue();
        CheckLeverStatus();
    }

    private void CheckLeverStatus()
    {
        if (GetNormalizedValue() >= 1f - LeverDeadZoneAudio && GetNormalizedValue() <= 1f) 
            isActivated = true;
        else if (GetNormalizedValue() >= 0f && GetNormalizedValue() <= 0f + LeverDeadZoneAudio)
            isActivated = false;

        if (isActivated != previousActivationState)
        {
            if (isActivated)
                onLeverActivate.Invoke();
            else
                onLeverDeactivate.Invoke();

            previousActivationState = isActivated;
        }
    }

    float GetNormalizedValue()
    {
        float angle = m_Handle.localRotation.eulerAngles.x;
        if (angle > 180f)
            angle -= 360f;

        return Mathf.InverseLerp(m_MinAngle, m_MaxAngle, angle);
    }

    void SetRotateTransformerConstraints()
    {
        float val = GetValue();
        grabRotateTransformer.Constraints.MaxAngle.Value = m_MaxAngle - val;
        grabRotateTransformer.Constraints.MinAngle.Value = m_MinAngle - val;
    }

    void SetHandleAngle(float angle)
    {
        if (m_Handle != null)
            m_Handle.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);
    }

    void OnDrawGizmosSelected()
    {
        var angleStartPoint = transform.position;

        if (m_Handle != null)
            angleStartPoint = m_Handle.position;
        
        const float k_AngleLength = 0.25f;

        var angleMaxPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MaxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
        var angleMinPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MinAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(angleStartPoint, angleMaxPoint);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(angleStartPoint, angleMinPoint);
    }

    private float GetValue()
    {
        return Mathf.Lerp(m_MinAngle, m_MaxAngle, m_Value);
    }

    void OnValidate()
    {
        SetHandleAngle(GetValue());
        SetRotateTransformerConstraints();
    }
}
