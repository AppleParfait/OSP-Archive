using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullRedirector : Redirector
{
    public override (GainType, float) ApplyRedirection(RedirectedUnit unit, Vector2 deltaPosition, float deltaRotation) // 안움직이는 Redirector
    {
        float degree = 0;
        GainType type = GainType.Undefined;

        return (type, degree);
    }
}
