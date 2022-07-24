using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class APFRedirector_OSP : GainRedirector
{
    private const float MOVEMENT_THRESHOLD = 0.2f; // meters per second. For 2. A linear movement rotation
    private const float MAXIMUM_LINEAR_MOVEMENT_ROTATION_RATE = 15f;
    private const float ROTATION_THRESHOLD = 1.5f; // degrees per second. For 3. An angular rotation
    private const float MAXIMUM_ANGULAR_ROTATION_RATE = 30f;
    private const float ANGLE_THRESHOLD_FOR_DAMPENING = 1f; // Angle threshold to apply dampening (degrees)
    private const float DISTANCE_THRESHOLD_FOR_DAMPENING = 1.25f; // Distance threshold to apply dampening (meters)
    private const float SMOOTHING_FACTOR = 0.125f; // Smoothing factor for redirection rotations

    private float previousMagnitude = 0f;

    protected Vector2 userPosition; // user localPosition
    protected Vector2 userDirection; // user local direction (localforward)
    protected Vector2 targetPosition; // steerting target localPosition

    Space2D realSpace;
    Polygon2D realPolygonObject;

    List<Vector2> segmentedVertices = new List<Vector2>();
    List<Vector2> segmentNormalVectors = new List<Vector2>();
    List<float> segmentedEdgeLengths = new List<float>();

    private bool bInit = false;


    public override (GainType, float) ApplyRedirection(RedirectedUnit unit, Vector2 deltaPosition, float deltaRotation)
    {
        if (deltaPosition == Vector2.zero && deltaRotation == 0.0f)
        {
            return (GainType.Undefined, 0);
        }

        // define some variables for redirection
        Transform2D realUserTransform = unit.GetRealUser().transform2D;
        userPosition = realUserTransform.localPosition;
        userDirection = realUserTransform.forward;

        Space2D realSpace = unit.GetRealSpace();
        Polygon2D realPolygonObject = (Polygon2D)realSpace.spaceObject;

        //PickSteeringTargetForAPF(userPosition, GetW(unit.GetRealUser(), realSpace));

        (Vector2 w, float t) = GetWandT(unit, realSpace);
        Vector2 userToTarget = 10000 * w;//targetPosition - userPosition;

        ///Debug line W
        Vector3 temp = new Vector3(userToTarget.x, 0.0f, userToTarget.y);
        Debug.DrawLine(realUserTransform.transform.position, realUserTransform.transform.position + temp, Color.yellow);
        //Debug.Log(userToTarget);
        float angleToTarget = Vector2.Angle(userDirection, userToTarget);
        float distanceToTarget = userToTarget.magnitude;

        // control applied gains according to user and target
        float directionToTarget = Mathf.Sign(Vector2.SignedAngle(userDirection, userToTarget)); // if target is to the left of the user, directionToTarget > 0. 그냥 부호임. 왼쪽: 1 or 오른쪽: -1.
        float directionRotation = Mathf.Sign(deltaRotation); // If user is rotating to the left, directionRotation > 0. 그냥 부호임. 왼쪽: 1 or 오른쪽: -1.

        if (directionToTarget > 0)  // If the target is to the left of the user,
            curvatureGain = HODGSON_MIN_CURVATURE_GAIN;
        else
            curvatureGain = HODGSON_MAX_CURVATURE_GAIN;

        if (directionToTarget * directionRotation < 0) // if user rotates away from the target (if their direction are opposite),
            rotationGain = MIN_ROTATION_GAIN;
        else
            rotationGain = MAX_ROTATION_GAIN;

        // select the largest magnitude
        float rotationMagnitude = 0, curvatureMagnitude = 0;

        if (deltaPosition.magnitude > MOVEMENT_THRESHOLD)
        {
            curvatureMagnitude = Mathf.Rad2Deg * curvatureGain * deltaPosition.magnitude; // 2. A linear movement rotation rate. 여기에 delta T를 곱해야 Rotation이 됨.

            if (curvatureMagnitude > 0)
            {
                curvatureMagnitude = (1 - t) * curvatureMagnitude + t * MAXIMUM_LINEAR_MOVEMENT_ROTATION_RATE;
            }
            else
            {
                curvatureMagnitude = (1 - t) * curvatureMagnitude + t * (-MAXIMUM_LINEAR_MOVEMENT_ROTATION_RATE);
            }

            if (curvatureMagnitude > 0)
            {
                curvatureMagnitude = Mathf.Clamp(curvatureMagnitude, 0, MAXIMUM_LINEAR_MOVEMENT_ROTATION_RATE);
            }
            else
            {
                curvatureMagnitude = Mathf.Clamp(curvatureMagnitude, -MAXIMUM_LINEAR_MOVEMENT_ROTATION_RATE, 0);
            }

            //Debug.Log("curvatureMagnitude: "+curvatureMagnitude);
        }
        if (Mathf.Abs(deltaRotation) >= ROTATION_THRESHOLD)
        {
            rotationMagnitude = rotationGain * deltaRotation; // 3. An angular rotation rate. 여기에 delta T를 곱해야 Rotation이 됨.
            if (rotationMagnitude > 0)
            {
                rotationMagnitude = Mathf.Clamp(rotationMagnitude, 0, MAXIMUM_ANGULAR_ROTATION_RATE);
            }
            else
            {
                rotationMagnitude = Mathf.Clamp(rotationMagnitude, -MAXIMUM_ANGULAR_ROTATION_RATE, 0);
            }
        }

        float selectedMagnitude = Mathf.Max(Mathf.Abs(rotationMagnitude), Mathf.Abs(curvatureMagnitude)); // selectedMagnitude is ABS(절대값)
        bool isCurvatureSelected = Mathf.Abs(curvatureMagnitude) > Mathf.Abs(rotationMagnitude);

        // dampening 
        if (angleToTarget <= ANGLE_THRESHOLD_FOR_DAMPENING)
            selectedMagnitude *= Mathf.Sin(Mathf.Deg2Rad * 90 * angleToTarget / ANGLE_THRESHOLD_FOR_DAMPENING);
        // 지운 이유: 시뮬레이션 회전 자체가 느려서 보는방향 직선이 중앙을 지나면 방향이 고정되는 효과. 정상동작은 함. 실제구현에서는 넣어줘야할 듯.
        if (distanceToTarget <= DISTANCE_THRESHOLD_FOR_DAMPENING)
        {
            selectedMagnitude *= distanceToTarget / DISTANCE_THRESHOLD_FOR_DAMPENING;
        }

        //smoothing
        float finalRotation = (1.0f - SMOOTHING_FACTOR) * previousMagnitude + SMOOTHING_FACTOR * selectedMagnitude;
        previousMagnitude = finalRotation;

        // apply final redirection
        if (!isCurvatureSelected)
        {
            float direction = directionRotation;
            return (GainType.Rotation, finalRotation * direction);
        }
        else
        {
            float direction = -Mathf.Sign(curvatureGain);
            return (GainType.Curvature, finalRotation * direction);
        }



    }

    // private void PickSteeringTargetForAPF(Vector2 userPosition, Vector2 wDirection)
    // {
    //     this.targetPosition = userPosition + 100000*wDirection;
    //     Debug.Log(targetPosition);
    // }

    private (Vector2, float) GetWandT(RedirectedUnit unit, Space2D realSpace)
    {
        const float C = 0.00897f;
        const float lambda = 2.656f;
        const float r = 7.5f;
        const float gamma = 3.091f;
        const float M = 15f;

        Object2D realUser = unit.GetRealUser();

        // define some variables for redirection
        Transform2D realUserTransform = realUser.transform2D;
        Vector2 userPosition = realUserTransform.localPosition - 0.02f * realUserTransform.forward.normalized;

        ///realSpace = simulationSetting.realSpaceSetting.GetSpace();
        /// Object2D spaceObject = spaceObjectSetting.GetObject();
        ///    return new Polygon2DBuilder().SetName(name).SetPrefab(prefab).SetLocalPosition(position).SetLocalRotation(rotation).SetMode(useRegularPolygon).SetSize(size).SetCount(count).SetVertices(vertices).Build();
        /// return new Space2DBuilder().SetName(spaceObjectSetting.name).SetSpaceObject(spaceObject).SetObstacles(obstacles).Build();

        //Object2D spaceObject = new Object2D(RDWSimulationManager.instance.simulationSetting.realSpaceSetting.GetObject());

        //


        int RedirectedUnitIndex = 0;
        for (int i = 0; i < RDWSimulationManager.instance.GetRedirectedUnits.Length; i++)
        {
            if(RDWSimulationManager.instance.GetRedirectedUnits[i] == unit)
            {
                //Debug.Log("index " + i);
                RedirectedUnitIndex = i;
                break;
            }
        }

        List<Vector2> partitionedSpaceVertices = new List<Vector2>();

        _OSP.OSP_Agent.instance.dic_AreaSegmentsVertex.TryGetValue(RedirectedUnitIndex, out partitionedSpaceVertices);

        List<Vector2> partitionedSpaceVertices2 = new List<Vector2>(partitionedSpaceVertices);
        if (partitionedSpaceVertices2.Count == 0)
        {
            Debug.LogWarning("count 0");
            realPolygonObject = (Polygon2D)realSpace.spaceObject;
        }
        else
        {
            Object2D spaceObject = new Polygon2DBuilder().SetName("sPS_" + RedirectedUnitIndex).SetPrefab(null).SetLocalPosition(Vector2.zero).SetLocalRotation(0).SetMode(false).SetSize(1).SetCount(4).SetVertices(partitionedSpaceVertices).Build();

            List<Object2D> obstacles = new List<Object2D>();
            Space2D partitionedSpace = new Space2DBuilder().SetName("PS_" + RedirectedUnitIndex).SetSpaceObject(spaceObject).SetObstacles(obstacles).Build();
            //realSpace = RDWSimulationManager.simulationSetting.realSpaceSetting.GetSpace();
            //RDWSimulationManager.instance.simulationSetting.realSpaceSetting.spaceObjectSetting.vertices = ;
            partitionedSpace.spaceObject.GenerateShape(_OSP.OSP_Agent.instance.PartitionedSpaceMaterials[RedirectedUnitIndex], 3, false, "gPS_" + RedirectedUnitIndex);

            partitionedSpace.spaceObject.gameObject.transform.position = partitionedSpace.spaceObject.gameObject.transform.position + Vector3.up * 0.01f;

            realPolygonObject = (Polygon2D)partitionedSpace.spaceObject;
        }
      
        segmentedVertices = realPolygonObject.segmentedVertices;
        segmentNormalVectors = realPolygonObject.segmentNormalVectors;
        segmentedEdgeLengths = realPolygonObject.segmentedEdgeLengths;

        List<Vector2> dList = new List<Vector2>();
        List<float> dListMagnitude = new List<float>();
        List<Vector2> dNormalizedList = new List<Vector2>();
        List<float> inverseDList = new List<float>();

        if (partitionedSpaceVertices.Count != 0)
        {
            realPolygonObject.Destroy(0.02f);
        }

        //Debug.Log(realUser.gameObject.tag +  " count" + segmentedVertices.Count);

        for (int i = 0; i < segmentedVertices.Count; i++)
        {
            dList.Add(userPosition - segmentedVertices[i]);
            //Vector3 pos = new Vector3(segmentedVertices[i].x, 0, segmentedVertices[i].y);
            //Debug.DrawLine(pos, pos + (Vector3.up * 100), Color.red, Time.deltaTime);
        }

        for (int i = 0; i < dList.Count; i++)
        {
            dListMagnitude.Add(dList[i].magnitude);
        }

        for (int i = 0; i < dList.Count; i++)
        {
            dNormalizedList.Add(dList[i].normalized);
        }

        for (int i = 0; i < dList.Count; i++)
        {
            inverseDList.Add(Mathf.Pow(1 / dList[i].magnitude, lambda));
        }

        Vector2 w = Vector2.zero;
        for (int i = 0; i < segmentedVertices.Count; i++)
        {
            if (Vector2.Dot(segmentNormalVectors[i], dNormalizedList[i]) > 0)
            {
                w += C * segmentedEdgeLengths[i] * dNormalizedList[i] * inverseDList[i];
            }
            else
            {
                ;// Do Nothing
            }
        }

        return (w, 1 - dListMagnitude.Min() * Mathf.Abs(HODGSON_MAX_CURVATURE_GAIN));
    }

}