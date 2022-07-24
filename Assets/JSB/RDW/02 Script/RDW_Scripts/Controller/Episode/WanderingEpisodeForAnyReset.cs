using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class WanderingEpisodeForAnyReset : Episode
{
    private int count;
    private int emergencyExitCount;
    private bool emergencyExit = false;
    private Vector2 previousUserPosition;
    private Vector2 currentTileLocationVector = new Vector2(0f,0f);
    private Vector2 nextTileLocationVector = new Vector2(0f,0f);
    private Vector2 restoreVector = new Vector2(0f,0f);
    public Vector2 resetPoint = new Vector2(0f,0f);
    public string resetType = "";

    private int currentTileNumber;
    private int Horizontal;
    private int Vertical;
    private int currentHorizontal;
    private int currentVertical;
    public bool skipBit = false;
    public bool resetMode = false;
    public bool pathRestoreMode = false;
    public bool syncMode = false;
    private float virtualSpaceBound;
    private float intersectionBound;

    private Vector2 a;
    private Vector2 b;
    private Vector2 c;
    private Vector2 d;

    private Vector2 rightPoint1;
    private Vector2 topPoint1;
    private Vector2 leftPoint1;
    private Vector2 bottomPoint1;

    private Vector2 rightPoint2;
    private Vector2 topPoint2;
    private Vector2 leftPoint2;
    private Vector2 bottomPoint2;

    private Vector2 globalUserPosition;
    private Vector2 globalSamplingPosition;
    private Vector2 globalVirtualSpacePosition;

    private bool predefinedMode = false;
    private string filePath;
    private TextReader reader;
    private List<Vector2> targetPositionList;

    public WanderingEpisodeForAnyReset() : base()
    {
        GetPreDefinedTargetFile("Test1000");
    }

    public WanderingEpisodeForAnyReset(int episodeLength) : base(episodeLength)
    {
        GetPreDefinedTargetFile("Test1000");
    }

    public override Vector2 GetTarget(Transform2D virtualUserTransform, Space2D virtualSpace, Object2D virtualUser)
    {
        if (!currentTargetPosition.HasValue)
        {
            GenerateEpisode(virtualUserTransform, virtualSpace, virtualUser);
            if(targetPrefab != null && showTarget && !resetMode && !pathRestoreMode) InstaniateTarget();
            else if(targetPrefab != null && showTarget && (resetMode || pathRestoreMode || syncMode) )
            {
                InstaniateTarget(restoreVector);
            }
        }

        return currentTargetPosition.Value;
    }

    public void GetPreDefinedTargetFile(string fileName)
    {
        targetPositionList = new List<Vector2>();
        filePath = "Assets/Resources/" + fileName +".txt";
        reader = File.OpenText(filePath);

        string line = null;
        while ((line = reader.ReadLine()) != null) {
            string[] num = line.Split(',');
            float x = float.Parse(num[0]);
            float y = float.Parse(num[1]);
            targetPositionList.Add(new Vector2(x, y));
        }
        reader.Close();

        if (this.episodeLength != targetPositionList.Count)
            this.episodeLength = targetPositionList.Count;
    }

    protected override void GenerateEpisode(Transform2D virtualUserTransform, Space2D virtualSpace, Object2D virtualUser)
    {
        Vector2 samplingPosition = Vector2.zero;
        Vector2 sampleForward = Vector2.zero;
        Vector2 userPosition = virtualUserTransform.localPosition;
        globalUserPosition = virtualUserTransform.position;
        globalVirtualSpacePosition = virtualSpace.parentSpaceObject.transform2D.position;
        //Debug.Log("globalUserPosition: " + globalUserPosition);
        //Debug.Log("globalVirtualSpacePosition: " + globalVirtualSpacePosition);
        
        count = 0;
        virtualSpaceBound = 0.2f;
        intersectionBound = 0.2f;

        if (GetCurrentEpisodeIndex() <= 1)
        {
            previousUserPosition = virtualAgentInitialPosition;
        }

        do
        {
            count++;
            
            if(true || predefinedMode)
            {
                samplingPosition = targetPositionList[currentEpisodeIndex];
                // if (currentEpisodeIndex == 287)
                // {
                //     Debug.Log("episodeLength: "+episodeLength);
                // }

                break;
            }

            currentTileNumber = Convert.ToInt32(virtualSpace.spaceObject.gameObject.name.Replace("tile_",""));
            Polygon2D currentTile = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber];
            currentTileLocationVector = currentTile.transform2D.localPosition;

            Horizontal = virtualSpace.tileAreaSetting[0];
            Vertical = virtualSpace.tileAreaSetting[1];

            currentVertical = (int) (currentTileNumber/(4*Horizontal));
            currentHorizontal = currentTileNumber % (4*Horizontal);

            float resetLength = 0.3f;

            // float angle = Utility.sampleNormal(0f, 18f, -180f, 180f);
            float angle = Utility.sampleUniform(-180.0f, 180.0f);
            float distance = 0.5f; // 
            //float distance = 1.5f; // 0.3f: Small Exploration,  1.5f: Large Exploration

            sampleForward = Utility.RotateVector2(virtualUserTransform.forward, angle);
            samplingPosition = userPosition + sampleForward * distance; // local 좌표계에서 절대 위치 기준
            globalSamplingPosition = globalUserPosition + sampleForward * distance; // local 좌표계에서 절대 위치 기준

            a = virtualSpace.tileCrossingVectors[0];
            b = virtualSpace.tileCrossingVectors[1];
            c = virtualSpace.tileCrossingVectors[2];
            d = virtualSpace.tileCrossingVectors[3];

            rightPoint1 = currentTileLocationVector + a;
            topPoint1 = currentTileLocationVector + b;
            leftPoint1 = currentTileLocationVector + c;
            bottomPoint1 = currentTileLocationVector + d;

            rightPoint2 = currentTileLocationVector - c;
            topPoint2 = currentTileLocationVector - d;
            leftPoint2 = currentTileLocationVector - a;
            bottomPoint2 = currentTileLocationVector - b;



            if(currentTileNumber % 4 == 0 ) // Type 1
            {
                // Debug.Log("Now Type 1");
                Polygon2D rightTile1 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber+1];
                Polygon2D topTile1 = null;
                Polygon2D bottomTile1 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber+2];
                Polygon2D leftTile1 = null;
                
                bool firstRow = false;
                bool firstColumn = false;
                if( currentTileNumber - 4*Horizontal + 2 < 0)
                {
                    firstRow = true;
                }
                else
                {
                    topTile1 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber-4*Horizontal+2];
                }

                if( currentTileNumber - 3 < 0)
                {
                    firstColumn = true;
                }
                else
                {
                    leftTile1 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber-3];
                }

                if(virtualSpace.spaceObjects[currentTileNumber + 1].IsInsideTile(samplingPosition, rightTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound)) //오른쪽 전환인 경우 V    
                {
                    Polygon2D nextTile = rightTile1;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber+1];
                    nextTileLocationVector = nextTile.transform2D.localPosition;
                    virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 1];
                    break;
                }
                if(!firstRow) // 위쪽 전환인 경우 X
                {
                    if(virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].IsInsideTile(samplingPosition, topTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound)) // 위쪽 전환인 경우 X
                    {
                        Polygon2D nextTile = topTile1;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber-4*Horizontal+2];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2];
                        break;
                    }
                }
                if(!firstColumn) // 왼쪽인 경우 X
                {
                    if(virtualSpace.spaceObjects[currentTileNumber - 3].IsInsideTile(samplingPosition, leftTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound) ) // 왼쪽인 경우 X
                    { 
                        Polygon2D nextTile = leftTile1;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber-3];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 3];
                        break;
                    }
                }
                if(virtualSpace.spaceObjects[currentTileNumber + 2].IsInsideTile(samplingPosition, bottomTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound) ) // 아랫쪽인 경우 V
                {
                    Polygon2D nextTile = bottomTile1;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber+2];
                    nextTileLocationVector = nextTile.transform2D.localPosition;
                    virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 2];
                    break;
                }

                // Debug.Log("Type 1 Check Done");

            }
            else if(currentTileNumber % 4 == 1) // Type 2
            {
                // Debug.Log("Now Type 2");
                Polygon2D rightTile2 = null;
                Polygon2D topTile2 = null;
                Polygon2D leftTile2 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber - 1];
                Polygon2D bottomTile2 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber + 2];

                bool firstRow = false;
                bool lastColumn = false;
                if( currentTileNumber - 4*Horizontal + 2 < 0)
                {
                    firstRow = true;
                }
                else
                {
                    topTile2 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2];
                }
                if( (currentTileNumber+3)/4 % Horizontal == 0 )
                {
                    lastColumn = true;
                }
                else
                {
                    rightTile2 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber+3];
                }

                if(!lastColumn) //오른쪽 전환인 경우 X
                {
                    if(virtualSpace.spaceObjects[currentTileNumber + 3].IsInsideTile(samplingPosition, rightTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound) )
                    {
                        Polygon2D nextTile = rightTile2;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber+3];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 3];
                        break;
                    }

                }
                if(!firstRow) // 위쪽 전환인 경우 X
                {
                    if(virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].IsInsideTile(samplingPosition, topTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound) )
                    {
                        Polygon2D nextTile = topTile2;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2];
                        break;
                    }
                }
                if(virtualSpace.spaceObjects[currentTileNumber - 1].IsInsideTile(samplingPosition, leftTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound) )
                {
                    Polygon2D nextTile = leftTile2;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber - 1];
                    nextTileLocationVector = nextTile.transform2D.localPosition;
                    virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 1];
                    break;
                }

                if(virtualSpace.spaceObjects[currentTileNumber + 2].IsInsideTile(samplingPosition, bottomTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound) )
                {
                    Polygon2D nextTile = bottomTile2;// (Polygon2D) virtualSpace.spaceObjects[currentTileNumber + 2];
                    nextTileLocationVector = nextTile.transform2D.localPosition;
                    virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 2];
                    break;
                }
                // Debug.Log("Type 2 Check Done");

            }
            else if(currentTileNumber % 4 == 2) // Type 3
            {
                // Debug.Log("Now Type 3");
                Polygon2D topTile3 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber - 2];
                Polygon2D bottomTile3 = null;

                bool lastRow = false;
                if( currentTileNumber + 4*Horizontal - 2 > 4*(Vertical*Horizontal-1) )
                {
                    lastRow = true;
                }
                else
                {
                    bottomTile3 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2];
                }

                if(virtualSpace.spaceObjects[currentTileNumber - 2].IsInsideTile(samplingPosition, topTile3.transform2D.localPosition, Space.Self, this.virtualSpaceBound) )
                {
                    Polygon2D nextTile = topTile3;// (Polygon2D) virtualSpace.spaceObjects[currentTileNumber - 2];
                    nextTileLocationVector = nextTile.transform2D.localPosition;
                    virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 2];
                    break;
                }
                if(!lastRow)
                {
                    if(virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2].IsInsideTile(samplingPosition, bottomTile3.transform2D.localPosition, Space.Self, this.virtualSpaceBound) ) // 아랫쪽인 경우 X
                    {
                        Polygon2D nextTile = bottomTile3;// (Polygon2D) virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2];
                        break;
                    }
                }
                // Debug.Log("Type 3 Check Done");
            }
            else if(currentTileNumber % 4 == 3) // Type 4
            {
                // Debug.Log("Now Type 4");
                Polygon2D topTile4 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber-2];
                Polygon2D bottomTile4 = null;

                bool lastRow = false;
                if( currentTileNumber + 4*Horizontal - 2 > 4*(Vertical*Horizontal-1) )
                {
                    lastRow = true;
                }
                else
                {
                    bottomTile4 = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber+4*Horizontal-2];
                }

                if(virtualSpace.spaceObjects[currentTileNumber - 2].IsInsideTile(samplingPosition, topTile4.transform2D.localPosition, Space.Self, this.virtualSpaceBound) ) // 위쪽 전환인 경우 V
                {
                    Polygon2D nextTile = topTile4;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber-2];
                    nextTileLocationVector = nextTile.transform2D.localPosition;
                    virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 2];
                    break;
                }
                if(!lastRow)
                {
                    if(virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2].IsInsideTile(samplingPosition, bottomTile4.transform2D.localPosition, Space.Self, this.virtualSpaceBound) ) // 아랫쪽인 경우 X
                    {
                        Polygon2D nextTile = bottomTile4;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber+4*Horizontal-2];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2];
                        break;
                    }
                }
                // Debug.Log("Type 4 Check Done");
            }
            



            if (count >= 50)
            {
                //angle = Utility.sampleUniform(90f, 270f);
                angle = Utility.sampleUniform(90f, 270f);
                count = 1;
                emergencyExitCount++;
                
                if(emergencyExitCount == 5)
                {
                    emergencyExit = true;
                    emergencyExitCount = 0;

                    Vector2 movedVector1 = previousUserPosition - userPosition;
                    Vector2 movedVector2 = userPosition - virtualAgentInitialPosition;
                    
                    if (movedVector1.magnitude < distance-0.001f && movedVector2.magnitude < distance - 0.001f) // 이전과 현재 위치가 같으면서 초기 위치일 때
                    {
                        SetWrongEpisode(true);
                        SetCurrentEpisodeIndex(GetEpisodeLength());
                        // Debug.Log("Wrong Initialized Episode!");
                        // sampleForward = Utility.RotateVector2(virtualUserTransform.forward, angle);
                        // samplingPosition = userPosition + sampleForward * distance; // local 좌표계에서 절대 위치 기준
                    }

                    break;
                }
            }

            bool insideNextTile = virtualSpace.IsInsideTile(samplingPosition, nextTileLocationVector, Space.Self, this.virtualSpaceBound);
            int numOfIntersect = virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", this.intersectionBound);
            // Debug.Log("samplingPosition: "+samplingPosition);
            // Debug.Log("insideNextTile: "+insideNextTile);
            // Debug.Log("numOfIntersect: "+numOfIntersect);
        //} while (!virtualSpace.IsInside(samplingPosition, Space.Self, 0.5f)); // !virtualSpace.IsPossiblePath(samplingPosition, userPosition, Space.Self, 0.2f)
        } while (  !(  virtualSpace.IsInsideTile(samplingPosition, nextTileLocationVector, Space.Self, this.virtualSpaceBound) && (virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", this.intersectionBound) == 1)  )
                && !(  virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) && (virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", this.intersectionBound) == 0) )
                 );

        if (emergencyExit)
        {
            emergencyExit = false;

            // float angle = Utility.sampleNormal(0f, 18f, -180f, 180f);
            // float distance = 0.2f;
            // Vector2 sampleForward = Utility.RotateVector2(virtualUserTransform.forward, angle);
            // samplingPosition = userPosition + sampleForward * distance; // local 좌표계에서 절대 위치 기준
            if(GetWrongEpisode())
            {
                currentTargetPosition = userPosition;
            }
            else
            {
                currentTargetPosition = previousUserPosition;
            }
            
        }
        else
        {
            count = 1;
            previousUserPosition = userPosition;
            currentTargetPosition = samplingPosition;
            // Debug.Log("Move to target: " + samplingPosition);
        }

        // Vector2 initialToTarget = previousUserPosition - virtualUserTransform.localPosition;
        // float InitialAngle = Vector2.SignedAngle(virtualUserTransform.forward, initialToTarget);
        // float initialDistance = Vector2.Distance(virtualUserTransform.localPosition, previousUserPosition);
        // float initialAngleDirection = Mathf.Sign(InitialAngle);

        // Vector2 virtualTargetDirection = Matrix3x3.CreateRotation(InitialAngle) * virtualUserTransform.forward; // target을 향하는 direction(forward)를 구함
        // Vector2 virtualTargetPosition = virtualUserTransform.localPosition + virtualTargetDirection * initialDistance; // target에 도달하는 position을 구함

        // float maxRotTime = Mathf.Abs(InitialAngle) / 500;
        // float maxTransTime = initialDistance / 4;
        // float remainRotTime = 0;
        // float remainTransTime = 0;

        // if (maxTransTime - remainTransTime > 0.06f)
        // {
        //     // Debug.Log(maxTransTime - remainTransTime);
        // }
    }
}
