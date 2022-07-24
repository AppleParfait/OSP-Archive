using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Intersect { EXIST, NONE, INFINITY }
public class Object2D
{
    public Transform2D transform2D;

    public GameObject gameObject
    {
        get
        {
            if (transform2D == null)
                return null;
            else
                return transform2D.transform.gameObject;
        }
    }

    public virtual Bounds2D bound {
        get
        {
            return new Bounds2D(this.gameObject.GetComponent<MeshFilter>().mesh.bounds);
        }
    }

    public Object2D() // 기본 생성자
    {
        GameObject newObject = new GameObject(); // 기본 GameObject 생성
        transform2D = new Transform2D(newObject.transform); // 해당 GameObject의 transform을 참조
    }

    public Object2D(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale, Object2D parentObject = null) // 생성자
    {
        if (parentObject == null)
            Initialize(prefab, name, localPosition, localRotation, localScale, null);
        else
            Initialize(prefab, name, localPosition, localRotation, localScale, parentObject.transform2D.transform);
    }

    public Object2D(Object2D otherObject, string name = null)  // 복사 생성자
    {
        Initialize(otherObject.gameObject, name, otherObject.transform2D.localPosition, otherObject.transform2D.localRotation, otherObject.transform2D.localScale, otherObject.transform2D.parent);
    }

    public Object2D(GameObject prefab) // 참조 생성자
    {
        transform2D = new Transform2D(prefab.transform); // 해당 GameObject의 transform을 참조
    }

    public void Destroy()
    {
        GameObject.Destroy(gameObject);
        transform2D = null;
    }

    public void Destroy(float delay)
    {
        GameObject.Destroy(gameObject, delay);
        transform2D = null;
    }

    public void ChangePrefab(GameObject prefab, string name = null)
    {
        GameObject.Destroy(transform2D.transform.gameObject); // 현재 참조하는 GameObject를 삭제

        if (name == null)
            Initialize(prefab, gameObject.name, transform2D.localPosition, transform2D.localRotation, transform2D.localScale, transform2D.parent);
        else
            Initialize(prefab, name, transform2D.localPosition, transform2D.localRotation, transform2D.localScale, transform2D.parent);
    }

    public void GenerateShape(Material material, float height, bool useOutNormal, string name = null)
    {
        GameObject.Destroy(transform2D.transform.gameObject);

        GameObject newObject = new GameObject();
        newObject.AddComponent<MeshFilter>(); // MeshFilter 컴포넌트 부착
        newObject.GetComponent<MeshFilter>().mesh = GenerateMesh(useOutNormal, height);
        newObject.AddComponent<MeshRenderer>(); // MeshRenderer 컴포넌트 부착
        newObject.GetComponent<MeshRenderer>().material = material; // 공간 object에 대한 material 초기화
        //newObject.GetComponent<MeshRenderer>().enabled = false; // for resource optimi

        //newObject.AddComponent<MeshCollider>(); // MeshRenderer 컴포넌트 부착
        //newObject.GetComponent<MeshCollider>().sharedMesh = newObject.GetComponent<MeshFilter>().mesh; 
        //newObject.layer = 10;

        if (name == null)
            Initialize(newObject, gameObject.name, transform2D.localPosition, transform2D.localRotation, transform2D.localScale, transform2D.parent);
        else
            Initialize(newObject, name, transform2D.localPosition, transform2D.localRotation, transform2D.localScale, transform2D.parent);

        GameObject.Destroy(newObject);
    }
    public void GenerateShape_Obs(Material material, float height, bool useOutNormal, string name = null)
    {
        GameObject.Destroy(transform2D.transform.gameObject);

        GameObject newObject = new GameObject();
        newObject.AddComponent<MeshFilter>(); // MeshFilter 컴포넌트 부착
        newObject.GetComponent<MeshFilter>().mesh = GenerateMesh(useOutNormal, height);
        newObject.AddComponent<MeshRenderer>(); // MeshRenderer 컴포넌트 부착
        newObject.GetComponent<MeshRenderer>().material = material; // 공간 object에 대한 material 초기화
        //newObject.GetComponent<MeshRenderer>().enabled = false; // for resource optimi


        //Debug.Log("---");
        //foreach (var item in newObject.GetComponent<MeshFilter>().sharedMesh.vertices)
        //{
        //    Debug.Log(item);
        //}
        //Debug.Log("---");

        //newObject.AddComponent<MeshCollider>(); // MeshRenderer 컴포넌트 부착
        //newObject.GetComponent<MeshCollider>().sharedMesh = newObject.GetComponent<MeshFilter>().sharedMesh.; 
        //newObject.layer = 10;

        if (name == null)
            Initialize(newObject, gameObject.name, transform2D.localPosition, transform2D.localRotation, transform2D.localScale, transform2D.parent);
        else
            Initialize(newObject, name, transform2D.localPosition, transform2D.localRotation, transform2D.localScale, transform2D.parent);

        GameObject.Destroy(newObject);
    }


    public virtual Object2D Clone(string name = null)
    {
        Object2D copied = new Object2D(this, name);
        return copied;
    }

    public virtual void Initialize(GameObject prefab, string name, Vector2 localPosition, float localRotation, Vector2 localScale, Transform parent)
    {
        GameObject newObject = null;

        if (prefab == null)
            newObject = new GameObject(); // prefab이 주어지지 않았다면 empty GameObject를 생성
        else
            newObject = GameObject.Instantiate(prefab); // prefab을 Instantiate하여 GameObject 생성

        if (name != null) newObject.name = name;
        newObject.transform.parent = parent;
        newObject.transform.localPosition = Utility.CastVector2Dto3D(localPosition); // otherObject의 transform 값을 새로운 GameObject로 붙여넣기
        newObject.transform.localRotation = Utility.CastRotation2Dto3D(localRotation);
        newObject.transform.localScale = Utility.CastVector2Dto3D(localScale, 1);

        transform2D = new Transform2D(newObject.transform); // 생성된 GameObject의 transform을 참조


        //GameObject newObject = null;

        //if (prefab == null)
        //    newObject = new GameObject(); // prefab이 주어지지 않았다면 empty GameObject를 생성
        //else
        //    newObject = GameObject.Instantiate(prefab); // prefab을 Instantiate하여 GameObject 생성

        //if (transform2D != null)
        //{
        //    transform2D.parent = parent;
        //    transform2D.localPosition = localPosition;
        //    transform2D.localRotation = localRotation;
        //    transform2D.localScale = localScale;

        //    transform2D.transform.gameObject.GetComponent<MeshFilter>().mesh = newObject.GetComponent<MeshFilter>().mesh;
        //    transform2D.transform.gameObject.GetComponent<MeshRenderer>().material = newObject.GetComponent<MeshRenderer>().material;

        //    GameObject.Destroy(newObject);
        //}
        //else
        //{
        //    if (name != null) newObject.name = name;
        //    newObject.transform.parent = parent;
        //    newObject.transform.localPosition = Utility.CastVector2Dto3D(localPosition); // otherObject의 transform 값을 새로운 GameObject로 붙여넣기
        //    newObject.transform.localRotation = Utility.CastRotation2Dto3D(localRotation);
        //    newObject.transform.localScale = Utility.CastVector2Dto3D(localScale, 1);

        //    transform2D = new Transform2D(newObject.transform); // 생성된 GameObject의 transform을 참조
        //}

    }

    public virtual Mesh GenerateMesh(bool useOutNormal, float height)
    {
        return new Mesh();
    }

    public virtual bool IsIntersect(Object2D targetObject) // global 좌표계로 변환시킨 후 비교 
    {
        throw new System.NotImplementedException();
    }

    public virtual bool IsIntersect(Edge2D targetLine, Space relativeTo, string option = "default", float bound = 0.01f) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {
        throw new System.NotImplementedException();
    }

    public virtual int NumOfIntersect(Vector2 sourcePosition, Vector2 targetPosition, Space relativeTo, string option = "default", float bound = 0.01f) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {
        throw new System.NotImplementedException();
    }

    public virtual bool IsInside(Object2D targetObject, float bound = 0) // global 좌표계로 변환시킨 후 비교
    {
        throw new System.NotImplementedException();
    }

    public virtual bool IsInside(Vector2 targetPoint, Space relativeTo, float bound = 0) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {
        throw new System.NotImplementedException();
    }

    public virtual bool IsInsideTile(Vector2 targetPoint, Vector2 tileLocation, Space relativeTo, float bound = 0) // targetLine 은 relativeTo 좌표계에 있다고 가정
    {
        throw new System.NotImplementedException();
    }

    public virtual void DebugDraw(Color color)
    {
        throw new System.NotImplementedException();
    }
}
