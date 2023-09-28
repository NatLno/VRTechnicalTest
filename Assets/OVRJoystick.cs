using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Angle
{
    [SerializeField]
    [Range(0f, 1f)]
    private float _value;

    [SerializeField]
    [Range(-90.0f, 90.0f)]
    private float _maxAngle = 90.0f;
    [SerializeField]
    [Range(-90.0f, 90.0f)]
    private float _minAngle = -90.0f;

    public Angle(float value, float maxAngle, float minAngle)
    {
        _value = value;
        _maxAngle = maxAngle;
        _minAngle = minAngle;
    }

    public float value
    {
        get => _value;
        set => _value = value;
    }

    public float maxAngle
    {
        get => _maxAngle;
        set => _maxAngle = value;
    }

    public float minAngle
    {
        get => _minAngle;
        set => _minAngle = value;
    }
}

public class OVRJoystick : MonoBehaviour
{
    [SerializeField]
    private OneGrabRotateTransformer grabRightRotateTransformer;

    [SerializeField]
    private OneGrabRotateTransformer grabForwardRotateTransformer;

    [SerializeField]
    [Tooltip("The object that is visually grabbed and manipulated")]
    Transform m_Handle = null;

    [SerializeField]
    private Angle m_RightValue;
    [SerializeField]
    private Angle m_ForwardValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float GetNormalizedRightValue()
    {
        float rightAngle = m_Handle.localRotation.eulerAngles.x;
        if (rightAngle > 180f)
            rightAngle -= 360f;

        return Mathf.InverseLerp(m_RightValue.minAngle, m_RightValue.maxAngle, rightAngle);
    }
    
    float GetNormalizedForwardValue()
    {
        float forwardAngle = m_Handle.localRotation.eulerAngles.z;
        if (forwardAngle > 180f)
            forwardAngle -= 360f;

        return Mathf.InverseLerp(m_ForwardValue.minAngle, m_ForwardValue.maxAngle, forwardAngle);
    }

    void SetRotateTransformerConstraints()
    {
        float rightValue = GetRightValue();
        grabRightRotateTransformer.Constraints.MaxAngle.Value = m_RightValue.maxAngle - rightValue;
        grabRightRotateTransformer.Constraints.MinAngle.Value = m_RightValue.minAngle- rightValue;
        
        float forwardValue = GetForwardValue();
        grabForwardRotateTransformer.Constraints.MaxAngle.Value = m_ForwardValue.maxAngle - forwardValue;
        grabForwardRotateTransformer.Constraints.MinAngle.Value = m_ForwardValue.minAngle - forwardValue;
    }

    void SetHandleAngle(float rightAngle, float forwardAngle)
    {
        if (m_Handle != null)
            m_Handle.localRotation = Quaternion.Euler(rightAngle, 0.0f, forwardAngle);
    }

    void OnDrawGizmosSelected()
    {
        var angleStartPoint = transform.position;

        if (m_Handle != null)
            angleStartPoint = m_Handle.position;

        const float k_AngleLength = 0.25f;

        var angleRightMaxPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_RightValue.maxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
        var angleRightMinPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_RightValue.minAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
        
        var angleForwardMaxPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, m_ForwardValue.maxAngle) * Vector3.up) * k_AngleLength;
        var angleForwardMinPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, m_ForwardValue.minAngle) * Vector3.up) * k_AngleLength;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(angleStartPoint, angleRightMaxPoint);
        Gizmos.DrawLine(angleStartPoint, angleForwardMaxPoint);

        Gizmos.DrawLine(angleStartPoint, angleRightMinPoint);
        Gizmos.DrawLine(angleStartPoint, angleForwardMinPoint);
    }

    private float GetRightValue()
    {
        return Mathf.Lerp(m_RightValue.minAngle, m_RightValue.maxAngle, m_RightValue.value);
    }

    private float GetForwardValue()
    {
        return Mathf.Lerp(m_ForwardValue.minAngle, m_ForwardValue.maxAngle, m_ForwardValue.value);
    }

    void OnValidate()
    {
        SetHandleAngle(GetRightValue(), GetForwardValue());
        //SetRotateTransformerConstraints();
    }
}
