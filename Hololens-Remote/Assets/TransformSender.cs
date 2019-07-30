using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TransformSender : MonoBehaviour
{
    private TcpClient socketConnection;     
    private Thread clientReceiveThread;
    public int port;
    private float frameTime = 0f;
    private float FRAME_MAX = 0.03f;
    private CameraTransform cameraSerial = new CameraTransform();
    public GameObject pingTarget;
    private ScenePing ping;
    private bool shouldUpdateTransform = false;
    public float scaleFactor = 1f;

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

        if (shouldUpdateTransform) {
            shouldUpdateTransform = false;
            pingTarget.transform.position = new Vector3(ping.position[0]/scaleFactor, ping.position[1]/scaleFactor, ping.position[2]/scaleFactor);
        }
    }

    private void ConnectToTcpServer () {
        try {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
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

            while (true) {
                // Get a stream object for reading
                using (NetworkStream stream = socketConnection.GetStream()) {
                    StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                    while(true) {
                        string json = reader.ReadLine();
                        ping = JsonUtility.FromJson<ScenePing>(json);
                        shouldUpdateTransform = true;
                    }               
                }
			}
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

