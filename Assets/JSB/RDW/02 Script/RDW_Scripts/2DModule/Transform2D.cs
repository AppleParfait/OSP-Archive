using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transform2D // transform 위에서 돌아가는 transform 2D
{
    public Transform transform; // transform 3D를 참조

    public Transform2D(Transform transform) // 참조 생성자
    {
        this.transform = transform;
    }

    public Vector2 forward
    {
        get { return Utility.CastVector3Dto2D(transform.forward); }
        set
        {
            if (value.magnitude > 1)
                value = value.normalized;

            transform.forward = Utility.CastVector2Dto3D(value);
        }
    }

    public Vector2 position
    {
        get { return Utility.CastVector3Dto2D(transform.position); }
        set
        {
            transform.position = Utility.CastVector2Dto3D(value);
        }
    }

    public float rotation
    {
        get { return Utility.CastRotation3Dto2D(transform.rotation); }
        set
        {
            transform.rotation = Utility.CastRotation2Dto3D(value);
        }
    }

    public Vector2 localPosition
    {
        get { return Utility.CastVector3Dto2D(transform.localPosition); }
        set
        {
            transform.localPosition = Utility.CastVector2Dto3D(value);
        }
    }
    public float localRotation
    {
        get { return Utility.CastRotation3Dto2D(transform.localRotation); }
        set
        {
            transform.localRotation = Utility.CastRotation2Dto3D(value);
        }
    }
    public Vector2 localScale
    {
        get { return Utility.CastVector3Dto2D(transform.localScale); }
        set
        {
            transform.localScale = Utility.CastVector2Dto3D(value, transform.localScale.y);
        }
    }

    public Transform parent
    {
        get { return transform.parent; }
        set
        {
            transform.parent = value;
        }
    }

    public void Translate(Vector2 translation, Space relativeTo = Space.Self)
    {
        transform.Translate(Utility.CastVector2Dto3D(translation), relativeTo);
    }

    public void Rotate(float degree, Space relativeTo = Space.Self)
    {
        transform.Rotate(new Vector3(0, -degree, 0), relativeTo);
    }

    public Vector2 TransformPointToGlobal(Vector2 localPoint) // this local 좌표계 있는 point를 global 좌표계로 변환
    {
        Vector3 castedPoint = Utility.CastVector2Dto3D(localPoint);
        return Utility.CastVector3Dto2D(transform.TransformPoint(castedPoint));
    }

    public Vector2 TransformPointToLocal(Vector2 globalPoint) // this global 좌표계 있는 point를 this local 좌표계로 변환
    {
        Vector3 castedPoint = Utility.CastVector2Dto3D(globalPoint);
        return Utility.CastVector3Dto2D(transform.InverseTransformPoint(castedPoint));
    }

    public Vector2 TransformPointToOtherLocal(Vector2 localPoint, Transform2D other) // this local 좌표계 있는 point를 wother local 좌표계로 변환
    {
        return other.TransformPointToLocal(this.TransformPointToGlobal(localPoint));
    }

    public override string ToString()
    {
        return string.Format("position: {0}, rotation: {1}, localscale: {2}\n" +
            "localPosition: {3}, localRotation: {4}, localScale: {5}, " +
            "forward: {6}", position, rotation, localScale, localPosition, localRotation, localScale, forward);
    }
}
