using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTangentPointCircle : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab1;

    [SerializeField]
    private GameObject prefab2;

    [SerializeField]
    private GameObject prefab3;


    // Start is called before the first frame update
    void Start()
    {
        //FuncA();
        //FuncB();
        FuncC(new Vector3(0.0f, 0.0f, 3.0f), Vector3.zero, Vector3.up, 90.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FuncA()
    {
        /*
         # Example values
        (Px, Py) = (5, 2)
        (Cx, Cy) = (1, 1)
        a = 2

        from math import sqrt, acos, atan2, sin, cos

        b = sqrt((Px - Cx)**2 + (Py - Cy)**2)  # hypot() also works here
        th = acos(a / b)  # angle theta
        d = atan2(Py - Cy, Px - Cx)  # direction angle of point P from C
        d1 = d + th  # direction angle of point T1 from C
        d2 = d - th  # direction angle of point T2 from C

        T1x = Cx + a * cos(d1)
        T1y = Cy + a * sin(d1)
        T2x = Cx + a * cos(d2)
        T2y = Cy + a * sin(d2)
         */

        Vector3 externalPoint = new Vector3(5.0f, 0.0f, 2.0f);
        Vector3 circleCenterlPoint = new Vector3(1.0f, 0.0f, 1.0f);
        float radius = 0.3f;
        float distanceBetP_C = Mathf.Sqrt(Mathf.Pow(externalPoint.x - circleCenterlPoint.x, 2) + Mathf.Pow(externalPoint.z - circleCenterlPoint.z, 2));
        float theta = Mathf.Acos(radius / distanceBetP_C);
        float d = Mathf.Atan2(externalPoint.z - circleCenterlPoint.z, externalPoint.x - circleCenterlPoint.x);
        float d1 = d + theta;
        float d2 = d - theta;
        Vector3 T1 = new Vector3(circleCenterlPoint.x + radius * Mathf.Cos(d1), 0.0f, circleCenterlPoint.z + radius * Mathf.Sin(d1));
        Vector3 T2 = new Vector3(circleCenterlPoint.x + radius * Mathf.Cos(d2), 0.0f, circleCenterlPoint.z + radius * Mathf.Sin(d2));

        List<GameObject> list_go = new List<GameObject>();

        list_go.Add(Instantiate(prefab1, externalPoint, Quaternion.identity));
        list_go.Add(Instantiate(prefab2, circleCenterlPoint, Quaternion.identity));
        list_go.Add(Instantiate(prefab3, T1, Quaternion.identity));
        list_go.Add(Instantiate(prefab3, T2, Quaternion.identity));

        //Debug.Log((list_go[0].transform.position - list_go[1].transform.position).magnitude + " // " + distanceBetP_C);
        Debug.Log((list_go[2].transform.position - list_go[1].transform.position).magnitude + " // " + distanceBetP_C);
        Debug.Log((list_go[3].transform.position - list_go[1].transform.position).magnitude + " // " + distanceBetP_C);
    }

    private void FuncB()
    {
        Vector3 User0Pos = new Vector3(2.0f, 0.0f, 2.0f);
        Vector3 User1Pos = new Vector3(4.0f, 0.0f, 4.0f);

        Vector3 Vertex0 = new Vector3(3.0f, 0.0f, 2.0f);
        Vector3 Vertex1 = new Vector3(1.0f, 0.0f, 4.0f);

        Vector3 intersection;
        Vector3 aDiff = User1Pos - User0Pos;
        Vector3 bDiff = Vertex1 - Vertex0;
        if (LineLineIntersection(out intersection, User0Pos, aDiff, Vertex0, bDiff))
        {
            float aSqrMagnitude = aDiff.sqrMagnitude;
            float bSqrMagnitude = bDiff.sqrMagnitude;

            if ((intersection - User0Pos).sqrMagnitude <= aSqrMagnitude
                 && (intersection - User1Pos).sqrMagnitude <= aSqrMagnitude
                 && (intersection - Vertex0).sqrMagnitude <= bSqrMagnitude
                 && (intersection - Vertex1).sqrMagnitude <= bSqrMagnitude)
            {
                // there is an intersection between the two segments and it is at intersection

                List<GameObject> list_go = new List<GameObject>();

                list_go.Add(Instantiate(prefab1, intersection, Quaternion.identity));
                list_go.Add(Instantiate(prefab2, User0Pos, Quaternion.identity));
                list_go.Add(Instantiate(prefab2, User1Pos, Quaternion.identity));
                list_go.Add(Instantiate(prefab3, Vertex0, Quaternion.identity));
                list_go.Add(Instantiate(prefab3, Vertex1, Quaternion.identity));
            }
        }
    }

    public bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    private Vector3 FuncC(Vector3 position, Vector3 center, Vector3 axis, float angle)
    {
        Vector3 point = Quaternion.AngleAxis(angle, axis) * (position - center);
        Vector3 resultVec3 = center + point;

        List<GameObject> list_go = new List<GameObject>();

        list_go.Add(Instantiate(prefab1, position, Quaternion.identity));
        list_go.Add(Instantiate(prefab2, center, Quaternion.identity));
        list_go.Add(Instantiate(prefab3, resultVec3, Quaternion.identity));

        return resultVec3;
    }

}
