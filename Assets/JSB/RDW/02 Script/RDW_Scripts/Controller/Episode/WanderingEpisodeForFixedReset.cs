using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class WanderingEpisodeForFixedReset : Episode
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

    private string filePath;
    private List<Vector2> targetPositionList = new List<Vector2>();

    public WanderingEpisodeForFixedReset() : base() { }

    public WanderingEpisodeForFixedReset(int episodeLength) : base(episodeLength) { }

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

        if(virtualSpace.tileMode)
        {
            skipBit = false;
            currentTileNumber = Convert.ToInt32(virtualSpace.spaceObject.gameObject.name.Replace("tile_",""));
            Polygon2D currentTile = (Polygon2D) virtualSpace.spaceObjects[currentTileNumber];
            currentTileLocationVector = currentTile.transform2D.localPosition;

            Horizontal = virtualSpace.tileAreaSetting[0];
            Vertical = virtualSpace.tileAreaSetting[1];

            currentVertical = (int) (currentTileNumber/(4*Horizontal));
            currentHorizontal = currentTileNumber % (4*Horizontal);

            float resetLength = 0.3f;


            do
            {
                count++;

                // float angle = Utility.sampleNormal(0f, 18f, -180f, 180f);
                float angle = Utility.sampleUniform(-180.0f, 180.0f);
                float distance = 2f; // 0.5f
                //float distance = 1.5f; // 0.3f: Small Exploration,  1.5f: Large Exploration

                sampleForward = Utility.RotateVector2(virtualUserTransform.forward, angle);
                samplingPosition = userPosition + sampleForward * distance; // local 좌표계에서 절대 위치 기준
                globalSamplingPosition = globalUserPosition + sampleForward * distance; // local 좌표계에서 절대 위치 기준
                
                // Polygon2D currentTile1 = (Polygon2D) virtualSpace.spaceObjects[0];
                // Debug.Log("0 Tile localPosition: " + currentTile1.transform2D.localPosition);

                // Polygon2D currentTile2 = (Polygon2D) virtualSpace.spaceObjects[1];
                // Debug.Log("1 Tile localPosition: " + currentTile2.transform2D.localPosition);

                // Polygon2D currentTile3 = (Polygon2D) virtualSpace.spaceObjects[2];
                // Debug.Log("2 Tile localPosition: " + currentTile3.transform2D.localPosition);

                // Polygon2D currentTile4 = (Polygon2D) virtualSpace.spaceObjects[3];
                // Debug.Log("3 Tile localPosition: " + currentTile4.transform2D.localPosition);


                a = virtualSpace.tileCrossingVectors[0];
                b = virtualSpace.tileCrossingVectors[1];
                c = virtualSpace.tileCrossingVectors[2];
                d = virtualSpace.tileCrossingVectors[3];
                // Debug.Log("a: " + a);
                // Debug.Log("b: " + b);
                // Debug.Log("c: " + c);
                // Debug.Log("d: " + d);

                rightPoint1 = currentTileLocationVector + a;
                topPoint1 = currentTileLocationVector + b;
                leftPoint1 = currentTileLocationVector + c;
                bottomPoint1 = currentTileLocationVector + d;

                rightPoint2 = currentTileLocationVector - c;
                topPoint2 = currentTileLocationVector - d;
                leftPoint2 = currentTileLocationVector - a;
                bottomPoint2 = currentTileLocationVector - b;


                if(resetMode)
                {
                    if(resetType == "1R")
                    {
                        samplingPosition = resetPoint + new Vector2(resetLength, 0);
                        // currentTargetPosition = resetPoint + new Vector2(resetLength, 0);
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 1];
                        // Debug.Log("currentTargetPosition2: "+currentTargetPosition);
                        // Debug.Log("User local Position2: "+virtualUserTransform.localPosition);
                    }
                    else if(resetType == "2R")
                    {
                        samplingPosition = resetPoint + new Vector2(resetLength, 0);
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 3];
                    }
                    else if(resetType == "1T" || resetType == "2T")
                    {
                        samplingPosition = resetPoint + new Vector2(0, resetLength);
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2];
                    }
                    else if(resetType == "1L")
                    {
                        samplingPosition = resetPoint - new Vector2(resetLength, 0);
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 3];
                    }
                    else if(resetType == "2L")
                    {
                        samplingPosition = resetPoint - new Vector2(resetLength, 0);
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 1];
                    }
                    else if(resetType == "1B" || resetType == "2B")
                    {
                        samplingPosition = resetPoint - new Vector2(0, resetLength);
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 2];
                    }
                    else if(resetType == "3T" || resetType == "4T")
                    {
                        samplingPosition = resetPoint + new Vector2(0, resetLength);
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber - 2];
                    }
                    else if(resetType == "3B" || resetType == "4B")
                    {
                        samplingPosition = resetPoint - new Vector2(0, resetLength);
                        virtualSpace.spaceObject = virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2];
                    }

                    resetMode = false;
                    pathRestoreMode = true;
                    currentEpisodeIndex = currentEpisodeIndex - 2;
                    // Debug.Log("Position during reset: "+ userPosition);
                    // Debug.Log("Reset Point: "+ resetPoint);
                    
                    break;
                }
                if(pathRestoreMode)
                {
                    samplingPosition = restoreVector;
                    currentTargetPosition = restoreVector;
                    pathRestoreMode = false;
                    syncMode = true;
                    // currentEpisodeIndex = currentEpisodeIndex - 1;
                    // Debug.Log("Current Tile Location Vector: "+ currentTileLocationVector);
                    // Debug.Log("Position during path restore: "+ userPosition);
                    break;
                }

                // Debug.Log("Sampling Position: " + samplingPosition);
                // Debug.Log("Current Tile Number: " + currentTileNumber);

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

                    // Debug.Log("Type1 R Inside: "+virtualSpace.spaceObjects[currentTileNumber + 1].IsInsideTile(samplingPosition, rightTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                    // Debug.Log("Type1 R Intersect: "+virtualSpace.spaceObjects[currentTileNumber + 1].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                    if(virtualSpace.spaceObjects[currentTileNumber + 1].IsInsideTile(samplingPosition, rightTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                    && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + rightPoint1 - new Vector2(resetLength, 0), Space.World, "default", this.intersectionBound) == 0 
                    && virtualSpace.spaceObjects[currentTileNumber + 1].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + rightPoint1 + new Vector2(resetLength, 0), Space.World, "default", this.intersectionBound) == 0 ) //오른쪽 전환인 경우 V    
                    //if( rightPoint1.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) //오른쪽 전환인 경우 V
                    {
                        Polygon2D nextTile = rightTile1;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber+1];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        //Debug.Log("currentTileLocationVector: " + currentTileLocationVector);
                        //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                        restoreVector = samplingPosition;
                        resetPoint = rightPoint1;
                        samplingPosition = resetPoint - new Vector2(resetLength, 0);
                        resetMode = true;
                        resetType = "1R";
                        // Debug.Log("Move from Type 1 to Right");
                        break;
                    }
                    if(!firstRow) // 위쪽 전환인 경우 X
                    {
                        // Debug.Log("Type1 T Inside: "+virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].IsInsideTile(samplingPosition, topTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                        // Debug.Log("Type1 T Intersect: "+virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                        if(virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].IsInsideTile(samplingPosition, topTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                        && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + topPoint1 - new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0
                        && virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + topPoint1 + new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0 ) // 위쪽 전환인 경우 X
                        //if(topPoint1.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) // 위쪽 전환인 경우 X
                        {
                            Polygon2D nextTile = topTile1;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber-4*Horizontal+2];
                            nextTileLocationVector = nextTile.transform2D.localPosition;
                            //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                            restoreVector = samplingPosition;
                            resetPoint = topPoint1;
                            samplingPosition = resetPoint - new Vector2(0, resetLength);
                            resetMode = true;
                            resetType = "1T";
                            // Debug.Log("Move from Type 1 to Top");
                            break;
                        }
                    }
                    if(!firstColumn) // 왼쪽인 경우 X
                    {
                        // Debug.Log("Type1 L Inside: "+virtualSpace.spaceObjects[currentTileNumber - 3].IsInsideTile(samplingPosition, leftTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                        // Debug.Log("Type1 L Intersect: "+virtualSpace.spaceObjects[currentTileNumber - 3].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                        if(virtualSpace.spaceObjects[currentTileNumber - 3].IsInsideTile(samplingPosition, leftTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                        && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + leftPoint1 + new Vector2(resetLength, 0), Space.World, "default", this.intersectionBound) == 0
                        && virtualSpace.spaceObjects[currentTileNumber - 3].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + leftPoint1 - new Vector2(resetLength, 0), Space.World, "default", this.intersectionBound) == 0 ) // 왼쪽인 경우 X
                        //if(leftPoint1.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) // 왼쪽인 경우 X
                        { 
                            Polygon2D nextTile = leftTile1;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber-3];
                            nextTileLocationVector = nextTile.transform2D.localPosition;
                            //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);
                            
                            restoreVector = samplingPosition;
                            resetPoint = leftPoint1;
                            samplingPosition = resetPoint + new Vector2(resetLength, 0);
                            resetMode = true;
                            resetType = "1L";
                            // Debug.Log("Move from Type 1 to Left");
                            break;
                        }
                    }
                    // Debug.Log("Type1 B Inside: "+virtualSpace.spaceObjects[currentTileNumber + 2].IsInsideTile(samplingPosition, bottomTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                    // Debug.Log("Type1 B Intersect: "+virtualSpace.spaceObjects[currentTileNumber + 2].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                    if(virtualSpace.spaceObjects[currentTileNumber + 2].IsInsideTile(samplingPosition, bottomTile1.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                    && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + bottomPoint1 + new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0
                    && virtualSpace.spaceObjects[currentTileNumber + 2].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + bottomPoint1 - new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0 ) // 아랫쪽인 경우 V
                    //if( bottomPoint1.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) // 아랫쪽인 경우 V
                    {
                        Polygon2D nextTile = bottomTile1;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber+2];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                        restoreVector = samplingPosition;
                        resetPoint = bottomPoint1;
                        samplingPosition = resetPoint + new Vector2(0, resetLength);
                        resetMode = true;
                        resetType = "1B";
                        // Debug.Log("Move from Type 1 to Bottom");
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
                        // Debug.Log("Type2 R Inside: "+virtualSpace.spaceObjects[currentTileNumber + 3].IsInsideTile(samplingPosition, rightTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                        // Debug.Log("Type2 R Intersect: "+virtualSpace.spaceObjects[currentTileNumber + 3].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                        if(virtualSpace.spaceObjects[currentTileNumber + 3].IsInsideTile(samplingPosition, rightTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                        && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + rightPoint2 - new Vector2(resetLength, 0), Space.World, "default", this.intersectionBound) == 0
                        && virtualSpace.spaceObjects[currentTileNumber + 3].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + rightPoint2 + new Vector2(resetLength, 0), Space.World, "default", this.intersectionBound) == 0 )
                        //if(rightPoint2.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound))
                        {
                            Polygon2D nextTile = rightTile2;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber+3];
                            nextTileLocationVector = nextTile.transform2D.localPosition;
                            //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                            restoreVector = samplingPosition;
                            resetPoint = rightPoint2;
                            samplingPosition = resetPoint - new Vector2(resetLength, 0);
                            resetMode = true;
                            resetType = "2R";
                            // Debug.Log("Move from Type 2 to Right");
                            break;
                        }

                    }
                    if(!firstRow) // 위쪽 전환인 경우 X
                    {
                        // Debug.Log("Type2 T Inside: "+virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].IsInsideTile(samplingPosition, topTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                        // Debug.Log("Type2 T Intersect: "+virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                        if(virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].IsInsideTile(samplingPosition, topTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                        && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + topPoint2 - new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0
                        && virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + topPoint2 + new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0 )
                        //if(topPoint2.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound)) // 위쪽 전환인 경우 X
                        {
                            Polygon2D nextTile = topTile2;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber - 4*Horizontal + 2];
                            nextTileLocationVector = nextTile.transform2D.localPosition;
                            //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                            restoreVector = samplingPosition;
                            resetPoint = topPoint2;
                            samplingPosition = resetPoint - new Vector2(0, resetLength);
                            resetMode = true;
                            resetType = "2T";
                            // Debug.Log("Move from Type 2 to Top");
                            break;
                        }
                    }

                    // Debug.Log("Type2 L Inside: "+virtualSpace.spaceObjects[currentTileNumber - 1].IsInsideTile(samplingPosition, leftTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                    // Debug.Log("Type2 L Intersect: "+virtualSpace.spaceObjects[currentTileNumber - 1].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                    if(virtualSpace.spaceObjects[currentTileNumber - 1].IsInsideTile(samplingPosition, leftTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                    && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + leftPoint2 + new Vector2(resetLength, 0), Space.World, "default", this.intersectionBound) == 0
                    && virtualSpace.spaceObjects[currentTileNumber - 1].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + leftPoint2 - new Vector2(resetLength, 0), Space.World, "default", this.intersectionBound) == 0 )
                    //if(leftPoint2.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) // 왼쪽인 경우 V
                    {
                        Polygon2D nextTile = leftTile2;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber - 1];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                        restoreVector = samplingPosition;
                        resetPoint = leftPoint2;
                        samplingPosition = resetPoint + new Vector2(resetLength, 0);
                        resetMode = true;
                        resetType = "2L";
                        // Debug.Log("Move from Type 2 to Left");
                        break;
                    }

                    // Debug.Log("Type2 B Inside: "+virtualSpace.spaceObjects[currentTileNumber + 2].IsInsideTile(samplingPosition, bottomTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                    // Debug.Log("Type2 B Intersect: "+virtualSpace.spaceObjects[currentTileNumber + 2].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                    if(virtualSpace.spaceObjects[currentTileNumber + 2].IsInsideTile(samplingPosition, bottomTile2.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                    && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + bottomPoint2 + new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0
                    && virtualSpace.spaceObjects[currentTileNumber + 2].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + bottomPoint2 - new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0 )
                    //if(bottomPoint2.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) // 아랫쪽인 경우 V
                    {
                        Polygon2D nextTile = bottomTile2;// (Polygon2D) virtualSpace.spaceObjects[currentTileNumber + 2];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                        restoreVector = samplingPosition;
                        resetPoint = bottomPoint2;
                        samplingPosition = resetPoint + new Vector2(0, resetLength);
                        resetMode = true;
                        resetType = "2B";
                        // Debug.Log("Move from Type 2 to Bottom");
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

                    // Debug.Log("Type3 T Inside: "+virtualSpace.spaceObjects[currentTileNumber - 2].IsInsideTile(samplingPosition, topTile3.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                    // Debug.Log("Type3 T Intersect: "+virtualSpace.spaceObjects[currentTileNumber - 2].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                    if(virtualSpace.spaceObjects[currentTileNumber - 2].IsInsideTile(samplingPosition, topTile3.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                    && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + topPoint2 - new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0
                    && virtualSpace.spaceObjects[currentTileNumber - 2].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + topPoint2 + new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0 )
                    //if(topPoint2.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) // 위쪽 전환인 경우 V
                    {
                        Polygon2D nextTile = topTile3;// (Polygon2D) virtualSpace.spaceObjects[currentTileNumber - 2];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                        restoreVector = samplingPosition;
                        resetPoint = topPoint2;
                        samplingPosition = resetPoint - new Vector2(0, resetLength);
                        resetMode = true;
                        resetType = "3T";
                        // Debug.Log("Move from Type 3 to Top");
                        break;
                    }
                    if(!lastRow)
                    //if(bottomPoint2.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) // 아랫쪽인 경우 X
                    {
                        // Debug.Log("Type3 lastRow In !");
                        // Debug.Log("Type3 B Intersect: "+virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                        if(virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2].IsInsideTile(samplingPosition, bottomTile3.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                        && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + bottomPoint2 + new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0
                        && virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + bottomPoint2 - new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0 ) // 아랫쪽인 경우 X
                        {
                            Polygon2D nextTile = bottomTile3;// (Polygon2D) virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2];
                            nextTileLocationVector = nextTile.transform2D.localPosition;
                            //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                            restoreVector = samplingPosition;
                            resetPoint = bottomPoint2;
                            samplingPosition = resetPoint + new Vector2(0, resetLength);
                            resetMode = true;
                            resetType = "3B";
                            // Debug.Log("Move from Type 3 to Bottom");
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

                    // Debug.Log("Type4 T Inside: "+virtualSpace.spaceObjects[currentTileNumber - 2].IsInsideTile(samplingPosition, topTile4.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                    // Debug.Log("Type4 T Intersect: "+virtualSpace.spaceObjects[currentTileNumber - 2].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                    if(virtualSpace.spaceObjects[currentTileNumber - 2].IsInsideTile(samplingPosition, topTile4.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                    && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + topPoint1 - new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0
                    && virtualSpace.spaceObjects[currentTileNumber - 2].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + topPoint1 + new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0 ) // 위쪽 전환인 경우 V
                    //if(topPoint1.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) // 위쪽 전환인 경우 V
                    {
                        Polygon2D nextTile = topTile4;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber-2];
                        nextTileLocationVector = nextTile.transform2D.localPosition;
                        //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                        restoreVector = samplingPosition;
                        resetPoint = topPoint1;
                        samplingPosition = resetPoint - new Vector2(0, resetLength);
                        resetMode = true;
                        resetType = "4T";
                        // Debug.Log("Move from Type 4 to Top");
                        break;
                    }
                    if(!lastRow)
                    //if(bottomPoint1.magnitude < resetLength && virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) ) // 아랫쪽인 경우 X
                    {
                        // Debug.Log("Type4 B Inside: "+virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2].IsInsideTile(samplingPosition, bottomTile4.transform2D.localPosition, Space.Self, this.virtualSpaceBound));
                        // Debug.Log("Type4 B Intersect: "+virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", 0f));
                        if(virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2].IsInsideTile(samplingPosition, bottomTile4.transform2D.localPosition, Space.Self, this.virtualSpaceBound)
                        && virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalVirtualSpacePosition + bottomPoint1 + new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0
                        && virtualSpace.spaceObjects[currentTileNumber + 4*Horizontal - 2].NumOfIntersect(globalSamplingPosition, globalVirtualSpacePosition + bottomPoint1 - new Vector2(0, resetLength), Space.World, "default", this.intersectionBound) == 0 ) // 아랫쪽인 경우 X
                        {
                            Polygon2D nextTile = bottomTile4;//(Polygon2D) virtualSpace.spaceObjects[currentTileNumber+4*Horizontal-2];
                            nextTileLocationVector = nextTile.transform2D.localPosition;
                            //Debug.Log("Tile Assigning Vector: " + nextTileLocationVector);

                            restoreVector = samplingPosition;
                            resetPoint = bottomPoint1;
                            samplingPosition = resetPoint + new Vector2(0, resetLength);
                            // currentTargetPosition = resetPoint + new Vector2(0, resetLength);
                            resetMode = true;
                            resetType = "4B";
                            // Debug.Log("Move from Type 4 to Bottom");
                            break;
                        }
                    }
                    // Debug.Log("Type 4 Check Done");
                }
                
                if (count >= 20)
                {
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
            //} while ( (!virtualSpace.IsInsideTile(samplingPosition, nextTileLocationVector, Space.Self, this.virtualSpaceBound) || !virtualSpace.IsPossiblePath(samplingPosition, userPosition, Space.Self) ) && !skipBit); // !virtualSpace.IsPossiblePath(samplingPosition, userPosition, Space.Self, 0.2f)
            //} while ( (!virtualSpace.IsInsideTile(samplingPosition, nextTileLocationVector, Space.Self, this.virtualSpaceBound)) && !skipBit); // !virtualSpace.IsPossiblePath(samplingPosition, userPosition, Space.Self, 0.2f)
            } while ( !virtualSpace.IsInsideTile(samplingPosition, currentTileLocationVector, Space.Self, this.virtualSpaceBound) || !(virtualSpace.spaceObjects[currentTileNumber].NumOfIntersect(globalUserPosition, globalSamplingPosition, Space.World, "default", this.intersectionBound) == 0));

        }
        else if(!virtualSpace.tileMode)
        {
            do
            {
                count++;
            
                float angle = Utility.sampleNormal(0f, 18f, -180f, 180f);
                float distance = 0.2f;
                //float distance = Utility.sampleNormal(0.4f, 2f, 0.25f, 5f); Distance도 랜덤. 깊숙히 안들어가는 문제 약간 보임.
                // Debug.Log(-virtualUserTransform.localPosition * Time.fixedDeltaTime);

                if (count >= 100)
                {
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
                sampleForward = Utility.RotateVector2(virtualUserTransform.forward, angle);
                samplingPosition = userPosition + sampleForward * distance; // local 좌표계에서 절대 위치 기준

            } while (!virtualSpace.IsInside(samplingPosition, Space.Self, 0.5f).Item1); // !virtualSpace.IsPossiblePath(samplingPosition, userPosition, Space.Self, 0.2f)
        }

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
                targetPositionList.Add(userPosition);
                //Debug.Log("1");
            }
            else
            {
                currentTargetPosition = previousUserPosition;
                targetPositionList.Add(previousUserPosition);
                //Debug.Log("2");
            }
            
        }
        else
        {
            count = 1;
            previousUserPosition = userPosition;
            currentTargetPosition = samplingPosition;
            //Debug.Log("3");

            if(!resetMode && !pathRestoreMode)
            {
                targetPositionList.Add(samplingPosition);
                ;//Debug.Log("S");
            }
            else
            {
                if(resetMode && !pathRestoreMode)
                {
                    ;//Debug.Log("W1");
                }
                else if(!resetMode && pathRestoreMode)
                {
                    ;//Debug.Log("W2");
                }
                else if(resetMode && pathRestoreMode)
                {
                    ;//Debug.Log("W3");
                }
            }
        }
        //Debug.Log("4");
        
        if(episodeLength == currentEpisodeIndex + 1)
        {
            filePath = "Assets/Resources/" + "Test1000" +".txt";
            if(!File.Exists(filePath))
            {
                var file = File.CreateText(filePath);
                file.Close();
                StreamWriter sw = new StreamWriter(filePath);
                for (int i = 0; i < targetPositionList.Count; i++)
                {
                    sw.WriteLine(targetPositionList[i].x + "," + targetPositionList[i].y);
                }
                sw.Flush();
                sw.Close();
            }
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
