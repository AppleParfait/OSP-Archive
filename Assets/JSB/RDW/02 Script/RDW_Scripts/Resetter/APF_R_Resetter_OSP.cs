using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APF_R_Resetter_OSP : RotationResetter
{
    Polygon2D realPolygonObject;
    List<Vector2> segmentedVertices = new List<Vector2>();
    List<Vector2> segmentNormalVectors = new List<Vector2>();
    List<float> segmentedEdgeLengths = new List<float>();
    List<Vector2> middleVertices;
    List<Vector2> edgeNormalVectors;

    private bool bInit = false;

    public APF_R_Resetter_OSP() : base()
    {
        // targetAngle = 180;
        // ratio = 2;
    }

    public APF_R_Resetter_OSP(float translationSpeed, float rotationSpeed) : base(translationSpeed, rotationSpeed)
    {
        // targetAngle = 180;
        // ratio = 2;
    }

    private Vector2 GetOldW(Object2D realUser, Space2D realSpace) //paper: Multi-User Redirected Walking and Resetting Using Artificial Potential Fields
    {
        // define some variables for redirection
        Transform2D realUserTransform = realUser.transform2D;
        Vector2 userPosition = realUserTransform.localPosition - 0.02f * realUserTransform.forward.normalized;
        Vector2 userDirection = realUserTransform.forward;


        int RedirectedUnitIndex = 0;
        for (int i = 0; i < RDWSimulationManager.instance.GetRedirectedUnits.Length; i++)
        {
            if (RDWSimulationManager.instance.GetRedirectedUnits[i].realUser == realUser)
            {
                RedirectedUnitIndex = i;
            }
        }

        List<Vector2> partitionedSpaceVertices = new List<Vector2>();

        _OSP.OSP_Agent.instance.dic_AreaSegmentsVertex.TryGetValue(RedirectedUnitIndex, out partitionedSpaceVertices);

        List<Vector2> partitionedSpaceVertices2 = new List<Vector2>(partitionedSpaceVertices);
        if (partitionedSpaceVertices2.Count == 0)
        {
            Debug.LogWarning("count 0");
            _OSP.OSP_Agent.instance.EndEpisode();
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



        middleVertices = realPolygonObject.middleVertices;
        edgeNormalVectors = realPolygonObject.edgeNormalVectors;

        if (partitionedSpaceVertices.Count != 0)
        {
            realPolygonObject.Destroy(0.01f);
        }


        //Polygon2D realPolygonObject = (Polygon2D)realSpace.spaceObject;

        //List<Vector2> middleVertices = realPolygonObject.GetMiddleVertices();
        //List<Vector2> edgeNormalVectors = realPolygonObject.GetEdgeNormalVectors();
        List<Vector2> dList = new List<Vector2>();
        List<float> inverseDList = new List<float>();
        float dAbsSum = 0;

        Vector2 w = Vector2.zero;
        Vector2 middleToUser = Vector2.zero;

        // dAbsSum과 dList, inverseDList를 우선 구함
        for (int i = 0; i < middleVertices.Count; i++)
        {
            middleToUser = userPosition - middleVertices[i];
            inverseDList.Add(1 / Vector2.Dot(edgeNormalVectors[i], middleToUser));
            dList.Add(Vector2.Dot(edgeNormalVectors[i], middleToUser) * (edgeNormalVectors[i]));

            if (Vector2.Dot(middleToUser, edgeNormalVectors[i]) > 0)
            {
                dAbsSum += Vector2.Dot(edgeNormalVectors[i], middleToUser);
            }
        }

        // w계산
        for (int i = 0; i < middleVertices.Count; i++)
        {
            middleToUser = userPosition - middleVertices[i];
            if (Vector2.Dot(middleToUser, edgeNormalVectors[i]) > 0)
            {
                w += dList[i] * inverseDList[i] * inverseDList[i] * dAbsSum;
            }
            else
            {
                ;// Do Nothing
            }
        }

        Vector3 temp = new Vector3(w.x, 0.0f, w.y);
        Debug.DrawLine(realUserTransform.transform.position, realUserTransform.transform.position + temp * 10000, Color.green, 2.0f);

        return w;
    }

    private Vector2 GetW(Object2D realUser, Space2D realSpace) //paper: Effects of Tracking Area Shape and Size on Artificial Potential Field Redirected Walking
    {
        const float C = 0.00897f;
        const float lambda = 2.656f;
        const float r = 7.5f;
        const float gamma = 3.091f;
        const float M = 15f;

        // define some variables for redirection
        Transform2D realUserTransform = realUser.transform2D;
        Vector2 userPosition = realUserTransform.localPosition - 0.02f * realUserTransform.forward.normalized;
        Vector2 userDirection = realUserTransform.forward;


        if (!bInit)
        {
            bInit = true;

            realPolygonObject = (Polygon2D)realSpace.spaceObject;
            //segmentedVertices = realPolygonObject.GetSegmentedVertices();
            //segmentNormalVectors = realPolygonObject.GetSegmentNormalVectors();
            //segmentedEdgeLengths = realPolygonObject.GetSegmentedEdgeLengths();
            segmentedVertices = realPolygonObject.segmentedVertices;
            segmentNormalVectors = realPolygonObject.segmentNormalVectors;
            segmentedEdgeLengths = realPolygonObject.segmentedEdgeLengths;
        }

        //Polygon2D realPolygonObject = (Polygon2D)realSpace.spaceObject;

        // 실제 포텐셜에 가까운 코드이나, 논문 내 APF-R 리셋 방식에 오류가 있는 것으로 보임.
        // 리셋 벽 부근의 실제 포텐셜은 저자 생각과는 달리 수직 방향임.
        // 현재는 여러개의 점전하와 같이 구현되어 벽 부근의 포텐셜이 매우 불규칙적임. 리셋은 APF-R을 사용하지 않는 것을 권장.

        //List<Vector2> segmentedVertices = realPolygonObject.GetSegmentedVertices();
        //List<Vector2> segmentNormalVectors = realPolygonObject.GetSegmentNormalVectors();
        //List<float> segmentedEdgeLengths = realPolygonObject.GetSegmentedEdgeLengths();
        List<Vector2> dList = new List<Vector2>();
        List<Vector2> dNormalizedList = new List<Vector2>();
        List<float> inverseDList = new List<float>();

        for (int i = 0; i < segmentedVertices.Count; i++)
        {
            dList.Add(userPosition - segmentedVertices[i]);
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

        return w;
    }

    public override string ApplyWallReset(Object2D realUser, Object2D virtualUser, Space2D realSpace)
    {
        Vector2 w = Vector2.zero;

        if (isFirst)
        {
            w = GetOldW(realUser, realSpace);
            targetAngle = Vector2.SignedAngle(realUser.transform2D.forward, w);

            //Debug.Log("w: " + w);

            realTargetRotation = Utility.RotateVector2(realUser.transform2D.forward, targetAngle);
            virtualTargetRotation = Utility.RotateVector2(virtualUser.transform2D.forward, 360);

            isFirst = false;

            float resetRotationGain = 0; // Reset 동작에는 Gain이 적용 되지 않음. 하지만 시뮬레이션에는 문제가 없을 것으로 보임. 실제는 Gain을 적용하여 설정해야 할 듯.

            if (targetAngle > 0)
            {
                resetRotationGain = Mathf.Abs((360f - Mathf.Abs(360f - targetAngle)) / (targetAngle));
                maxRotTime = Mathf.Abs(360f - targetAngle) / (rotationSpeed);
            }
            else
            {
                resetRotationGain = Mathf.Abs((360 - Mathf.Abs(360f + targetAngle)) / (targetAngle));
                maxRotTime = Mathf.Abs(360f + targetAngle) / (rotationSpeed);
            }
            remainRotTime = 0;
        }

        if (remainRotTime < maxRotTime)
        {
            if (targetAngle > 0)
            {
                realUser.transform2D.Rotate(-Mathf.Sign(360f - targetAngle) * rotationSpeed * Time.deltaTime);
            }
            else
            {
                realUser.transform2D.Rotate(Mathf.Sign(360f + targetAngle) * rotationSpeed * Time.deltaTime);
            }
            virtualUser.transform2D.Rotate(Mathf.Sign(-targetAngle) * (360 / maxRotTime) * Time.deltaTime);
            remainRotTime += Time.fixedDeltaTime;
        }
        else
        {
            Utility.SyncDirection(virtualUser, realUser, virtualTargetRotation, realTargetRotation);
            realUser.transform2D.localPosition = realUser.transform2D.localPosition + realUser.transform2D.forward * translationSpeed * Time.fixedDeltaTime;

            isFirst = true;
            return "WALL_RESET_DONE";
        }

        return "IDLE";
    }
}