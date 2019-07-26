using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

[System.Serializable]
public class CameraTransform {
    public List<float> position = new List<float>();
    public List<float> rotation = new List<float>();

    public void SetAttributes(Transform t) {
        position.Clear();
        position.Add(t.position.x);
        position.Add(t.position.y);
        position.Add(t.position.z);
        rotation.Clear();
        rotation.Add(t.rotation.x);
        rotation.Add(t.rotation.y);
        rotation.Add(t.rotation.z);
        rotation.Add(t.rotation.w);
    }
}

public class TransformSender : MonoBehaviour
{
    private TcpClient socketConnection;     
    private Thread clientReceiveThread;
    public int port;
    private float frameTime = 0f;
    private float FRAME_MAX = 0.03f;
    private CameraTransform cameraSerial = new CameraTransform();

    // Use this for initialization
    void Start () {
        ConnectToTcpServer();
    }

    // Update is called once per frame
    void Update () {
        frameTime += Time.deltaTime;
        if (frameTime > FRAME_MAX) {
            cameraSerial.SetAttributes(transform);
            SendJSON(JsonUtility.ToJson(cameraSerial));
        }
    }

    private void ConnectToTcpServer () {
        try {
            clientReceiveThread = new Thread (new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }       
        catch (Exception e) {
            Debug.Log("On client connect exception " + e);
        }
    }   

    private void ListenForData() {
        try {
            socketConnection = new TcpClient("localhost", port);
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }
 
    private void SendJSON(string msg) {
        if (socketConnection == null) {
            return;
        }

        try {
            // Get a stream object for writing.
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite) {
                // Convert string message to byte array.
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(msg + "\r\n");
                // Write byte array to socketConnection stream.
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }
}

