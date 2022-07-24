using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S2CRedirector : SteerToTargetRedirector
{
    private const float S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE = 160;
    private const float S2C_TEMP_TARGET_DISTANCE = 4;

    private Vector3 centerPoint = Vector3.zero;

    public override void PickSteeringTarget() {
        //Vector2 trackingAreaPosition = Vector2.zero; // center must be zero in local space (정사각형인경우)
        Vector2 trackingAreaPosition = centerPoint;
        //Debug.Log(trackingAreaPosition.ToString("F3"));
        Vector2 userToCenter = trackingAreaPosition - userPosition;

        //Compute steering target for S2C
        float bearingToCenter = Vector2.Angle(userDirection, userToCenter);
        float directionToCenter = Mathf.Sign(Vector2.SignedAngle(userDirection, userToCenter)); // if target is to the left of the user, directionToTarget > 0

        if (bearingToCenter >= S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE) { 
            targetPosition = userPosition + S2C_TEMP_TARGET_DISTANCE * Utility.RotateVector2(userDirection, directionToCenter * 90);
        }
        else {
            targetPosition = trackingAreaPosition;
        }
    }

    public void SetCenterPoint(Vector3 _pos)
    {
        centerPoint = _pos;
    }

    public Vector3 GetCenterPoint()
    {
        return centerPoint;
    }
}
