using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounds2D
{
    public Bounds bounds;

    public Bounds2D(Bounds bounds)
    {
        this.bounds = bounds;
    }

    public Bounds2D(Vector2 center, Vector2 size)
    {
        bounds = new Bounds(Utility.CastVector2Dto3D(center), Utility.CastVector2Dto3D(size));
    }

    public Vector2 center
    {
        get { return Utility.CastVector3Dto2D(bounds.center); }
        set
        {
            bounds.center = Utility.CastVector2Dto3D(value);
        }
    }

    public Vector2 extents
    {
        get { return Utility.CastVector3Dto2D(bounds.extents); }
        set
        {
            bounds.extents = Utility.CastVector2Dto3D(value);
        }
    }

    public Vector2 max
    {
        get { return Utility.CastVector3Dto2D(bounds.max); }
        set
        {
            bounds.max = Utility.CastVector2Dto3D(value);
        }
    }

    public Vector2 min
    {
        get { return Utility.CastVector3Dto2D(bounds.min); }
        set
        {
            bounds.min = Utility.CastVector2Dto3D(value);
        }
    }

    public Vector2 size
    {
        get { return Utility.CastVector3Dto2D(bounds.size); }
        set
        {
            bounds.size = Utility.CastVector2Dto3D(value);
        }
    }

    public bool Contains(Vector2 point)
    {
        return bounds.Contains(Utility.CastVector2Dto3D(point));
    }
}
