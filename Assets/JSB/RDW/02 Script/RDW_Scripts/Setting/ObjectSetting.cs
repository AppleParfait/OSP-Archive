using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum OBJECT_TYPE { POLYGON, CIRCLE, LINESEGMENT, AUTO };

[System.Serializable]
public class ObjectSetting
{
    [Header("Common Setting")]
    public OBJECT_TYPE type;
    public string name = null;
    public GameObject prefab;
    public Vector2 position;
    public float rotation;

    [Header("Polygon Setting")]
    public bool useRegularPolygon;
    public int count, size;
    public List<Vector2> vertices;

    [Header("Circle Setting")]
    public float radius;

    [Header("Line Setting")]
    public Vector2 p1;
    public Vector2 p2;

    public Object2D GetObject()
    {
        switch(type)
        {
            default:
                return new Object2DBuilder().SetName(name).SetPrefab(prefab).SetLocalPosition(position).SetLocalRotation(rotation).Build();
            case OBJECT_TYPE.POLYGON:
                return new Polygon2DBuilder().SetName(name).SetPrefab(prefab).SetLocalPosition(position).SetLocalRotation(rotation).SetMode(useRegularPolygon).SetSize(size).SetCount(count).SetVertices(vertices).Build();
            case OBJECT_TYPE.CIRCLE:
                return new Circle2DBuilder().SetName(name).SetPrefab(prefab).SetLocalPosition(position).SetLocalRotation(rotation).SetRadius(radius).Build();
            case OBJECT_TYPE.LINESEGMENT:
                return new LineSegment2DBuilder().SetName(name).SetPrefab(prefab).SetLocalPosition(position).SetLocalRotation(rotation).SetP1(p1).SetP2(p2).Build();
            case OBJECT_TYPE.AUTO:
                if (prefab.tag == "Circle")
                    return new Circle2DBuilder().SetName(name).SetPrefab(prefab).SetLocalPosition(position).SetLocalRotation(rotation).SetRadius(radius).Build();
                else if (prefab.tag == "Line")
                    return new LineSegment2DBuilder().SetName(name).SetPrefab(prefab).SetLocalPosition(position).SetLocalRotation(rotation).SetP1(p1).SetP2(p2).Build();
                else
                    return new Polygon2DBuilder().SetName(name).SetPrefab(prefab).SetLocalPosition(position).SetLocalRotation(rotation).SetMode(useRegularPolygon).SetSize(size).SetCount(count).SetVertices(vertices).Build();
        }
    }
}
