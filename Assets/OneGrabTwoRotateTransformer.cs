using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneGrabTwoRotateTransformer : MonoBehaviour, ITransformer
{
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

    [System.Serializable]
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

    private float _relativeAngle1 = 0.0f;
    private float _relativeAngle2 = 0.0f;
    private float _constrainedRelativeAngle1 = 0.0f;
    private float _constrainedRelativeAngle2 = 0.0f;

    private IGrabbable _grabbable;
    private Vector3 _grabPositionInPivotSpace;
    private Pose _transformPoseInPivotSpace1;
    private Pose _transformPoseInPivotSpace2;

    private Pose _worldPivotPose;
    private Vector3 _previousVectorInPivotSpace1;
    private Vector3 _previousVectorInPivotSpace2;

    private Quaternion _localRotation;
    private float _startAngle1 = 0;
    private float _startAngle2 = 0;

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
        _transformPoseInPivotSpace1 = new Pose(worldPositionDelta, worldRotationDelta);

        Vector3 initialOffset = _worldPivotPose.rotation * _grabPositionInPivotSpace;
        Vector3 initialVector = Vector3.ProjectOnPlane(initialOffset, rotationAxis);
        _previousVectorInPivotSpace1 = Quaternion.Inverse(_worldPivotPose.rotation) * initialVector;

        _startAngle1 = _constrainedRelativeAngle1;
        _relativeAngle1 = _startAngle1;

        float parentScale = targetTransform.parent != null ? targetTransform.parent.lossyScale.x : 1f;
        _transformPoseInPivotSpace1.position /= parentScale;
    }

    public void UpdateTransform()
    {
        var grabPoint = _grabbable.GrabPoints[0];
        var targetTransform = _grabbable.Transform;

        Vector3 localAxis1 = Vector3.zero;
        localAxis1[(int)_rotationAxis1] = 1f;
        Vector3 localAxis2 = Vector3.zero;
        localAxis2[(int)_rotationAxis2] = 1f;
        
        _worldPivotPose = ComputeWorldPivotPose();
        Vector3 rotationAxis1 = _worldPivotPose.rotation * localAxis1;
        Vector3 rotationAxis2 = _worldPivotPose.rotation * localAxis2;

        // Project our positional offsets onto a plane with normal equal to the rotation axis
        Vector3 targetOffset = grabPoint.position - _worldPivotPose.position;

        float angle1 = Vector3.Dot(targetOffset, rotationAxis1);
        float angle2 = Vector3.Dot(targetOffset, rotationAxis2);

        Quaternion rotation1 = Quaternion.AngleAxis(angle1, rotationAxis1);
        Quaternion rotation2 = Quaternion.AngleAxis(angle2, rotationAxis2);

        targetTransform.rotation = _worldPivotPose.rotation * rotation1 * rotation2;
        //Vector3 targetVector1 = Vector3.ProjectOnPlane(targetOffset, rotationAxis1);
        //Vector3 targetVector2 = Vector3.ProjectOnPlane(targetOffset, rotationAxis2);

        //Vector3 previousVectorInWorldSpace1 =
        //    _worldPivotPose.rotation * _previousVectorInPivotSpace1;
        //Vector3 previousVectorInWorldSpace2 =
        //    _worldPivotPose.rotation * _previousVectorInPivotSpace2;

        //// update previous
        //_previousVectorInPivotSpace1 = Quaternion.Inverse(_worldPivotPose.rotation) * targetVector1;
        //_previousVectorInPivotSpace2 = Quaternion.Inverse(_worldPivotPose.rotation) * targetVector2;

        //float signedAngle1 =
        //    Vector3.SignedAngle(previousVectorInWorldSpace1, targetVector1, rotationAxis1);
        //float signedAngle2 =
        //    Vector3.SignedAngle(previousVectorInWorldSpace2, targetVector2, rotationAxis2);

        //_relativeAngle1 += signedAngle1;
        //_relativeAngle2 += signedAngle2;

        //_constrainedRelativeAngle1 = _relativeAngle1;
        //_constrainedRelativeAngle2 = _relativeAngle2;
        //if (Constraints.MinAngle.Constrain)
        //{
        //    _constrainedRelativeAngle1 = Mathf.Max(_constrainedRelativeAngle1, Constraints.MinAngle.Value);
        //    _constrainedRelativeAngle2 = Mathf.Max(_constrainedRelativeAngle2, Constraints.MinAngle.Value);
        //}
        //if (Constraints.MaxAngle.Constrain)
        //{
        //    _constrainedRelativeAngle1 = Mathf.Min(_constrainedRelativeAngle1, Constraints.MaxAngle.Value);
        //    _constrainedRelativeAngle2 = Mathf.Min(_constrainedRelativeAngle2, Constraints.MaxAngle.Value);
        //}

        //Quaternion deltaRotation1 = Quaternion.AngleAxis(_constrainedRelativeAngle1 - _startAngle1, rotationAxis1);
        //Quaternion deltaRotation2 = Quaternion.AngleAxis(_constrainedRelativeAngle2 - _startAngle2, rotationAxis1);

        //float parentScale = targetTransform.parent != null ? targetTransform.parent.lossyScale.x : 1f;

        //Pose transformDeltaInWorldSpace1 =
        //    new Pose(
        //        _worldPivotPose.rotation * (parentScale * _transformPoseInPivotSpace1.position),
        //        _worldPivotPose.rotation * _transformPoseInPivotSpace1.rotation);
        //Pose transformDeltaInWorldSpace2 =
        //    new Pose(
        //        _worldPivotPose.rotation * (parentScale * _transformPoseInPivotSpace2.position),
        //        _worldPivotPose.rotation * _transformPoseInPivotSpace2.rotation);

        //Pose transformDeltaRotated1 = new Pose(
        //    deltaRotation1 * transformDeltaInWorldSpace1.position,
        //    deltaRotation1 * transformDeltaInWorldSpace1.rotation);
        //Pose transformDeltaRotated2 = new Pose(
        //    deltaRotation2 * transformDeltaInWorldSpace2.position,
        //    deltaRotation2 * transformDeltaInWorldSpace2.rotation);

        //targetTransform.position = _worldPivotPose.position + transformDeltaRotated1.position + transformDeltaRotated2.position;
        //targetTransform.rotation = transformDeltaRotated1.rotation * transformDeltaRotated2.rotation;
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
