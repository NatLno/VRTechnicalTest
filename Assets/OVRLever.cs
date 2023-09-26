using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("Events to trigger when the lever activates")]
    UnityEvent m_OnLeverActivate = new UnityEvent();

    [SerializeField]
    [Tooltip("Events to trigger when the lever deactivates")]
    UnityEvent m_OnLeverDeactivate = new UnityEvent();

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

    public UnityEvent onLeverActivate => m_OnLeverActivate;

    public UnityEvent onLeverDeactivate => m_OnLeverDeactivate;


    private void Start()
    {
        SetRotateTransformerConstraints();
    }

    private void Update()
    {
    }

    float GetNormalizedValue()
    {
        return Mathf.InverseLerp(minAngle, maxAngle, m_Handle.localRotation.eulerAngles.x);
    }

    void SetRotateTransformerConstraints()
    {
        grabRotateTransformer.Constraints.MaxAngle.Value = maxAngle;
        grabRotateTransformer.Constraints.MinAngle.Value = minAngle;
    }

    void SetHandleAngle(float angle)
    {
        if (m_Handle != null)
        {
            m_Handle.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);
            SetRotateTransformerConstraints();
        }
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
        return Mathf.Lerp(minAngle, maxAngle, m_Value);
    }

    void OnValidate()
    {
        SetHandleAngle(GetValue());
    }

}
