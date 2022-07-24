using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Text;

public class SocketCommunicator
{
    private int floatByteSize = 4;
    private string ip; 
    private int port;
    private int bufSize;
    public Socket socket;

    public SocketCommunicator()
    {
        this.ip = "127.0.0.1";
        this.port = 8080;
        this.bufSize = 1024;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(this.ip, this.port); 
    }

    public SocketCommunicator(string ip, int port,int bufSize)
    {
        this.ip = ip;
        this.port = port;
        this.bufSize = bufSize;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(this.ip, this.port); 
    }

    public override string ToString() => "IP : " + ip + ", Port number : " + port.ToString();

    public void SendData(float[] dataList)
    {
        try
        {
            int len = dataList.Length * floatByteSize;
            byte[] lenByte = BitConverter.GetBytes(len);
            byte[] sendBytes = new byte[len + floatByteSize];

            byte[] tmp;
            lenByte.CopyTo(sendBytes, 0);
            for (int i = 1; i < dataList.Length; i++)
            {
                tmp = BitConverter.GetBytes(dataList[i]);
                tmp.CopyTo(sendBytes, i * floatByteSize);
            }

            socket.Send(sendBytes);
        }
        catch(Exception e)
        {
            Debug.Log("Send Fail! Socket disconnected!");
        }
    }
    public float[] ReceiveData(int dataNum)
    {
        float[] result = new float[dataNum];
        try
        {
            byte[] receiveByte = new byte[bufSize];
            int rc = socket.Receive(receiveByte);
            if (rc > 0)
            {
                string dataReceived = Encoding.UTF8.GetString(receiveByte);
                string[] sArray = dataReceived.Split(',');
                for (int i = 0; i < dataNum; i++)
                {
                    result[i] = float.Parse(sArray[i]);
                }
            }
            return result;
        }
        catch (Exception e)
        {
            Debug.Log("Receive Fail! Socket disconnected!");
            return result;
        }
    }
}
