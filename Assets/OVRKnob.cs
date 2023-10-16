using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OVRKnob : MonoBehaviour
{
    [SerializeField]
    private OneGrabKnobTransformerIncrement grabRotateTransformer;

    [SerializeField]
    [Tooltip("The object that is visually grabbed and manipulated")]
    Transform m_Handle = null;

    [SerializeField]
    [Range(0f, 1f)]
    float m_Value;

    [SerializeField]
    float m_Increment;

    [SerializeField]
    [Tooltip("Angle of the lever in the 'on' position")]
    [Range(0f, 360.0f)]
    float m_MaxAngle = 360f;

    [SerializeField]
    [Tooltip("Angle of the lever in the 'off' position")]
    [Range(0f, 360.0f)]
    float m_MinAngle = 0f;

    [SerializeField]
    bool m_IsActivated = false;


    [SerializeField]
    UnityEvent m_OnStart = new UnityEvent();

    [SerializeField]
    [Tooltip("Events to trigger when the lever activates")]
    UnityEvent m_OnLeverActivate = new UnityEvent();

    [SerializeField]
    [Tooltip("Events to trigger when the lever deactivates")]
    UnityEvent m_OnLeverDesactivate = new UnityEvent();

    private float LeverDeadZone = 0.3f;
    private bool previousActivationState = false;
    public float value
    {
        get => m_Value;
        set => m_Value = value;
    }

    public float increment
    {
        get => m_Increment;
        set => m_Increment = value;
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

    public UnityEvent onLeverDesactivate => m_OnLeverDesactivate;


    private void Start()
    {
        onStart.Invoke();
        previousActivationState = isActivated;
        m_Value = (isActivated) ? 1f : 0f;
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
        if (GetNormalizedValue() >= 1f - LeverDeadZone && GetNormalizedValue() <= 1f)
            isActivated = true;
        else if (GetNormalizedValue() >= 0f && GetNormalizedValue() <= 0f + LeverDeadZone)
            isActivated = false;

        if (isActivated != previousActivationState)
        {
            if (isActivated)
                onLeverActivate.Invoke();
            else
                onLeverDesactivate.Invoke();

            previousActivationState = isActivated;
        }
    }

    float GetNormalizedValue()
    {
        float angle = m_Handle.localRotation.eulerAngles.y;
        return Mathf.InverseLerp(m_MinAngle, m_MaxAngle, angle);
    }

    void SetRotateTransformerConstraints()
    {
        float val = GetValue();
        grabRotateTransformer.Constraints.MaxAngle.Value = m_MaxAngle - val;
        grabRotateTransformer.Constraints.MinAngle.Value = m_MinAngle - val;

        grabRotateTransformer.IncrementAmount = ConvertIncrement();
    }

    void SetHandleAngle(float angle)
    {
        if (m_Handle != null)
            m_Handle.localRotation = Quaternion.Euler(0.0f, angle, 0.0f);
    }

    void OnDrawGizmosSelected()
    {
        var angleStartPoint = transform.position;

        if (m_Handle != null)
            angleStartPoint = m_Handle.position;

        const float k_AngleLength = 0.10f;

        var angleMaxPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, m_MaxAngle, 0.0f) * -Vector3.forward) * k_AngleLength;
        var angleMinPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, m_MinAngle, 0.0f) * -Vector3.forward) * k_AngleLength;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(angleStartPoint, angleMaxPoint);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(angleStartPoint, angleMinPoint);
    }

    private float GetValue()
    {
        return Mathf.Lerp(m_MinAngle, m_MaxAngle, m_Value);
    }
    private float ConvertIncrement()
    {
        return Mathf.Lerp(m_MinAngle, m_MaxAngle, m_Increment) - minAngle;
    }

    void OnValidate()
    {
        SetHandleAngle(GetValue());
        SetRotateTransformerConstraints();
    }
}
