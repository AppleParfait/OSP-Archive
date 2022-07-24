using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RotationResetter : Resetter
{
    public static int testInt = 1;
    protected float targetAngle;
    protected float ratio;
    protected float maxRotTime, remainRotTime;
    protected Vector2 realTargetRotation, virtualTargetRotation;

    public RotationResetter() : base() {
    }

    public RotationResetter(float translationSpeed, float rotationSpeed) : base(translationSpeed, rotationSpeed)
    {
    }

    public override string ApplyWallReset(Object2D realUser, Object2D virtualUser, Space2D realSpace) {
        if (isFirst)
        {
            realTargetRotation = Matrix3x3.CreateRotation(targetAngle) * realUser.transform2D.forward;
            virtualTargetRotation = Matrix3x3.CreateRotation(ratio * targetAngle) * virtualUser.transform2D.forward;
            isFirst = false;

            maxRotTime = Mathf.Abs(targetAngle) / rotationSpeed;
            remainRotTime = 0;
        }

        if (remainRotTime < maxRotTime)
        {
            realUser.transform2D.Rotate(rotationSpeed * Time.deltaTime);
            virtualUser.transform2D.Rotate(ratio * rotationSpeed * Time.deltaTime);
            remainRotTime += Time.fixedDeltaTime;
        }
        else
        {
            Utility.SyncDirection(virtualUser, realUser, virtualTargetRotation, realTargetRotation);
            while(!realSpace.IsInside(realUser.transform2D.localPosition, Space.Self, 0.00001f).Item1) realUser.transform2D.localPosition = realUser.transform2D.localPosition + realUser.transform2D.forward * translationSpeed * Time.fixedDeltaTime;

            isFirst = true;
            return "WALL_RESET_DONE";
        }

        return "IDLE";
    }
}
