//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Animations;

//public class Plane2D : Shape2D
//{
//    List<Vector2> corners; // local 좌표계 기준

//    // TODO: N각형에 대한 Plane2D 생성자 구현

//    public Plane2D() : base()
//    {
//        corners = new List<Vector2>();
//    }

//    public Plane2D(Vector2 size, Transform2D transform)
//    : base(transform)
//    {

//        corners = new List<Vector2>();
//        corners.Add(new Vector2(size.x / 2, size.y / 2));
//        corners.Add(new Vector2(-size.x / 2, size.y / 2));
//        corners.Add(new Vector2(-size.x / 2, -size.y / 2));
//        corners.Add(new Vector2(size.x / 2, -size.y / 2));
//    }

//    public Plane2D(List<Vector2> corners, Vector2 localPosition, float localRotation = 0.0f, Vector2 localScale = new Vector2(), Transform2D parent = null) 
//        : base(localPosition, localRotation, localScale, parent) {
//        this.corners = new List<Vector2>(corners);
//    }

//    public Plane2D(Vector2 size, Vector2 localPosition, float localRotation = 0.0f, Vector2 localScale = new Vector2(), Transform2D parent = null) 
//        : base(localPosition, localRotation, localScale, parent) {

//        corners = new List<Vector2>();
//        corners.Add(new Vector2(size.x / 2, size.y / 2));
//        corners.Add(new Vector2(-size.x / 2, size.y / 2));
//        corners.Add(new Vector2(-size.x / 2, -size.y / 2));
//        corners.Add(new Vector2(size.x / 2, -size.y / 2));
//    }

//    public override string ToString()
//    {
//        string result = "";

//        foreach(Vector2 corner in corners)
//        {
//            result += (corner + transform.position).ToString();
//        }
//        return string.Format("corners: {0}", result);
//    }

//    public override bool IsInside(Vector2 point) { // point는 로컬 좌표계 기준
//        Ray2D ray = new Ray2D(point, Vector2.right);
//        int numOfIntersect = 0;

//        for (int i = 0; i < corners.Count; i++) {
//            Line2D boundary = new Line2D(corners[i], corners[(i + 1) % 4]);

//            Vector2 result;
//            if (boundary.IsIntersect(ray, out result, "exclude"))
//                numOfIntersect += 1;
//        }

//        if (numOfIntersect % 2 == 0)
//            return false;
//        else
//            return true;
//    }

    
//}
