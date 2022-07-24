using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space2D 
{
    public Object2D spaceObject;
    public List<Object2D> spaceObjects; // tile용
    public Object2D parentSpaceObject; // tile용
    public List<Vector2> tileCrossingVectors; // tile용
    public List<int> tileAreaSetting; // tile용
    public List<Object2D> obstacles;
    public bool tileMode = false;
    private List<Vector2> initialObstaclePositions;

    public Space2D() // 기본 생성자
    {
        this.spaceObject = new Object2D();
        this.obstacles = new List<Object2D>();
    }

    public Space2D(Space2D otherSpace) // 복사 생성자
    {
        this.spaceObject = otherSpace.spaceObject.Clone();

        this.obstacles = new List<Object2D>();
        foreach (var obstacle in this.obstacles)
        {
            this.obstacles.Add(obstacle.Clone());
        }
    }

    public Space2D(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale) // 생성자
    {
        switch(prefab.tag) // 좀더 깔끔한 코드 있을 꺼 같은데 (추상화 가능성)
        {
            default:
                this.spaceObject = new Polygon2D(prefab, name, localPosition, localRotation, localScale);
                break;
            case "Circle":
                this.spaceObject = new Circle2D(prefab, name, localPosition, localRotation, localScale);
                break;
            case "Line":
                this.spaceObject = new LineSegment2D(prefab, name, localPosition, localRotation, localScale);
                break;
        }

        this.obstacles = new List<Object2D>();
        int children = this.spaceObject.gameObject.transform.childCount;
        foreach(Transform child in this.spaceObject.gameObject.GetComponentInChildren<Transform>())
        {
            switch (child.gameObject.tag)
            {
                default:
                    this.obstacles.Add(new Polygon2D(child.gameObject));
                    break;
                case "Circle":
                    this.obstacles.Add(new Circle2D(child.gameObject));
                    break;
                case "Line":
                    this.obstacles.Add(new LineSegment2D(child.gameObject));
                    break;
            }
        }
    }

    public Space2D(Object2D spaceObject, List<Object2D> obstacles) // 참조 생성자
    {
        this.spaceObject = spaceObject;

        this.obstacles = obstacles;
        foreach (Object2D obstacle in this.obstacles)
        {
            obstacle.transform2D.parent = this.spaceObject.transform2D.transform;
        }
    }

    public Space2D(Object2D parentSpaceObject, List<Object2D> spaceObjects, List<Vector2> tileCrossingVectors, List<int> tileAreaSetting, List<Object2D> obstacles) // 참조 생성자
    {
        this.tileMode = true;

        this.parentSpaceObject = parentSpaceObject;
        this.spaceObjects = spaceObjects;
        this.tileCrossingVectors = tileCrossingVectors;
        this.tileAreaSetting = tileAreaSetting;

        // foreach (Object2D spaceObject in this.spaceObjects)
        // {
        //     spaceObject.transform2D.parent = this.parentSpaceObject.transform2D.transform;
        // }

        this.spaceObject = this.spaceObjects[0];
        this.parentSpaceObject.transform2D.localPosition = this.spaceObjects[0].transform2D.localPosition;
        //spaceObject.transform2D.localPosition = new Vector2(2f,2f);
        //Debug.Log(this.spaceObject.transform2D.localPosition);
        //Debug.Log(this.parentSpaceObject.transform2D.localPosition);
        // Debug.Log(this.parentSpaceObject.transform2D.localPosition);
        // Debug.Log(spaceObjects[0].transform2D.localPosition);
        // Debug.Log(spaceObjects[1].transform2D.localPosition);

        this.obstacles = obstacles;
        foreach (Object2D obstacle in this.obstacles)
        {
            obstacle.transform2D.parent = this.spaceObject.transform2D.transform;
        }
    }

    public void Destroy()
    {
        for (int i = 0; i < this.obstacles.Count; i++)
        {
            this.obstacles[i].Destroy();
        }

        this.spaceObject.Destroy();
    }

    public void GenerateSpace(Material spaceMaterial, Material obstacleMaterial, float spaceHeight, float obstacleHeight, string name = null)
    {
        this.spaceObject.GenerateShape(spaceMaterial, spaceHeight, false, name);
        
        for (int i=0; i<this.obstacles.Count; i++)
        {
            string obstacleName = "obstacle_" + i;
            this.obstacles[i].GenerateShape(obstacleMaterial, obstacleHeight, true, obstacleName);
            this.obstacles[i].transform2D.parent = this.spaceObject.transform2D.transform;
        }
    }

    public void GenerateObstacle(Material obstacleMaterial, Transform parent, int index)
    {
        string obstacleName = "obstacle_" + index;
        ((Polygon2D)this.obstacles[index]).CC(ref RDWSimulationManager.instance.simulationSetting.realSpaceSetting.obstacleObjectSettings[index].vertices);
        this.obstacles[index].GenerateShape_Obs(obstacleMaterial, 2.0f, true, obstacleName);
        this.obstacles[index].transform2D.parent = parent;
    }

    public void GenerateObstacle(Material obstacleMaterial, Transform parent)
    {
        for (int i = 0; i < this.obstacles.Count; i++)
        {
            string obstacleName = "obstacle_" + i;
            ((Polygon2D)this.obstacles[i]).CC(ref RDWSimulationManager.instance.simulationSetting.realSpaceSetting.obstacleObjectSettings[i].vertices);
            this.obstacles[i].GenerateShape(obstacleMaterial, 0.0f, true, obstacleName);
            this.obstacles[i].transform2D.parent = parent;
        }


    }

    public void GenerateTiledSpace(Material spaceMaterial, Material obstacleMaterial, float spaceHeight, float obstacleHeight, string name = null)
    {
        for (int i=0; i<this.spaceObjects.Count; i++)
        {
            string obstacleName = "tile_" + i;
            this.spaceObjects[i].GenerateShape(spaceMaterial, spaceHeight, true, obstacleName);
            this.spaceObjects[i].transform2D.parent = this.parentSpaceObject.transform2D.transform;
        }
        
        for (int i=0; i<this.obstacles.Count; i++)
        {
            string obstacleName = "obstacle_" + i;
            this.obstacles[i].GenerateShape(obstacleMaterial, obstacleHeight, true, obstacleName);
            this.obstacles[i].transform2D.parent = this.parentSpaceObject.transform2D.transform;
        }
    }

    public void SetTileMode(bool tileMode)
    {
        this.tileMode = tileMode;
    }

    public List<Vector2> GetInitialObstaclePositions()
    {
        return this.initialObstaclePositions;
    }

    public void SetInitialObstaclePositions(List<Vector2> initialObstaclePositions)
    {
        this.initialObstaclePositions = initialObstaclePositions;
    }

    public void SetObstaclesToInitialPosition()
    {
        for (int i=0; i<this.obstacles.Count; i++)
        {
            this.obstacles[i].transform2D.localPosition = this.initialObstaclePositions[i];
        }
    }

    public Vector2 GetObstaclePositionByIndex(int index)
    {
        if (index >= 0 && index < this.obstacles.Count)
        {
            return this.obstacles[index].transform2D.localPosition;
        }
        else
        {
            return Vector2.zero;
        }
    }

    public void JumpObstacleByIndex(int index, Vector2 displacement, Space relativeTo = Space.Self)
    {
        if(index >= 0 && index < this.obstacles.Count)
            this.obstacles[index].transform2D.localPosition = displacement;
    }

    public void TranslateObstacleByIndex(int index, Vector2 translation, Space relativeTo = Space.Self)
    {
        if(index >= 0 && index < this.obstacles.Count)
            this.obstacles[index].transform2D.Translate(translation, relativeTo);
    }

    public void TranslateObstacleByName(string name, Vector2 translation, Space relativeTo = Space.Self)
    {
        foreach (var obstacle in this.obstacles)
        {
            if (obstacle.gameObject.name == name)
            {
                obstacle.transform2D.Translate(translation, relativeTo);
                break;
            }
        }
    }
    public void RotateObstacleByIndex(int index, float degree, Space relativeTo = Space.Self)
    {
        if (index >= 0 && index < this.obstacles.Count)
            this.obstacles[index].transform2D.Rotate(degree, relativeTo);
    }

    public void RotateObstacleByName(string name, float degree, Space relativeTo = Space.Self)
    {
        foreach (var obstacle in this.obstacles)
        {
            if (obstacle.gameObject.name == name)
            {
                obstacle.transform2D.Rotate(degree, relativeTo);
                break;
            }
        }
    }

    public void ScaleObstacleByIndex(int index, Vector2 localScale)
    {
        if (index >= 0 && index < this.obstacles.Count)
        {
            if (localScale.x < 0 && this.obstacles[index].transform2D.localScale.x <= 0)
                localScale.x = 0;
            if (localScale.y < 0 && this.obstacles[index].transform2D.localScale.y <= 0)
                localScale.y = 0;

            this.obstacles[index].transform2D.localScale += localScale;
        }
    }

    public void ScaleObstacleByName(string name, Vector2 localScale)
    {
        foreach (var obstacle in this.obstacles)
        {
            if (obstacle.gameObject.name == name)
            {
                if (localScale.x < 0 && obstacle.transform2D.localScale.x <= 0)
                    localScale.x = 0;
                if (localScale.y < 0 && obstacle.transform2D.localScale.y <= 0)
                    localScale.y = 0;

                obstacle.transform2D.localScale += localScale;
                break;
            }
        }
    }

    public Vector2 GetRandomPoint(float bound)
    {
        Vector2 samplingPosition = Vector2.zero; // sampling position은 spaceObject을 기준으로 하는 local position

        do
        {
            //float x = Random.Range(this.spaceObject.bound.min.x, this.spaceObject.bound.max.x);
            //float y = Random.Range(this.spaceObject.bound.min.y, this.spaceObject.bound.max.y);

            float x = Random.Range(this.spaceObject.bound.min.x + 1f, this.spaceObject.bound.max.x -1f);
            float y = Random.Range(this.spaceObject.bound.min.y + 1f, this.spaceObject.bound.max.y - 1f);

            samplingPosition = new Vector2(x, y);
        } while (!this.IsInside(samplingPosition, Space.Self, bound).Item1);

        return samplingPosition;
    }

    public bool IsInside(Object2D otherObject, float bound)
    {
        bool isInside = true;

        foreach (var obstacle in this.obstacles)
        {
            if (obstacle == otherObject)
                continue;
            else if(obstacle.IsIntersect(otherObject) || obstacle.IsInside(otherObject, 0))  // obstacle과 만나거나 아예 안쪽에 있을 경우 space 안에 있다고 보지 않는다
            {
                isInside = false;
                break;
            }
        }

        if (!spaceObject.IsInside(otherObject, bound)) // spaceObject 안쪽에 있지 않을 경우 space 안에 있다고 보지 않는다.
            isInside = false;

        return isInside;
    }

    public (bool,bool) IsInside(Vector2 samplingPosition, Space relativeTo, float bound) // samplingPosition이 relativeTo 좌표계에 있다고 가정
    {
        bool isInside = true;
        bool isShutterReset = false;

        foreach (var obstacle in this.obstacles) // obstacle 밖에 있는지를 확인
        {
            Vector2 localSamplingPosition = Vector2.zero;

            if (relativeTo == Space.World)
                localSamplingPosition = obstacle.transform2D.TransformPointToLocal(samplingPosition);
            else
                localSamplingPosition = spaceObject.transform2D.TransformPointToOtherLocal(samplingPosition, obstacle.transform2D);
            ///20210512 1439
            //if (obstacle.IsInside(localSamplingPosition, Space.Self, -0.2f)) // relativeTo 와 상관없이 local로 비교 
            //if (obstacle.IsInside(localSamplingPosition, Space.Self, -0.499f)) // 0.5m margin for boundry reset
            if (obstacle.IsInside(localSamplingPosition, Space.Self, -0.51f)) // 0.5m margin for boundry reset
            //if (obstacle.IsInside(localSamplingPosition, Space.Self, 0.0f)) 
            {
                isInside = false;
                isShutterReset = true;
                break;
            }
        }

        if (!spaceObject.IsInside(samplingPosition, relativeTo, bound)) // spaceObject 안에 있는지를 확인
            isInside = false;

        return (isInside, isShutterReset);
    }

    public bool IsInsideTile(Vector2 samplingPosition, Vector2 tileLocation, Space relativeTo, float bound) // samplingPosition이 relativeTo 좌표계에 있다고 가정
    {
        bool isInsideTile = true;

        // foreach (var obstacle in this.obstacles) // obstacle 밖에 있는지를 확인
        // {
        //     Vector2 localSamplingPosition = Vector2.zero;

        //     if (relativeTo == Space.World)
        //         localSamplingPosition = obstacle.transform2D.TransformPointToLocal(samplingPosition);
        //     else
        //         localSamplingPosition = spaceObject.transform2D.TransformPointToOtherLocal(samplingPosition, obstacle.transform2D);

        //     if (obstacle.IsInside(localSamplingPosition, Space.Self, -0.2f)) // relativeTo 와 상관없이 local로 비교 
        //     {
        //         isInside = false;
        //         break;
        //     }
        // }

        if (!spaceObject.IsInsideTile(samplingPosition, tileLocation, relativeTo, bound)) // spaceObject 안에 있는지를 확인
            isInsideTile = false;

        return isInsideTile;
    }

    public bool IsPossiblePath(Vector2 targetPosition, Vector2 sourcePosition, Space relativeTo) // TODO: Circle2D 일 때 뭔가 잘 안됨 왜인지는 확인해봐야 함
    {
        if (Vector2.Distance(targetPosition, sourcePosition) <= 0.01f)
            return true;

        bool isPossible = true;
        Edge2D line = null;

        foreach (var obstacle in this.obstacles)
        {
            Vector2 localTargetPosition = Vector2.zero;
            Vector2 localSourcePosition = Vector2.zero;

            if (relativeTo == Space.World)
            {
                localTargetPosition = obstacle.transform2D.TransformPointToLocal(targetPosition);
                localSourcePosition = obstacle.transform2D.TransformPointToLocal(sourcePosition);
            }
            else
            {
                localTargetPosition = spaceObject.transform2D.TransformPointToOtherLocal(targetPosition, obstacle.transform2D);
                localSourcePosition = spaceObject.transform2D.TransformPointToOtherLocal(sourcePosition, obstacle.transform2D);
            }

            line = new Edge2D(localSourcePosition, localTargetPosition);
      
            if (obstacle.IsIntersect(line, Space.Self)) // relativeTo 와 상관없이 // relativeTo 와 상관없이 line과의 intersect를 obstacle local 좌표계로 비교 
            {
                isPossible = false;
                break;
            }
        }

        line = new Edge2D(sourcePosition, targetPosition);

        if (spaceObject.IsIntersect(line, relativeTo, "exclude")) // line과의 intersect을 relativeTo 좌표계를 기준으로 비교
            isPossible = false;

        return isPossible;
    }

    public bool IsPossiblePath(Vector2 targetPosition, Vector2 sourcePosition, Space relativeTo, float bound) // TODO: Circle2D 일 때 뭔가 잘 안됨 왜인지는 확인해봐야 함
    {
        if (Vector2.Distance(targetPosition, sourcePosition) <= 0.01f)
            return true;

        bool isPossible = true;
        Edge2D line = null;

        foreach (var obstacle in this.obstacles)
        {
            Vector2 localTargetPosition = Vector2.zero;
            Vector2 localSourcePosition = Vector2.zero;

            if (relativeTo == Space.World)
            {
                localTargetPosition = obstacle.transform2D.TransformPointToLocal(targetPosition);
                localSourcePosition = obstacle.transform2D.TransformPointToLocal(sourcePosition);
            }
            else
            {
                localTargetPosition = spaceObject.transform2D.TransformPointToOtherLocal(targetPosition, obstacle.transform2D);
                localSourcePosition = spaceObject.transform2D.TransformPointToOtherLocal(sourcePosition, obstacle.transform2D);
            }

            line = new Edge2D(localSourcePosition, localTargetPosition);
      
            if (obstacle.IsIntersect(line, Space.Self)) // relativeTo 와 상관없이 // relativeTo 와 상관없이 line과의 intersect를 obstacle local 좌표계로 비교 
            {
                isPossible = false;
                break;
            }
        }

        line = new Edge2D(sourcePosition, targetPosition);

        if (spaceObject.IsIntersect(line, relativeTo, "exclude", bound)) // line과의 intersect을 relativeTo 좌표계를 기준으로 비교
            isPossible = false;

        return isPossible;
    }

    public void DebugDraws(Color spaceColor, Color obstacleColor)
    {
        this.spaceObject.DebugDraw(spaceColor);

        foreach (var obstacle in this.obstacles)
        {
            obstacle.DebugDraw(obstacleColor);
        }
    }
}
