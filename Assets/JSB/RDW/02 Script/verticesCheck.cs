using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class verticesCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in GetComponent<MeshFilter>().mesh.vertices)
        {
            Debug.Log(string.Format("{0:F3}", item.x) + "," + string.Format("{0:F3}", item.y) + "," + string.Format("{0:F3}", item.z));
        }
        
    }


}
