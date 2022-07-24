//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Line2D {
//    Vector2 p1, p2;

//    public Line2D(Vector2 p1, Vector2 p2) {
//        this.p1 = p1;
//        this.p2 = p2;
//    }

//    public Line2D(Vector3 p1, Vector3 p2) {
//        this.p1 = Utility.Cast3Dto2D(p1);
//        this.p2 = Utility.Cast3Dto2D(p2);
//    }

//    public Line2D() : base() {
//        this.p1 = Vector2.zero;
//        this.p2 = Vector2.zero;
//    }

//    public override string ToString() {
//        return string.Format("p1: {0}, p2: {1}", p1, p2);
//    }

//    public bool IsIntersect(Line2D line, out Vector2 result) {
//        Vector2 p3 = line.p1, p4 = line.p2;
//        float under = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

//        if (under == 0) {
//            result = Vector2.positiveInfinity;
//            return false;
//        }

//        float t = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / under; // this line
//        float s = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / under; // other line

//        if (t < 0.0 || t > 1.0 || s < 0.0 || s > 1.0)
//        {
//            result = Vector2.positiveInfinity;
//            return false;
//        }


//        result = new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
//        return true;
//    }

//    public bool IsIntersect(Ray2D ray, out Vector2 result, string option = "default") {
//        Vector2 p3 = ray.origin, p4 = ray.origin + ray.direction;
//        float under = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

//        if (under == 0) {
//            result = Vector2.positiveInfinity;
//            return false;
//        }

//        float t = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / under; // this line
//        float s = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / under; // other ray

//        if(option == "exclude" && (t < 0.0 || t >= 1.0 || s <= 0.0)) // exclude last point of this line(p2)
//        {
//            result = Vector2.positiveInfinity;
//            return false;
//        }
//        else if (t < 0.0 || t > 1.0 || s < 0.0)
//        {
//            result = Vector2.positiveInfinity;
//            return false;
//        }

//        result = new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
//        return true;
//    }
//}