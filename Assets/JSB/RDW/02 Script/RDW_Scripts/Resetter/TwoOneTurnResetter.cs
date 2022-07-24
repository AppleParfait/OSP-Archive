using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoOneTurnResetter : RotationResetter
{
    public TwoOneTurnResetter() : base() {
        targetAngle = 180;
        ratio = 2;
    }

    public TwoOneTurnResetter(float translationSpeed, float rotationSpeed) : base(translationSpeed, rotationSpeed)
    {
        targetAngle = 180;
        ratio = 2;
    }
}
