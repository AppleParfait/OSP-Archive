using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommunicatorTester : MonoBehaviour
{
    private SocketCommunicator socket;
    private float[] dataList;
    // Start is called before the first frame update
    void Start()
    {
        socket = new SocketCommunicator("127.0.0.1", 56789, 1024);
        dataList = new float[3];
        StartCoroutine(Communicate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Communicate()
    {
        while(true)
        {
            dataList[0] = transform.position.x;
            dataList[1] = transform.position.y;
            dataList[2] = transform.position.z;
            socket.SendData(dataList);
            yield return new WaitUntil(() => ReceiveData());
        }
    }
    bool ReceiveData()
    {
        float[] result = socket.ReceiveData(1);
        return true;
    }
}
