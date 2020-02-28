﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    private TcpClient socketConnection;     
    private Thread clientReceiveThread;
    public int port;
    public string ip;
    private float frameTime = 0f;
    private float FRAME_MAX = 0.03f;
    private CameraTransform cameraSerial = new CameraTransform();
    public GameObject mainCamera;
    public GameObject pingTarget;
    // public LineRenderer pingRay;
    private bool receivedPacket = false;
    private bool isConnected = false;
    private JSONPacket newPacket;
    public float scaleFactor = 1f;
    public StrokeParser suggestions;

    // Use this for initialization
    void Start () {
        ConnectToTcpServer();
    }

    // Update is called once per frame
    void Update () {
        frameTime += Time.deltaTime;
        if (frameTime > FRAME_MAX) {
            cameraSerial.SetAttributes(mainCamera.transform);
            
            SendJSON(JsonUtility.ToJson(new JSONPacket("CAMERA", JsonUtility.ToJson(cameraSerial))));
        }

        if (receivedPacket) {
            Debug.Log(newPacket.type);
            switch (newPacket.type) {
                case "PING":
                    ScenePing ping = JsonUtility.FromJson<ScenePing>(newPacket.data);
                    pingTarget.transform.position = new Vector3(ping.position[0], ping.position[1], ping.position[2]);
                    break;
                case "SETPING":
                    Debug.Log("Set Ping mang");
                    ScenePing setPing = JsonUtility.FromJson<ScenePing>(newPacket.data);
                    pingTarget.transform.position = new Vector3(setPing.position[0], setPing.position[1], setPing.position[2]);
                    break;
                case "STROKE":
                    StrokeData stroke = JsonUtility.FromJson<StrokeData>(newPacket.data);
                    suggestions.AddStroke(stroke.points);
                    break;
                case "CLEAR":
                    suggestions.Clear();
                    break;
            }
            receivedPacket = false;
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
        while (!isConnected) {
            try {
                socketConnection = new TcpClient(ip, port);
                isConnected = true;
            } catch (SocketException socketException) {
                Debug.Log("Socket exception: " + socketException);
                socketConnection= null;
                isConnected = false;
            }
        }

        while (true) {
            // Get a stream object for reading
            using (NetworkStream stream = socketConnection.GetStream()) {
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                while(true) {
                    string json = reader.ReadLine();
                    newPacket = JsonUtility.FromJson<JSONPacket>(json);
                    receivedPacket = true;
                }               
            }
        }
    }
 
    public void SendJSON(string msg) {
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