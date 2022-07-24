using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSegment2D : Object2D
{
    public Vector2 p1, p2; // local 좌표 기준

    public LineSegment2D() : base() // 기본 생성자
    {
        this.p1 = Vector2.zero;
        this.p2 = Vector2.zero;
    }

    public LineSegment2D(LineSegment2D otherLineSegment, string name = null) : base(otherLineSegment, name) // 복사 생성자
    {
        this.p1 = otherLineSegment.p1;
        this.p2 = otherLineSegment.p2;
    }

    public LineSegment2D(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale, Object2D parentObject = null, Vector2? p1 = null, Vector2? p2 = null) : base(prefab, name, localPosition, localRotation, localScale, parentObject) // 생성자
    {
        if(p1.HasValue)
            this.p1 = p1.Value;
        if(p2.HasValue)
            this.p2 = p2.Value;
    }

    public LineSegment2D(GameObject prefab) : base(prefab) // 참조 생성자
    {
        // TODO: p1, p2를 어떻게 갱신할 것인가?
    }

    public override void Initialize(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale, Transform parent)
    {
        base.Initialize(prefab, name, localPosition, localRotation, localScale, parent);

        // TODO: prefab이 있을 때 p1, p2를 어떻게 갱신할 것인가?
    }

    public override Object2D Clone(string name = null)
    {
        LineSegment2D copied = new LineSegment2D(this, name);
        return copied;
    }

    public Edge2D ChangeToEdge(Space relativeTo)
    {
        if (relativeTo == Space.World)
            return new Edge2D(this.transform2D.TransformPointToGlobal(p1), this.transform2D.TransformPointToGlobal(p2));
        else
            return new Edge2D(p1, p2);
    }

    public Vector2 GetDirection(bool isStartP1)
    {
        if (isStartP1)
            return (p2 - p1).normalized;
        else
            return (p1 - p2).normalized;
    }

    public override string ToString()
    {
        return string.Format("p1: {0}, p2: {1}", p1, p2);
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
        if (targetObject is LineSegment2D)
        {
            LineSegment2D line = (LineSegment2D)targetObject;
            Edge2D targetLine = line.ChangeToEdge(Space.World);

            return this.IsIntersect(targetLine, Space.World);
        }
        else
        {
            return targetObject.IsIntersect(this);
        }
    }

    public override bool IsIntersect(Edge2D targetLine, Space relativeTo, string option = "default", float bound = 0.01f) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {
        Edge2D thisLine = this.ChangeToEdge(relativeTo);

        Intersect intersect = thisLine.CheckIntersect(targetLine, bound);

        if (intersect == Intersect.NONE)
            return false;
        else
            return true;
    }

    public override void DebugDraw(Color color)
    {
        Vector3 vec1 = Utility.CastVector2Dto3D(this.transform2D.TransformPointToGlobal(p1));
        Vector3 vec2 = Utility.CastVector2Dto3D(this.transform2D.TransformPointToGlobal(p2));

        Debug.DrawLine(vec1, vec2, color);
    }
}
