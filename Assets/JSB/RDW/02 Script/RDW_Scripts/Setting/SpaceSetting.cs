using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpaceSetting
{
    [Header("Common Setting")]
    public bool usePredefinedSpace;

    [Header("Predefined Setting")]
    public string name = null;
    public GameObject predefinedSpace;
    public Vector2 position;
    public float rotation;

    [Header("Procedural Setting")]
    public ObjectSetting spaceObjectSetting;
    public List<ObjectSetting> obstacleObjectSettings;

    public Space2D GetSpace()
    {
        if (usePredefinedSpace)
        {
            return new Space2DBuilder().SetName(name).SetPrefab(predefinedSpace).SetLocalPosition(position).SetLocalRotation(rotation).Build();
        }
        else
        {
            Object2D spaceObject = spaceObjectSetting.GetObject();

            List<Object2D> obstacles = new List<Object2D>();
            foreach (ObjectSetting obstacleObjectSetting in obstacleObjectSettings)
                obstacles.Add(obstacleObjectSetting.GetObject());

            return new Space2DBuilder().SetName(spaceObjectSetting.name).SetSpaceObject(spaceObject).SetObstacles(obstacles).Build();
        }
    }
}
