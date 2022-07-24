using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTestScript : MonoBehaviour
{
    private Vector3 deltaRotation;
    // Start is called before the first frame update
    void Start()
    {
        deltaRotation = new Vector3(0, -60, 0);
    }

    private void FixedUpdate()
    {
        Debug.Log(GetComponent<Transform>().rotation.eulerAngles);
        GetComponent<Transform>().Rotate(deltaRotation * Time.fixedDeltaTime);   
    }
}
