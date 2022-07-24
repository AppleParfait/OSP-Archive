using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteerToTargetRedirector : GainRedirector
{
    
    private const float MOVEMENT_THRESHOLD = 0.2f; // meters per second. For 2. A linear movement rotation
    private const float MAXIMUM_LINEAR_MOVEMENT_ROTATION_RATE = 15f;
    private const float ROTATION_THRESHOLD = 1.5f; // degrees per second. For 3. An angular rotation
    private const float MAXIMUM_ANGULAR_ROTATION_RATE = 30f;
    private const float ANGLE_THRESHOLD_FOR_DAMPENING = 1f; // Angle threshold to apply dampening (degrees)
    private const float DISTANCE_THRESHOLD_FOR_DAMPENING = 1.25f; // Distance threshold to apply dampening (meters)
    private const float SMOOTHING_FACTOR = 0.125f; // Smoothing factor for redirection rotations

    private float previousMagnitude = 0f;

    protected Vector2 userPosition; // user localPosition
    protected Vector2 userDirection; // user local direction (localforward)
    protected Vector2 targetPosition; // steerting target localPosition

    public virtual void PickSteeringTarget() {}

    public override (GainType, float) ApplyRedirection(RedirectedUnit unit, Vector2 deltaPosition, float deltaRotation)
    {
        if (deltaPosition == Vector2.zero && deltaRotation == 0.0f)
        {
            return (GainType.Undefined, 0);
        }

        // define some variables for redirection
        Transform2D realUserTransform = unit.GetRealUser().transform2D;
        userPosition = realUserTransform.localPosition;
        userDirection = realUserTransform.forward;

        // pick a target to where user steer
        PickSteeringTarget();

        Vector2 userToTarget = targetPosition - userPosition;
        float angleToTarget = Vector2.Angle(userDirection, userToTarget);
        float distanceToTarget = userToTarget.magnitude;

        // control applied gains according to user and target
        float directionToTarget = Mathf.Sign(Vector2.SignedAngle(userDirection, userToTarget)); // if target is to the left of the user, directionToTarget > 0. 그냥 부호임. 왼쪽: 1 or 오른쪽: -1.
        float directionRotation = Mathf.Sign(deltaRotation); // If user is rotating to the left, directionRotation > 0. 그냥 부호임. 왼쪽: 1 or 오른쪽: -1.

        if (directionToTarget > 0)  // If the target is to the left of the user,
            curvatureGain = HODGSON_MIN_CURVATURE_GAIN;
        else
            curvatureGain = HODGSON_MAX_CURVATURE_GAIN;

        if (directionToTarget * directionRotation < 0) // if user rotates away from the target (if their direction are opposite),
            rotationGain = MIN_ROTATION_GAIN;
        else
            rotationGain = MAX_ROTATION_GAIN;

        // select the largest magnitude
        float rotationMagnitude = 0, curvatureMagnitude = 0;

        if (deltaPosition.magnitude > MOVEMENT_THRESHOLD)
        {
            curvatureMagnitude = Mathf.Rad2Deg * curvatureGain * deltaPosition.magnitude; // 2. A linear movement rotation rate. 여기에 delta T를 곱해야 Rotation이 됨.
            if(curvatureMagnitude > 0)
            {
                curvatureMagnitude = Mathf.Clamp(curvatureMagnitude, 0, MAXIMUM_LINEAR_MOVEMENT_ROTATION_RATE);
            }
            else
            {
                curvatureMagnitude = Mathf.Clamp(curvatureMagnitude, -MAXIMUM_LINEAR_MOVEMENT_ROTATION_RATE, 0);
            }
        }
        if (Mathf.Abs(deltaRotation) >= ROTATION_THRESHOLD)
        {
            rotationMagnitude = rotationGain * deltaRotation; // 3. An angular rotation rate. 여기에 delta T를 곱해야 Rotation이 됨.
            if(rotationMagnitude > 0)
            {
                rotationMagnitude = Mathf.Clamp(rotationMagnitude, 0, MAXIMUM_ANGULAR_ROTATION_RATE);
            }
            else
            {
                rotationMagnitude = Mathf.Clamp(rotationMagnitude, -MAXIMUM_ANGULAR_ROTATION_RATE, 0);
            }
        }
            
        float selectedMagnitude = Mathf.Max(Mathf.Abs(rotationMagnitude), Mathf.Abs(curvatureMagnitude)); // selectedMagnitude is ABS(절대값)
        bool isCurvatureSelected = Mathf.Abs(curvatureMagnitude) > Mathf.Abs(rotationMagnitude);

        // dampening 
        if (angleToTarget <= ANGLE_THRESHOLD_FOR_DAMPENING)
            selectedMagnitude *= Mathf.Sin(Mathf.Deg2Rad * 90 * angleToTarget / ANGLE_THRESHOLD_FOR_DAMPENING);
         // 지운 이유: 시뮬레이션 회전 자체가 느려서 보는방향 직선이 중앙을 지나면 방향이 고정되는 효과. 정상동작은 함. 실제구현에서는 넣어줘야할 듯.
        if (distanceToTarget <= DISTANCE_THRESHOLD_FOR_DAMPENING)
        {
            selectedMagnitude *= distanceToTarget / DISTANCE_THRESHOLD_FOR_DAMPENING;
        }

        //smoothing
        float finalRotation = (1.0f - SMOOTHING_FACTOR) * previousMagnitude + SMOOTHING_FACTOR * selectedMagnitude;
        previousMagnitude = finalRotation;

        // apply final redirection
        if (!isCurvatureSelected)
        {
            float direction = directionRotation;
            return (GainType.Rotation, finalRotation * direction);
        }
        else
        {
            float direction = -Mathf.Sign(curvatureGain);
            return (GainType.Curvature, finalRotation * direction);
        }
    }

}
