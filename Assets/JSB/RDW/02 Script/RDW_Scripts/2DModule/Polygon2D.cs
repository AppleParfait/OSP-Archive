using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using UnityEngine;

public class Polygon2D : Object2D
{
    private List<Vector2> vertices; // local 좌표계 기준
    //private List<Vector2> flippedVertices; // local 좌표계 기준
    private Vector2 squareCenter; // local 좌표계 기준
    private Vector2 crossingAxisIntersection; // local 좌표계 기준
    private List<Vector2> crossBoundaryPoints; // local 좌표계 기준
    private List<Vector2> crossXFromTo; // local 좌표계 기준
    private List<Vector2> crossYFromTo; // local 좌표계 기준
    private List<float> crossingAxisRotationInfo = new List<float>(new float[] {0, 0});
    private Vector2 crossingPointMovementInfo;
    public List<Vector2> segmentedVertices; // APF용. // local 좌표계 기준
    public List<Vector2> segmentNormalVectors; // APF용. // local 좌표계 기준
    public List<float> segmentedEdgeLengths; // APF용.
    public List<Vector2> middleVertices; // APF용. // local 좌표계 기준
    public List<Vector2> edgeNormalVectors; // APF용. // local 좌표계 기준
    private int tileType;

    public Polygon2D() : base() // 기본 생성자
    {
        vertices = new List<Vector2>();
        vertices.Add(new Vector2(0.5f, 0.5f));
        vertices.Add(new Vector2(-0.5f, 0.5f));
        vertices.Add(new Vector2(-0.5f, -0.5f));
        vertices.Add(new Vector2(0.5f, -0.5f));

        SetSquareProperties();
        this.crossBoundaryPoints = CalculateCrossBoundaryPoints();
    }

    public Polygon2D(Polygon2D otherObject, string name = null) : base(otherObject, name) // 복사 생성자
    {
        this.vertices = new List<Vector2>(otherObject.vertices);
        SetSquareProperties();
        this.crossBoundaryPoints = CalculateCrossBoundaryPoints();
    }

    public void CC(ref List<Vector2> vertices)
    {
        this.vertices.Clear();

        foreach (var item in vertices)
        {
            this.vertices.Add(item);
        }

    }

    public Polygon2D(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale, Object2D parentObject = null, List<Vector2> vertices = null, int tileType = 0, List<float> rotationInfo = null, List<float> movementInfo = null) : base(prefab, name, localPosition, localRotation, localScale, parentObject) // vertex 위치를 직접 지정 하여 polygon을 생성하는 생성자
    {
        if(prefab == null)
            this.vertices = new List<Vector2>(vertices);

        if(rotationInfo != null && movementInfo != null && tileType != null)
        {
            this.crossingAxisRotationInfo = rotationInfo;
            this.tileType = tileType;
            if(tileType == 0 || tileType == 3)
            {
                this.crossingPointMovementInfo = new Vector2(movementInfo[0], movementInfo[1]);
            }
            else if(tileType == 1 || tileType == 2)
            {
                this.crossingPointMovementInfo = new Vector2(-movementInfo[0], -movementInfo[1]);
            }
        }

        SetSquareProperties();
        //this.crossBoundaryPoints = CalculateCrossBoundaryPoints();
    }

    public Polygon2D(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale, int count, float size, Object2D parentObject = null) : base(prefab, name, localPosition, localRotation, localScale, parentObject) // n각형과 size를 지정하여 polygon을 생성하는 방식 생성자
    {
        if(prefab == null) // TODO: 현재 4각형만 지원
        {
            vertices = new List<Vector2>();
            vertices.Add(new Vector2(size / 2, size / 2));
            vertices.Add(new Vector2(-size / 2, size / 2));
            vertices.Add(new Vector2(-size / 2, -size / 2));
            vertices.Add(new Vector2(size / 2, -size / 2));
        }
        SetSquareProperties();
        this.crossBoundaryPoints = CalculateCrossBoundaryPoints();
    }

    public Polygon2D(GameObject prefab) : base(prefab) // 참조 생성자
    {
        Mesh objectMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        Vector2[] projectedVertices = Utility.ProjectionVertices(objectMesh.vertices);
        Graph connectionGraph = Utility.GetConnectionGraph(projectedVertices, objectMesh.triangles);

        this.vertices = new List<Vector2>();
        this.vertices = connectionGraph.FindOutline(true);
        SetSquareProperties();
        this.crossBoundaryPoints = CalculateCrossBoundaryPoints();
    }

    public override Object2D Clone(string name = null)
    {
        Polygon2D copied = new Polygon2D(this, name);
        return copied;
    }

    public override string ToString()
    {
        if (vertices == null)
            return "";
        else
            return base.ToString() + string.Format("vertices: {0}\n", string.Join(",", vertices));
    }

    public List<Vector2> GetVertices()
    {
        return vertices;
    }

    public Edge2D GetEdge(int startIndex, Space relativeTo)
    {
        Vector2 p1 = GetVertex(startIndex, relativeTo);
        Vector2 p2 = GetVertex(startIndex + 1, relativeTo);

        return new Edge2D(p1, p2);
    }

    public Vector2 GetVertex(int index, Space relativeTo)
    {
        int realIndex = Utility.mod(index, vertices.Count);

        if (relativeTo == Space.Self)
            return vertices[realIndex];
        else
            return this.transform2D.TransformPointToGlobal(vertices[realIndex]);
    }

    public Vector2 GetInnerVertex(int index, float distance, Space relativeTo)
    {
        Vector2 directionToPrevious = (GetVertex(index - 1, relativeTo) - GetVertex(index, relativeTo)).normalized;
        Vector2 directionToNext = (GetVertex(index + 1, relativeTo) - GetVertex(index, relativeTo)).normalized;
        float currentInnerAngle = Vector2.SignedAngle(directionToPrevious, directionToNext);

        if (currentInnerAngle == 0 || currentInnerAngle == 180)
        {
            Vector2 directionToPreviousInner = (GetInnerVertex(index - 1, distance, relativeTo) - GetVertex(index, relativeTo));
            float sign = Mathf.Sign(Vector2.SignedAngle(directionToPrevious, directionToPreviousInner));
            Vector2 directionToMiddle = Utility.RotateVector2(directionToPrevious, sign * 90).normalized;

            distance *= 0.5f;
            return GetVertex(index, relativeTo) + directionToMiddle * Mathf.Abs(distance);
        }
        else
        {
            Vector2 directionToMiddle = ((directionToPrevious + directionToNext) / 2).normalized;
            if (currentInnerAngle < 0)
                directionToMiddle = -directionToMiddle;

            return GetVertex(index, relativeTo) + directionToMiddle * distance;
        }
    }

    public void SetSquareProperties()
    {
        Vector2 v_max = Vector2.zero;
        Vector2 v_min = Vector2.zero;
        Vector2 c = Vector2.zero;

        //Debug.Log(vertices.Count);
        for (int i = 0; i < vertices.Count; i++)
        {
            if(v_max.x <= vertices[i].x)
            {
                v_max.x = vertices[i].x;
            }
            if(v_min.x >= vertices[i].x)
            {
                v_min.x = vertices[i].x;
            }

            if(v_max.y <= vertices[i].y)
            {
                v_max.y = vertices[i].y;
            }
            if(v_min.y >= vertices[i].y)
            {
                v_min.y = vertices[i].y;
            }
        }

        c = 0.5f*(v_max + v_min);
        this.squareCenter = c;
        // Debug.Log("squareCenter: "+ squareCenter);
        this.crossingAxisIntersection = c + this.crossingPointMovementInfo;

        Vector2 diagonal = 3*(v_max - v_min);
        // Debug.Log("diagonal: "+ diagonal);

        Vector2 fromX = c + new Vector2(-(diagonal.magnitude/2),0);
        Vector2 toX = c + new Vector2((diagonal.magnitude/2),0);

        Vector2 fromY = c + new Vector2(0,-(diagonal.magnitude/2));
        Vector2 toY = c + new Vector2(0,(diagonal.magnitude/2));

        List<Vector2> crossXFromTo = new List<Vector2>();
        crossXFromTo.Add(this.crossingAxisIntersection + Utility.RotateVector2(fromX - c, this.crossingAxisRotationInfo[0]));
        crossXFromTo.Add(this.crossingAxisIntersection + Utility.RotateVector2(toX - c, this.crossingAxisRotationInfo[0]));

        List<Vector2> crossYFromTo = new List<Vector2>();
        crossYFromTo.Add(this.crossingAxisIntersection + Utility.RotateVector2(fromY - c, this.crossingAxisRotationInfo[1]));
        crossYFromTo.Add(this.crossingAxisIntersection + Utility.RotateVector2(toY - c, this.crossingAxisRotationInfo[1]));

        this.crossXFromTo = crossXFromTo;
        this.crossYFromTo = crossYFromTo;

        //float segNo = 1000f;
        //float segNo = 100f;
        float segNo = 20f;

        //Debug.Log("aa");

        List<float> segmentedEdgeLengths = new List<float>();
        for(int i = 0 ; i < vertices.Count; i++)
        {

            if(vertices.Count <= i+1)
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    segmentedEdgeLengths.Add( ((vertices[0] - vertices[vertices.Count-1])/segNo).magnitude);
                }
            }
            else
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    segmentedEdgeLengths.Add( ((vertices[i+1] - vertices[i])/segNo).magnitude);
                }
            }

        }

        List<Vector2> segmentedVertices = new List<Vector2>();
        for(int i = 0 ; i < vertices.Count; i++)
        {
            if(vertices.Count <= i+1)
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    float jFloat = (float) j;
                    segmentedVertices.Add((vertices[0] - vertices[vertices.Count-1])*jFloat/segNo + vertices[vertices.Count-1] - (vertices[0] - vertices[vertices.Count-1])/(2*segNo));
                }
            }
            else
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    float jFloat = (float) j;
                    segmentedVertices.Add((vertices[i+1] - vertices[i])*jFloat/segNo + vertices[i] - (vertices[i+1] - vertices[i])/(2*segNo));
                }
            }
        }

        List<Vector2> segmentNormalVectors = new List<Vector2>();
        for(int i = 0 ; i < vertices.Count; i++)
        {
            if(vertices.Count <= i+1)
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    segmentNormalVectors.Add(  Utility.RotateVector2( ( (vertices[0] - vertices[vertices.Count-1])/segNo ).normalized, -90)   );
                }
            }
            else
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    segmentNormalVectors.Add(  Utility.RotateVector2( ( (vertices[i+1] - vertices[i])/segNo ).normalized, -90)   );
                }
            }
        }

        List<Vector2> middleVertices = new List<Vector2>();
        for(int i = 0 ; i < vertices.Count; i++)
        {
            if(vertices.Count <= i+1)
            {
                middleVertices.Add((vertices[0] - vertices[vertices.Count-1])/2f + vertices[vertices.Count-1]);
            }
            else
            {
                middleVertices.Add((vertices[i+1] - vertices[i])/2f + vertices[i]);
            }
        }

        List<Vector2> edgeNormalVectors = new List<Vector2>();
        for(int i = 0 ; i < vertices.Count; i++)
        {
            if(vertices.Count <= i+1)
            {
                edgeNormalVectors.Add(  Utility.RotateVector2( ( (vertices[0] - vertices[vertices.Count-1]) ).normalized, -90)   );
            }
            else
            {
                edgeNormalVectors.Add(  Utility.RotateVector2( ( (vertices[i+1] - vertices[i]) ).normalized, -90)   );
            }
        }

        this.segmentedEdgeLengths = segmentedEdgeLengths;
        this.segmentedVertices = segmentedVertices;
        this.segmentNormalVectors = segmentNormalVectors;
        this.middleVertices = middleVertices;
        this.edgeNormalVectors = edgeNormalVectors;
    }

    public List<Vector2> CalculateCrossBoundaryPoints()
    {
        Vector2 cxp = this.crossingAxisIntersection;
        Vector2 fromX = this.crossXFromTo[0];
        Vector2 toX = this.crossXFromTo[1];
        Vector2 fromY = this.crossYFromTo[0];
        Vector2 toY = this.crossYFromTo[1];
        // Debug.Log("cxp: "+ cxp);
        // Debug.Log("fromX: "+ fromX);
        // Debug.Log("toX: "+ toX);
        // Debug.Log("fromY: "+ fromY);
        // Debug.Log("toY: "+ toY);

        List<Vector2> possibleCrossPoints1 = new List<Vector2>();
        List<Vector2> possibleCrossPoints2 = new List<Vector2>();
        List<Vector2> possibleCrossPoints3 = new List<Vector2>();
        List<Vector2> possibleCrossPoints4 = new List<Vector2>();

        List<Vector2> crossPoints = new List<Vector2>();
        Vector2 crossPoint1 = new Vector2();
        Vector2 crossPoint2 = new Vector2();
        Vector2 crossPoint3 = new Vector2();
        Vector2 crossPoint4 = new Vector2();

        int p=0;
        float eps = Mathf.Pow(10f, -10f);

        for (int i=0; i < vertices.Count; i++)
        {
            if (i-1 < 0)
            {
                p = vertices.Count - 1;
            }
            else
            {
                p = i-1;
            }

            Vector2 lineCrossPoint1 = GetIntersectionPointCoordinates(vertices[p], vertices[i], cxp, toX);
            if( (vertices[p].x - lineCrossPoint1.x)*(vertices[i].x - lineCrossPoint1.x) <= eps
             && (vertices[p].y - lineCrossPoint1.y)*(vertices[i].y - lineCrossPoint1.y) <= eps
             && (cxp.x - lineCrossPoint1.x)*(toX.x - lineCrossPoint1.x) <= eps
             && (cxp.y - lineCrossPoint1.y)*(toX.y - lineCrossPoint1.y) <= eps )
            {
                possibleCrossPoints1.Add(lineCrossPoint1);
                //Debug.Log(lineCrossPoint1);
            }

            Vector2 lineCrossPoint2 = GetIntersectionPointCoordinates(vertices[p], vertices[i], cxp, toY);
            if( (vertices[p].x - lineCrossPoint2.x)*(vertices[i].x - lineCrossPoint2.x) <= eps
             && (vertices[p].y - lineCrossPoint2.y)*(vertices[i].y - lineCrossPoint2.y) <= eps
             && (cxp.x - lineCrossPoint2.x)*(toY.x - lineCrossPoint2.x) <= eps
             && (cxp.y - lineCrossPoint2.y)*(toY.y - lineCrossPoint2.y) <= eps )
            {
                possibleCrossPoints2.Add(lineCrossPoint2);
                //Debug.Log(lineCrossPoint2);
            }

            Vector2 lineCrossPoint3 = GetIntersectionPointCoordinates(vertices[p], vertices[i], cxp, fromX);
            if( (vertices[p].x - lineCrossPoint3.x)*(vertices[i].x - lineCrossPoint3.x) <= eps
             && (vertices[p].y - lineCrossPoint3.y)*(vertices[i].y - lineCrossPoint3.y) <= eps
             && (fromX.x - lineCrossPoint3.x)*(cxp.x - lineCrossPoint3.x) <= eps
             && (fromX.y - lineCrossPoint3.y)*(cxp.y - lineCrossPoint3.y) <= eps )
            {
                possibleCrossPoints3.Add(lineCrossPoint3);
                //Debug.Log(lineCrossPoint3);
            }

            Vector2 lineCrossPoint4 = GetIntersectionPointCoordinates(vertices[p], vertices[i], cxp, fromY);
            if( (vertices[p].x - lineCrossPoint4.x)*(vertices[i].x - lineCrossPoint4.x) <= eps
             && (vertices[p].y - lineCrossPoint4.y)*(vertices[i].y - lineCrossPoint4.y) <= eps
             && (fromY.x - lineCrossPoint4.x)*(cxp.x - lineCrossPoint4.x) <= eps
             && (fromY.y - lineCrossPoint4.y)*(cxp.y - lineCrossPoint4.y) <= eps )
            {
                possibleCrossPoints4.Add(lineCrossPoint4);
                //Debug.Log(lineCrossPoint4);
            }
        }

        crossPoint1 = possibleCrossPoints1[0];
        for (int i = 0; i < possibleCrossPoints1.Count; i++)
        {
            if(possibleCrossPoints1[i].magnitude > crossPoint1.magnitude)
            {
                crossPoint1 = possibleCrossPoints1[i];
            }
        }

        crossPoint2 = possibleCrossPoints2[0];
        for (int i = 0; i < possibleCrossPoints2.Count; i++)
        {
            if(possibleCrossPoints2[i].magnitude > crossPoint2.magnitude)
            {
                crossPoint2 = possibleCrossPoints2[i];
            }
        }

        crossPoint3 = possibleCrossPoints3[0];
        for (int i = 0; i < possibleCrossPoints3.Count; i++)
        {
            if(possibleCrossPoints3[i].magnitude > crossPoint3.magnitude)
            {
                crossPoint3 = possibleCrossPoints3[i];
            }
        }

        crossPoint4 = possibleCrossPoints4[0];
        for (int i = 0; i < possibleCrossPoints4.Count; i++)
        {
            if(possibleCrossPoints4[i].magnitude > crossPoint4.magnitude)
            {
                crossPoint4 = possibleCrossPoints4[i];
            }
        }

        crossPoints.Add(crossPoint1);
        crossPoints.Add(crossPoint2);
        crossPoints.Add(crossPoint3);
        crossPoints.Add(crossPoint4);
        
        return crossPoints;
    }

    public Vector2 GetCrossBoundaryVertex(int index, Space relativeTo)
    {
        // int realIndex = Utility.mod(index, vertices.Count);

        if (relativeTo == Space.Self)
            return crossBoundaryPoints[index];
        else
            return this.transform2D.TransformPointToGlobal(crossBoundaryPoints[index]);
    }

    public List<Vector2> GetCrossBoundaryPoints()
    {
        return this.crossBoundaryPoints;
    }

    public void SetCrossBoundaryPoints(List<Vector2> crossBoundaryPoints)
    {
        this.crossBoundaryPoints = crossBoundaryPoints;
    }

    public List<Vector2> GetCrossXFromTo()
    {
        return this.crossXFromTo;
    }

    public List<Vector2> GetCrossYFromTo()
    {
        return this.crossYFromTo;
    }

    public Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)//, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
    
        if (tmp == 0)
        {
            // No solution!
            //found = false;
            return Vector2.zero;
        }
    
        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;
    
        //found = true;
    
        return new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );
    }

    public Vector2 GetSquareCenter()
    {
        return this.squareCenter;
    }

    public Vector2 GetCrossingAxisIntersection()
    {
        return this.crossingAxisIntersection;
    }

    public int GetTileType()
    {
        return this.tileType;
    }

    //public List<float> GetSegmentedEdgeLengths()
    //{
    //    return this.segmentedEdgeLengths;
    //}

    //public List<Vector2> GetSegmentedVertices()
    //{
    //    return this.segmentedVertices;
    //}

    //public List<Vector2> GetSegmentNormalVectors()
    //{
    //    return this.segmentNormalVectors;
    //}

    //public List<Vector2> GetMiddleVertices()
    //{
    //    return this.middleVertices;
    //}

    //public List<Vector2> GetEdgeNormalVectors()
    //{
    //    return this.edgeNormalVectors;
    //}

    public override void Initialize(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale, Transform parent)
    {
        base.Initialize(prefab, name, localPosition, localRotation, localScale, parent);

        if (prefab != null)
        {
            Mesh objectMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            Vector2[] projectedVertices = Utility.ProjectionVertices(objectMesh.vertices);
            Graph connectionGraph = Utility.GetConnectionGraph(projectedVertices, objectMesh.triangles);

            this.vertices = new List<Vector2>();
            this.vertices = connectionGraph.FindOutline(true);
        }
    }

    public override Mesh GenerateMesh(bool useOutNormal, float height)
    {
        Vector2[] vertices2DArray = new Vector2[this.vertices.Count];
        Vector3[] vertices3DArray = new Vector3[this.vertices.Count];

        for (int i = 0; i < this.vertices.Count; i++)
        {
            vertices2DArray[i] = this.vertices[i];
        }

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2DArray);
        int[] triangles = tr.Triangulate();

        for (int i = 0; i < this.vertices.Count; i++)
        {
            vertices3DArray[i] = Utility.CastVector2Dto3D(GetVertex(i, Space.Self));//this.vertices[i];
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices3DArray;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public override bool IsIntersect(Object2D targetObject) // global 좌표계로 변환시킨 후 비교 
    {
        if (targetObject is Polygon2D)
        {
            Polygon2D polygon = (Polygon2D)targetObject;

            for(int i=0; i< polygon.GetVertices().Count; i++)
            {
                Edge2D otherEdge = polygon.GetEdge(i, Space.World);

                if (this.IsIntersect(otherEdge, Space.World))
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
        else if (targetObject is Circle2D)
        {
            return targetObject.IsIntersect(this);
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }

    public override bool IsIntersect(Edge2D targetLine, Space relativeTo, string option = "default", float bound = 0.01f) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {
        int numOfIntersect = 0;

        for (int i = 0; i < vertices.Count; i++)
        {
            Edge2D boundary = GetEdge(i, relativeTo);

            if (boundary.CheckIntersect(targetLine, bound, option) == Intersect.EXIST)
                numOfIntersect += 1;
        }

        if (numOfIntersect == 0)
            return false;
        else
            return true;
    }

    public override int NumOfIntersect(Vector2 sourcePosition, Vector2 targetPosition, Space relativeTo, string option = "default", float bound = 0.01f) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {
        Edge2D targetLine = new Edge2D(sourcePosition, targetPosition);
        int numOfIntersect = 0;

        for (int i = 0; i < vertices.Count; i++)
        {
            Edge2D boundary = GetEdge(i, relativeTo);
            // Debug.Log("targetLine Vector1: " + targetLine.p1 );
            // Debug.Log("targetLine Vector2: " + targetLine.p2 );
            // Debug.Log("EdgeCheck Vector1: " + boundary.p1 );
            // Debug.Log("EdgeCheck Vector2: " + boundary.p2 );

            if (boundary.CheckIntersect(targetLine, bound, option) == Intersect.EXIST)
                numOfIntersect += 1;
        }
       
        return numOfIntersect;
    }

    public override bool IsInside(Object2D targetObject, float bound = 0) // global 좌표계로 변환시킨 후 비교
    {
        Vector2 globalTargetPosition = targetObject.transform2D.position;

        return this.IsInside(globalTargetPosition, Space.World, bound);
        //if (IsInside(globalTargetPosition, Space.World, bound))
        //{
        //    return !IsIntersect(targetObject);
        //}
        //else
        //{
        //    return false;
        //}
    }

    public override bool IsInside(Vector2 targetPoint, Space relativeTo, float bound = 0) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {
        Ray2D ray = new Ray2D(targetPoint, Vector2.right);
        int numOfIntersect = 0;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 p1 = GetInnerVertex(i, bound, relativeTo);
            Vector2 p2 = GetInnerVertex(i + 1, bound, relativeTo);

            //if (relativeTo == Space.World)
            //{
            //    p1 = this.transform2D.TransformPointToGlobal(p1);
            //    p2 = this.transform2D.TransformPointToGlobal(p2);
            //}

            Edge2D boundary = new Edge2D(p1, p2);

            if (boundary.CheckIntersect(ray, 0.001f) == Intersect.EXIST)
                numOfIntersect += 1;
        }


        if (numOfIntersect % 2 == 0)
            return false;
        else
            return true;
    }

    public override bool IsInsideTile(Vector2 targetPoint, Vector2 tileLocation, Space relativeTo, float bound = 0) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {

        Ray2D ray = new Ray2D(targetPoint, Vector2.right);
        int numOfIntersect = 0;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 p1 = GetInnerVertex(i, bound, relativeTo) + tileLocation;
            Vector2 p2 = GetInnerVertex(i + 1, bound, relativeTo) + tileLocation;

            //if (relativeTo == Space.World)
            //{
            //    p1 = this.transform2D.TransformPointToGlobal(p1);
            //    p2 = this.transform2D.TransformPointToGlobal(p2);
            //}

            Edge2D boundary = new Edge2D(p1, p2);

            if (boundary.CheckIntersect(ray, 0.001f) == Intersect.EXIST)
                numOfIntersect += 1;
        }


        if (numOfIntersect % 2 == 0)
            return false;
        else
            return true;
    }    

    public override void DebugDraw(Color color)
    {
        int n = this.vertices.Count;
        //Debug.Log(vertices[3]);
        for (int i = 0; i < n; i++)
        {
            Vector3 vec1 = Utility.CastVector2Dto3D(GetVertex(i, Space.World));
            Vector3 vec2 = Utility.CastVector2Dto3D(GetVertex(i+1, Space.World));
            Debug.DrawLine(vec1, vec2, color);
        }

        // Debug.Log(crossBoundaryPoints[0]);
        // Debug.Log(crossBoundaryPoints[1]);
        // Debug.Log(crossBoundaryPoints[2]);
        // Debug.Log(crossBoundaryPoints[3]);
        if(this.tileType != null)
        {
            Vector3 cp1 = Utility.CastVector2Dto3D(GetCrossBoundaryVertex(0, Space.World)); // 오
            Vector3 cp2 = Utility.CastVector2Dto3D(GetCrossBoundaryVertex(2, Space.World)); // 왼
            Vector3 cp3 = Utility.CastVector2Dto3D(GetCrossBoundaryVertex(1, Space.World)); // 위
            Vector3 cp4 = Utility.CastVector2Dto3D(GetCrossBoundaryVertex(3, Space.World)); // 아래
            if(this.tileType == 0 || this.tileType == 1)
            {
                Debug.DrawLine(cp1, cp2, color); // 오 - 왼
                Debug.DrawLine(cp3, cp4, color); // 위 - 아래
            }
            else if(this.tileType == 2 || this.tileType == 3)
            {
                Debug.DrawLine(cp3, cp4, color); // 위 - 아래
            }

        }


    }
}
