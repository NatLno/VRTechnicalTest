using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OneGrabTwoRotateTransformer : MonoBehaviour, ITransformer
{
    public enum JoystickType
    {
        BothCircle,
        BothSquare,
        FrontBack,
        LeftRight,
    }
    
    [SerializeField]
    [Tooltip("The object that is visually grabbed and manipulated")]
    Transform m_Handle = null;

    [SerializeField]
    [Tooltip("The value of the joystick")]
    Vector2 m_Value = Vector2.zero;

    [Tooltip("Controls how the joystick moves")]
    [SerializeField]
    JoystickType m_JoystickMotion = JoystickType.BothCircle;

    [SerializeField]
    [Tooltip("Maximum angle the joystick can move")]
    [Range(1.0f, 90.0f)]
    float m_MaxAngle = 60.0f;

    [SerializeField]
    [Tooltip("Minimum amount the joystick must move off the center to register changes")]
    [Range(1.0f, 90.0f)]
    float m_DeadZoneAngle = 10.0f;

    [SerializeField]
    [Tooltip("Events to trigger when the joystick's x value changes")]
    UnityEvent m_OnValueChangeX = new UnityEvent();

    [SerializeField]
    [Tooltip("Events to trigger when the joystick's y value changes")]
    UnityEvent m_OnValueChangeY = new UnityEvent();

    public enum Axis
    {
        Right = 0,
        Up = 1,
        Forward = 2
    }

    private Transform _pivotTransform = null;

    public Transform Pivot => _pivotTransform != null ? _pivotTransform : transform;

    [SerializeField]
    private Axis _rotationAxis1 = Axis.Right;
    [SerializeField]
    private Axis _rotationAxis2 = Axis.Forward;

    public Axis RotationAxis1 => _rotationAxis1;
    public Axis RotationAxis2 => _rotationAxis2;

    [Serializable]
    public class OneGrabRotateConstraints
    {
        public FloatConstraint MinAngle;
        public FloatConstraint MaxAngle;
    }

    [SerializeField]
    private OneGrabRotateConstraints _constraints =
        new OneGrabRotateConstraints()
        {
            MinAngle = new FloatConstraint(),
            MaxAngle = new FloatConstraint()
        };

    public OneGrabRotateConstraints Constraints
    {
        get
        {
            return _constraints;
        }

        set
        {
            _constraints = value;
        }
    }

    private float _relativeAngle = 0.0f;
    private float _constrainedRelativeAngle = 0.0f;

    private IGrabbable _grabbable;
    private Vector3 _grabPositionInPivotSpace;
    private Pose _transformPoseInPivotSpace;

    private Pose _worldPivotPose;
    private Vector3 _previousVectorInPivotSpace;

    private Quaternion _localRotation;
    private float _startAngle = 0;

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
    }

    public Pose ComputeWorldPivotPose()
    {
        if (_pivotTransform != null)
        {
            return _pivotTransform.GetPose();
        }

        var targetTransform = _grabbable.Transform;

        Vector3 worldPosition = targetTransform.position;
        Quaternion worldRotation = targetTransform.parent != null
            ? targetTransform.parent.rotation * _localRotation
            : _localRotation;

        return new Pose(worldPosition, worldRotation);
    }

    public void BeginTransform()
    {
        var grabPoint = _grabbable.GrabPoints[0];
        var targetTransform = _grabbable.Transform;

        if (_pivotTransform == null)
        {
            _localRotation = targetTransform.localRotation;
        }

        Vector3 localAxis = Vector3.zero;
        localAxis[(int)_rotationAxis1] = 1f;

        _worldPivotPose = ComputeWorldPivotPose();
        Vector3 rotationAxis = _worldPivotPose.rotation * localAxis;

        Quaternion inverseRotation = Quaternion.Inverse(_worldPivotPose.rotation);

        Vector3 grabDelta = grabPoint.position - _worldPivotPose.position;
        // The initial delta must be non-zero between the pivot and grab location for rotation
        if (Mathf.Abs(grabDelta.magnitude) < 0.001f)
        {
            Vector3 localAxisNext = Vector3.zero;
            localAxisNext[((int)_rotationAxis1 + 1) % 3] = 0.001f;
            grabDelta = _worldPivotPose.rotation * localAxisNext;
        }

        _grabPositionInPivotSpace =
            inverseRotation * grabDelta;

        Vector3 worldPositionDelta =
            inverseRotation * (targetTransform.position - _worldPivotPose.position);

        Quaternion worldRotationDelta = inverseRotation * targetTransform.rotation;
        _transformPoseInPivotSpace = new Pose(worldPositionDelta, worldRotationDelta);

        Vector3 initialOffset = _worldPivotPose.rotation * _grabPositionInPivotSpace;
        Vector3 initialVector = Vector3.ProjectOnPlane(initialOffset, rotationAxis);
        _previousVectorInPivotSpace = Quaternion.Inverse(_worldPivotPose.rotation) * initialVector;

        _startAngle = _constrainedRelativeAngle;
        _relativeAngle = _startAngle;

        float parentScale = targetTransform.parent != null ? targetTransform.parent.lossyScale.x : 1f;
        _transformPoseInPivotSpace.position /= parentScale;
    }

    Vector3 GetLookDirection()
    {
        //Vector3 direction = m_Interactor.GetAttachTransform(this).position - m_Handle.position;
        //direction = transform.InverseTransformDirection(direction);
        //switch (m_JoystickMotion)
        //{
        //    case JoystickType.FrontBack:
        //        direction.x = 0;
        //        break;
        //    case JoystickType.LeftRight:
        //        direction.z = 0;
        //        break;
        //}

        //direction.y = Mathf.Clamp(direction.y, 0.01f, 1.0f);
        //return direction.normalized;
        return Vector3.zero;
    }

    public void UpdateTransform()
    {
        var lookDirection = GetLookDirection();

        // Get up/down angle and left/right angle
        var upDownAngle = Mathf.Atan2(lookDirection.z, lookDirection.y) * Mathf.Rad2Deg;
        var leftRightAngle = Mathf.Atan2(lookDirection.x, lookDirection.y) * Mathf.Rad2Deg;

        // Extract signs
        var signX = Mathf.Sign(leftRightAngle);
        var signY = Mathf.Sign(upDownAngle);

        upDownAngle = Mathf.Abs(upDownAngle);
        leftRightAngle = Mathf.Abs(leftRightAngle);

        var stickValue = new Vector2(leftRightAngle, upDownAngle) * (1.0f / m_MaxAngle);

        // Clamp the stick value between 0 and 1 when doing everything but circular stick motion
        if (m_JoystickMotion != JoystickType.BothCircle)
        {
            stickValue.x = Mathf.Clamp01(stickValue.x);
            stickValue.y = Mathf.Clamp01(stickValue.y);
        }
        else
        {
            // With circular motion, if the stick value is greater than 1, we normalize
            // This way, an extremely strong value in one direction will influence the overall stick direction
            if (stickValue.magnitude > 1.0f)
            {
                stickValue.Normalize();
            }
        }

        // Rebuild the angle values for visuals
        leftRightAngle = stickValue.x * signX * m_MaxAngle;
        upDownAngle = stickValue.y * signY * m_MaxAngle;

        // Apply deadzone and sign back to the logical stick value
        var deadZone = m_DeadZoneAngle / m_MaxAngle;
        var aliveZone = (1.0f - deadZone);
        stickValue.x = Mathf.Clamp01((stickValue.x - deadZone)) / aliveZone;
        stickValue.y = Mathf.Clamp01((stickValue.y - deadZone)) / aliveZone;

        // Re-apply signs
        stickValue.x *= signX;
        stickValue.y *= signY;

        SetHandleAngle(new Vector2(leftRightAngle, upDownAngle));
        SetValue(stickValue);
    }

    void SetValue(Vector2 value)
    {
        //m_Value = value;
        //m_OnValueChangeX.Invoke(m_Value.x);
        //m_OnValueChangeY.Invoke(m_Value.y);
    }

    void SetHandleAngle(Vector2 angles)
    {
        if (m_Handle == null)
            return;

        var xComp = Mathf.Tan(angles.x * Mathf.Deg2Rad);
        var zComp = Mathf.Tan(angles.y * Mathf.Deg2Rad);
        var largerComp = Mathf.Max(Mathf.Abs(xComp), Mathf.Abs(zComp));
        var yComp = Mathf.Sqrt(1.0f - largerComp * largerComp);

        m_Handle.up = (transform.up * yComp) + (transform.right * xComp) + (transform.forward * zComp);
    }

    public void EndTransform() { }

    #region Inject

    public void InjectOptionalPivotTransform(Transform pivotTransform)
    {
        _pivotTransform = pivotTransform;
    }

    public void InjectOptionalRotationAxis(Axis rotationAxis)
    {
        _rotationAxis1 = rotationAxis;
    }

    public void InjectOptionalConstraints(OneGrabRotateConstraints constraints)
    {
        _constraints = constraints;
    }

    #endregion
}
