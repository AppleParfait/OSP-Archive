using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Circle2D : Object2D
{
    private float radius;

    public Circle2D() : base() // 기본 생성자
    {
        this.radius = 1.0f;
    }

    public Circle2D(Circle2D otherCircle, string name = null) : base(otherCircle, name) // 복사 생성자
    {
        this.radius = otherCircle.radius;
    }

    public Circle2D(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale, Object2D parentObject = null, float radius = 1) : base(prefab, name, localPosition, localRotation, localScale, parentObject) // 생성자
    {
        if (prefab == null)
            this.radius = radius;
    }

    public Circle2D(GameObject prefab) : base(prefab) // 참조 생성자
    {
        if (prefab.GetComponent<CapsuleCollider>() != null)
            this.radius = prefab.GetComponent<CapsuleCollider>().radius;
        else if (prefab.GetComponent<SphereCollider>() != null)
            this.radius = prefab.GetComponent<SphereCollider>().radius;
        else
            this.radius = 1.0f;
    }

    public float GetRadius()
    {
        return radius;
    }

    public override void Initialize(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale, Transform parent)
    {
        base.Initialize(prefab, name, localPosition, localRotation, localScale, parent);

        if (prefab != null)
        {
            if (prefab.GetComponent<CapsuleCollider>() != null)
                this.radius = prefab.GetComponent<CapsuleCollider>().radius;
            else if (prefab.GetComponent<SphereCollider>() != null)
                this.radius = prefab.GetComponent<SphereCollider>().radius;
            else
                this.radius = 1.0f;
        }
    }

    public override Object2D Clone(string name = null)
    {
        Circle2D copied = new Circle2D(this, name);
        return copied;
    }

    //public override Mesh GenerateMesh(bool useOutNormal)
    //{
    //    Vector3[] vertices = null;
    //    int[] triangles = null;
    //    Vector3[] normals = null;

    //    float thetaScale = 0.01f;
    //    float theta1 = 0f, theta2 = 0f;
    //    int size = (int)((1f / thetaScale));

    //    // TODO: Mesh Generate

    //    Mesh mesh = new Mesh();
    //    mesh.vertices = vertices;
    //    mesh.triangles = triangles;
    //    mesh.normals = normals;

    //    return mesh;

    //}


    public override bool IsIntersect(Object2D targetObject) // global 좌표계로 변환시킨 후 비교
    {
        if (targetObject is Circle2D)
        {
            Circle2D other = (Circle2D)targetObject;
            Vector2 otherPosition = other.transform2D.position; 
            Vector2 thisPosition = this.transform2D.position;
            float otherRadius = other.GetRadius();

            //if (Mathf.Abs(Vector2.Distance(thisPosition, otherPosition) - (otherRadius + this.radius)) < 0.02F) // 차이가 epsilon 만큼이라면 intersect 했다고 판단
            if (Mathf.Abs(Vector2.Distance(thisPosition, otherPosition) - (otherRadius + this.radius)) < 0.02F) // 차이가 epsilon 만큼이라면 intersect 했다고 판단
                return true;
            else
                return false;
        }
        else if (targetObject is Polygon2D)
        {
            Polygon2D polygon = (Polygon2D)targetObject;

            for (int i = 0; i < polygon.GetVertices().Count; i++)
            {
                Edge2D edge = polygon.GetEdge(i, Space.World);

                if (this.IsIntersect(edge, Space.World))
                    return true;
            }

            return false;
        }
        else if (targetObject is LineSegment2D)
        {
            LineSegment2D line = (LineSegment2D)targetObject;
            Edge2D targetLine = line.ChangeToEdge(Space.World);

            return this.IsIntersect(targetLine, Space.World);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public override bool IsIntersect(Edge2D targetLine, Space relativeTo, string option = "default", float bound = 0.01f) // line이 relativeTo 좌표계에 이미 있다고 가정
    {
        Vector2 origin = Vector2.zero;
        if (relativeTo == Space.Self)
            origin = this.transform2D.localPosition;
        else
            origin = this.transform2D.position;

        float originToP1Distance = Vector2.Distance(origin, targetLine.p1);
        float originToP2Distance = Vector2.Distance(origin, targetLine.p2);

        if (originToP1Distance - this.radius <= bound && originToP2Distance - this.radius > bound) // p1이 원 안에 있고 p2가 원 밖에 있는 경우
            return true;
        else if (originToP2Distance - this.radius <= bound && originToP1Distance - this.radius > bound) // p2가 원 안에 있고 p1이 원 밖에 있는 경우
            return true;
        else if (originToP2Distance - this.radius <= bound && originToP1Distance - this.radius <= bound) // p2, p1 모두 원 안에 있는 경우
            return false;
        else // p2, p1 모두 원 밖에 있는 경우
        {
            Vector2 p1ToOrigin = origin - targetLine.p1;
            Vector2 lineDirection = targetLine.GetDirection();
            float sign = Mathf.Sign(Vector2.SignedAngle(lineDirection, p1ToOrigin));
            Vector2 originToLineDirection = Utility.RotateVector2(lineDirection, -sign * 90);
            Vector2 end = origin + originToLineDirection * this.radius;

            Edge2D originToEnd = new Edge2D(origin, end);

            return targetLine.IsIntersect(originToEnd);
        }
    }

    public override bool IsInside(Object2D targetObject, float bound = 0) // global 좌표계로 변환시킨 후 비교
    {
        Vector2 globalTargetPosition = targetObject.transform2D.position;

        if (IsInside(globalTargetPosition, Space.World, bound))
        {
            return !IsIntersect(targetObject);
        }
        else
        {
            return false;
        }
    }

    public override bool IsInside(Vector2 targetPoint, Space relativeTo, float bound = 0) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {
        Vector2 origin = Vector2.zero;
        if (relativeTo == Space.Self)
            origin = this.transform2D.localPosition;
        else
            origin = this.transform2D.position;

        float originToTargetDistance = Vector2.Distance(origin, targetPoint);

        if (originToTargetDistance - this.radius <= 0.01f)
            return true;
        else
            return false;
    }


    public override void DebugDraw(Color color)
    {
        float thetaScale = 0.01f;
        float theta1 = 0f, theta2 = 0f;
        int size = (int)((1f / thetaScale));

        for (int i = 0; i < size; i++)
        {
            theta1 = (2.0f * Mathf.PI * thetaScale) * i;
            float x1 = radius * Mathf.Cos(theta1);
            float y1 = radius * Mathf.Sin(theta1);
            Vector2 p1 = new Vector2(x1, y1);

            theta2 = (2.0f * Mathf.PI * thetaScale) * (i + 1);
            float x2 = radius * Mathf.Cos(theta2);
            float y2 = radius * Mathf.Sin(theta2);
            Vector2 p2 = new Vector2(x2, y2);

            Vector3 vec1 = Utility.CastVector2Dto3D(this.transform2D.TransformPointToGlobal(p1));
            Vector3 vec2 = Utility.CastVector2Dto3D(this.transform2D.TransformPointToGlobal(p2));

            Debug.DrawLine(vec1, vec2, color);
        }
    }
}
