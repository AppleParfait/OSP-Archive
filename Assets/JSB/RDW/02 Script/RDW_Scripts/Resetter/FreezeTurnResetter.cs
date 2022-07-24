using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeTurnResetter : RotationResetter
{
    public FreezeTurnResetter() : base() {
        targetAngle = 180;
        ratio = 0;
    }

    public FreezeTurnResetter(float translationSpeed, float rotationSpeed) : base(translationSpeed, rotationSpeed)
    {
        targetAngle = 180;
        ratio = 0;
    }
}
