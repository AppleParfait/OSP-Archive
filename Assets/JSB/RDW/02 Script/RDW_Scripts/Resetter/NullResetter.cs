using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullResetter : Resetter
{
    public NullResetter() : base() { }

    public NullResetter(float translationSpeed, float rotationSpeed) : base(translationSpeed, rotationSpeed) { }

}
