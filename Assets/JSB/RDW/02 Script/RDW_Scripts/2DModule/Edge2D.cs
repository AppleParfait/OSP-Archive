using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge2D
{
    public Vector2 p1, p2;

    public Edge2D(Vector2 p1, Vector2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    public Edge2D(Vector3 p1, Vector3 p2)
    {
        this.p1 = Utility.CastVector3Dto2D(p1);
        this.p2 = Utility.CastVector3Dto2D(p2);
    }

    public Edge2D() : base()
    {
        this.p1 = Vector2.zero;
        this.p2 = Vector2.zero;
    }

    public override string ToString()
    {
        return string.Format("p1: {0}, p2: {1}", p1, p2);
    }
    
    public Vector2 GetDirection()
    {
        return (this.p1 - this.p2).normalized;
    }

    public bool IsIntersect(Edge2D otherLine, float bound = 0)
    {
        Intersect intersect = this.CheckIntersect(otherLine, bound);

        if (intersect == Intersect.NONE)
            return false;
        else
            return true;
    }

    public bool IsIntersect(Ray2D ray, float bound = 0)
    {
        Intersect intersect = this.CheckIntersect(ray, bound);

        if (intersect == Intersect.NONE)
            return false;
        else
            return true;
    }

    public Intersect CheckIntersect(Edge2D otherLine, float bound = 0, string option = "default")
    {
        // 두 Line2D는 같은 좌표계를 가지고 있다고 가정
        Vector2 p1 = this.p1, p2 = this.p2;
        Vector2 p3 = otherLine.p1, p4 = otherLine.p2;

        float under = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
        float _t = (p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x);
        float _s = (p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x);

        if (under == 0)
        {
            float term1 = (p3.y - p1.y) * (p4.x - p2.x) - (p3.x - p1.x) * (p4.y - p2.y);
            float term2 = (p4.y - p1.y) * (p3.x - p2.x) - (p4.x - p1.x) * (p3.y - p2.y);

            //if (Mathf.Abs(term1) <= 0.01f && Mathf.Abs(term2) <= 0.01f) // line과 ray가 서로 일치하는 경우
            //    return Intersect.INFINITY;
            if (Mathf.Abs(_t) <= 0.01f && Mathf.Abs(_s) <= 0.01f) // line과 ray가 서로 일치하는 경우
                return Intersect.INFINITY;
            else // 서로 평행하는 경우
                return Intersect.NONE;
        }

        // this line의 시작과 끝점은 t = 0, 1
        // other line의 시작과 끝점은 s = 0, 1
        float t = _t / under; // this line
        float s = _s / under; // other line

        if (option == "exclude") // other line의 시작점과 끝점에서 만나면 교차하지 않는 것으로 판단하는 경우
        {
            if ((t < 0.0 || t > 1.0 || s <= 0.0 || s >= 1.0))
            {
                return Intersect.NONE;
            }
        }
        else
        {
            if (t < 0.0 || t > 1.0 || s < 0.0 || s > 1.0)
            {
                return Intersect.NONE;
            }
        }

        //if (t < 0.0 || t > 1.0 || s < 0.0 || s > 1.0)
        //{
        //    return Intersect.NONE;
        //}

        return Intersect.EXIST;
    }

    public Intersect CheckIntersect(Ray2D ray, float bound = 0, string option = "default")
    {
        // Line2D과 Ray는 같은 좌표계를 가지고 있다고 가정
        Vector2 p3 = ray.origin, p4 = ray.origin + ray.direction;

        float under = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
        float _t = (p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x);
        float _s = (p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x);

        if (under == 0) // 일치하거나 평행하는 경우
        {
            //result = Vector2.positiveInfinity;
            float term1 = (p3.y - p1.y) * (p4.x - p2.x) - (p3.x - p1.x) * (p4.y - p2.y);
            float term2 = (p4.y - p1.y) * (p3.x - p2.x) - (p4.x - p1.x) * (p3.y - p2.y);

            //if (Mathf.Abs(term1) <= 0.01f && Mathf.Abs(term2) <= 0.01f) // line과 ray가 서로 일치하는 경우
            //    return Intersect.INFINITY;
            if (Mathf.Abs(_t) <= 0.01f && Mathf.Abs(_s) <= 0.01f) // line과 ray가 서로 일치하는 경우
                return Intersect.INFINITY;
            else // 서로 평행하는 경우
                return Intersect.NONE;
        }

        float t = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / under; // this line
        float s = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / under; // other ray

        // line의 시작과 끝점은 t = 0, 1
        // ray의 시작점은 s = 0

        if (option == "exclude") // ray의 시작점과 line의 끝점에서 만나면 교차하지 않는 것으로 판단하는 경우
        {
            if ((t < 0.0 || t >= 1.0 || s <= 0.0))
            {
                //result = Vector2.positiveInfinity;
                return Intersect.NONE;
            }
        }
        else
        {
            if (t < 0.0 || t > 1.0 || s < 0.0)
            {
                //result = Vector2.positiveInfinity;
                return Intersect.NONE;
            }
        }

        //result = new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
        return Intersect.EXIST;
    }

    public void DebugDraw(Color color)
    {
        Debug.DrawLine(Utility.CastVector2Dto3D(p1), Utility.CastVector2Dto3D(p2), color);
    }

    //public bool IsIntersect(Line2D line, out Vector2 result)
    //{
    //    Vector2 p3 = line.p1, p4 = line.p2;
    //    float under = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

    //    if (under == 0)
    //    {
    //        result = Vector2.positiveInfinity;
    //        return false;
    //    }

    //    float t = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / under; // this line
    //    float s = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / under; // other line

    //    if (t < 0.0 || t > 1.0 || s < 0.0 || s > 1.0)
    //    {
    //        result = Vector2.positiveInfinity;
    //        return false;
    //    }


    //    result = new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
    //    return true;
    //}

    //public bool IsIntersect(Ray2D ray, out Vector2 result, string option = "default")
    //{
    //    Vector2 p3 = ray.origin, p4 = ray.origin + ray.direction;
    //    float under = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

    //    if (under == 0)
    //    {
    //        result = Vector2.positiveInfinity;
    //        return false;
    //    }

    //    float t = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / under; // this line
    //    float s = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / under; // other ray

    //    if (option == "exclude" && (t < 0.0 || t >= 1.0 || s <= 0.0)) // exclude last point of this line(p2)
    //    {
    //        result = Vector2.positiveInfinity;
    //        return false;
    //    }
    //    else if (t < 0.0 || t > 1.0 || s < 0.0)
    //    {
    //        result = Vector2.positiveInfinity;
    //        return false;
    //    }

    //    result = new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
    //    return true;
    //}
}