using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedirectedUnitBuilder
{
    protected Redirector redirector;
    protected Resetter resetter;
    protected SimulationController controller;
    protected Object2D realUser, virtualUser;
    protected Space2D realSpace, virtualSpace;

    public RedirectedUnitBuilder SetRedirector(Redirector redirector)
    {
        this.redirector = redirector;
        return this;
    }

    public RedirectedUnitBuilder SetResetter(Resetter resetter)
    {
        this.resetter = resetter;
        return this;
    }

    public RedirectedUnitBuilder SetController(SimulationController controller)
    {
        this.controller = controller;
        return this;
    }

    public RedirectedUnitBuilder SetRealUser(Object2D realUser)
    {
        this.realUser = realUser;
        return this;
    }

    public RedirectedUnitBuilder SetVirtualUser(Object2D virtualUser)
    {
        this.virtualUser = virtualUser;
        return this;
    }

    public RedirectedUnitBuilder SetRealSpace(Space2D realSpace)
    {
        this.realSpace = realSpace;
        return this;
    }

    public RedirectedUnitBuilder SetVirtualSpace(Space2D virtualSpace)
    {
        this.virtualSpace = virtualSpace;
        return this;
    }

    public void initialize()
    {
        redirector = null;
        resetter = null;
        controller = null;
        realUser = null;
        virtualUser = null;
        realSpace = null;
        virtualSpace = null;
    }

    public RedirectedUnit Build()
    {
        RedirectedUnit result = null;

        result = new RedirectedUnit(redirector, resetter, controller, realSpace, virtualSpace, realUser, virtualUser);

        initialize();
        return result;
    }

}

public class Space2DBuilder
{
    protected GameObject prefab = null;
    protected string name = "Space2D";
    protected Vector2 localPosition = Vector2.zero;
    protected float localRotation = 0;
    protected Vector2 localScale = Vector2.one;
    protected Object2D spaceObject;
    protected List<Object2D> spaceObjects;
    protected Object2D parentSpaceObject;
    protected List<Vector2> tileCrossingVectors;
    protected List<int> tileAreaSetting;
    protected List<Object2D> obstacles;

    public Space2DBuilder SetName(string _name)
    {
        name = _name;
        return this;
    }

    public Space2DBuilder SetLocalPosition(Vector2 _localPosition)
    {
        localPosition = _localPosition;
        return this;
    }

    public Space2DBuilder SetLocalRotation(float _localRotation)
    {
        localRotation = _localRotation;
        return this;
    }

    public Space2DBuilder SetLocalScale(Vector2 _localScale)
    {
        localScale = _localScale;
        return this;
    }

    public Space2DBuilder SetPrefab(GameObject _prefab)
    {
        prefab = _prefab;
        return this;
    }

    public Space2DBuilder SetSpaceObject(Object2D spaceObject)
    {
        this.spaceObject = spaceObject;
        return this;
    }

    public Space2DBuilder SetSpaceObjects(List<Object2D> spaceObjects)
    {
        this.spaceObjects = spaceObjects;
        return this;
    }

    public Space2DBuilder SetTileCrossingVectors(List<Vector2> _tileCrossingVectors)
    {
        this.tileCrossingVectors = _tileCrossingVectors;
        return this;
    }

    public Space2DBuilder SetTileAreaSetting(List<int> _tileAreaSetting)
    {
        this.tileAreaSetting = _tileAreaSetting;
        return this;
    }

    public Space2DBuilder SetObstacles(List<Object2D> obstacles)
    {
        this.obstacles = obstacles;
        return this;
    }

    public Space2DBuilder SetParentSpaceObject(Object2D _parentSpaceObject)
    {
        parentSpaceObject = _parentSpaceObject;
        return this;
    }

    public void initialize()
    {
        prefab = null;
        name = "Space2D";
        localPosition = Vector2.zero;
        localRotation = 0;
        localScale = Vector2.one;
        spaceObject = null;
        spaceObjects = null;
        parentSpaceObject = null;
        obstacles = null;
    }

    public Space2D Build()
    {
        Space2D result = null;

        if (spaceObject != null)
            result = new Space2D(spaceObject, obstacles);
        else if(spaceObjects != null && parentSpaceObject != null && tileCrossingVectors != null)
            result = new Space2D(parentSpaceObject, spaceObjects, tileCrossingVectors, tileAreaSetting, obstacles);
        else
            result = new Space2D(prefab, name, localPosition, localRotation, localScale);
        initialize();
        return result;
    }

}

public abstract class AbstractBuilder<TOjbect, TBuilder>
where TOjbect : Object2D
where TBuilder : AbstractBuilder<TOjbect, TBuilder>
{
    protected TBuilder _builderInstance = null;

    protected GameObject prefab = null;
    protected string name = "Object2D";
    protected Vector2 localPosition = Vector2.zero;
    protected float localRotation = 0;
    protected Vector2 localScale = Vector2.one;
    protected Object2D parentObject = null;

    public AbstractBuilder()
    {
        _builderInstance = (TBuilder)this;
        prefab = null;
        name = "Object2D";
        localPosition = Vector2.zero;
        localRotation = 0;
        localScale = Vector2.one;
        parentObject = null;
    }

    public virtual void initialize()
    {
        prefab = null;
        name = "Object2D";
        localPosition = Vector2.zero;
        localRotation = 0;
        localScale = Vector2.one;
        parentObject = null;
    }

    public TBuilder SetName(string _name)
    {
        name = _name;
        return _builderInstance;
    }

    public TBuilder SetLocalPosition(Vector2 _localPosition)
    {
        localPosition = _localPosition;
        return _builderInstance;
    }

    public TBuilder SetLocalRotation(float _localRotation)
    {
        localRotation = _localRotation;
        return _builderInstance;
    }

    public TBuilder SetLocalScale(Vector2 _localScale)
    {
        localScale = _localScale;
        return _builderInstance;
    }

    public TBuilder SetPrefab(GameObject _prefab)
    {
        prefab = _prefab;
        return _builderInstance;
    }

    public TBuilder SetParent(Object2D _parentObject)
    {
        parentObject = _parentObject;
        return _builderInstance;
    }

    public abstract TOjbect Build();
}


public class Object2DBuilder : AbstractBuilder<Object2D, Object2DBuilder>
{
    public override Object2D Build()
    {
        Object2D result = null;
        result = new Object2D(prefab, name, localPosition, localRotation, localScale, parentObject);
        initialize();
        return result;
    }
}

public class Polygon2DBuilder : AbstractBuilder<Polygon2D, Polygon2DBuilder>
{
    protected bool useRegularPolygon = true;
    protected bool tileMode = false;
    protected List<Vector2> vertices = null;
    protected int count = 4;
    protected float size = 1;
    protected List<float> rotationInfo = new List<float>(new float[] {0, 0});
    protected Vector2 movementInfo = Vector2.zero;
    protected int tileType = 0;

    public Polygon2DBuilder() : base()
    {
        useRegularPolygon = true;
        tileMode = false;
        vertices = null;
        count = 4;
        size = 1;
        name = "Polygon2D";
        rotationInfo = new List<float>(new float[] {0, 0});
        movementInfo = Vector2.zero;
        tileType = 0;
    }

    public override void initialize()
    {
        base.initialize();
        useRegularPolygon = true;
        tileMode = false;
        vertices = null;
        count = 4;
        size = 1;
        name = "Polygon2D";
        rotationInfo = new List<float>(new float[] {0, 0});
        movementInfo = Vector2.zero;
        tileType = 0;
    }

    public Polygon2DBuilder SetTileMode(bool _tileMode)
    {
        tileMode = _tileMode;
        return _builderInstance;
    }

    public Polygon2DBuilder SetMode(bool _useRegularPolygon)
    {
        useRegularPolygon = _useRegularPolygon;
        return _builderInstance;
    }

    public Polygon2DBuilder SetVertices(List<Vector2> _vertices)
    {
        vertices = _vertices;
        return _builderInstance;
    }

    public Polygon2DBuilder SetCount(int _count)
    {
        count = _count;
        return _builderInstance;
    }

    public Polygon2DBuilder SetSize(float _size)
    {
        size = _size;
        return _builderInstance;
    }

    public Polygon2DBuilder SetCrossingInfo(List<float> _rotationInfo, Vector2 _movementInfo)
    {
        for(int i=0; i < 2; i++)
            rotationInfo[i] = _rotationInfo[i];
        movementInfo = _movementInfo;

        return _builderInstance;
    }

    public Polygon2DBuilder SetTileType(int _tileType)
    {
        tileType = _tileType;
        return _builderInstance;
    }

    public override Polygon2D Build()
    {
        Polygon2D result = null;

        if (useRegularPolygon)
            result = new Polygon2D(prefab, name, localPosition, localRotation, localScale, count, size, parentObject);
        else
        {
            if(tileMode)
            {
                List<float> movementFloatInfo = new List<float>();
                movementFloatInfo.Add(movementInfo.x);
                movementFloatInfo.Add(movementInfo.y);
                result = new Polygon2D(prefab, name, localPosition, localRotation, localScale, parentObject, vertices, tileType, rotationInfo, movementFloatInfo);
            }
            else
                result = new Polygon2D(prefab, name, localPosition, localRotation, localScale, parentObject, vertices);
        }

        initialize();
        return result;
    }
}

public class Circle2DBuilder : AbstractBuilder<Circle2D, Circle2DBuilder>
{
    private float radius = 1;

    public Circle2DBuilder() : base()
    {
        radius = 1;
        name = "Circle2D";
    }

    public override void initialize()
    {
        base.initialize();
        radius = 1;
        name = "Circle2D";
    }

    public Circle2DBuilder SetRadius(float _radius)
    {
        radius = _radius;
        return _builderInstance;
    }

    public override Circle2D Build()
    {
        Circle2D result = null;
        result = new Circle2D(prefab, name, localPosition, localRotation, localScale, parentObject, radius);
        initialize();
        return result;
    }
}

public class LineSegment2DBuilder : AbstractBuilder<LineSegment2D, LineSegment2DBuilder>
{
    private Vector2 p1 = Vector2.right;
    private Vector2 p2 = Vector2.left;

    public LineSegment2DBuilder() : base()
    {
        p1 = Vector2.right;
        p2 = Vector2.left;
        name = "LineSegment2D";
    }

    public override void initialize()
    {
        base.initialize();
        p1 = Vector2.right;
        p2 = Vector2.left;
        name = "LineSegment2D";
    }

    public LineSegment2DBuilder SetP1(Vector2 p1)
    {
        this.p1 = p1;
        return _builderInstance;
    }

    public LineSegment2DBuilder SetP2(Vector2 p2)
    {
        this.p2 = p2;
        return _builderInstance;
    }

    public override LineSegment2D Build()
    {
        LineSegment2D result = null;
        result = new LineSegment2D(prefab, name, localPosition, localRotation, localScale, parentObject, p1, p2);
        initialize();
        return result;
    }

}