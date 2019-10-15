using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MessageServer : MonoBehaviour
{
    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private TcpClient connectedTcpClient;
    public int port;
    public float scale;
    CameraTransform ct;
    bool shouldUpdateTransform = false;

    // Use this for initialization
    void Start () {
        // Start TcpServer background thread
        tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    void OnApplicationQuit(){
      tcpListenerThread.Abort();
    }

    // Update is called once per frame
    void Update () {
        if (shouldUpdateTransform) {
            transform.position = new Vector3(ct.position[0] * scale, ct.position[1] * scale, ct.position[2] * scale);
            transform.rotation = new Quaternion(ct.rotation[0], ct.rotation[1], ct.rotation[2], ct.rotation[3]);
            shouldUpdateTransform = false;
        }
    }

    public void SendPing(Vector3 position) {
        ScenePing ping = new ScenePing();
        ping.SetAttributes(position);
        JSONPacket packet = new JSONPacket("PING", JsonUtility.ToJson(ping));
        SendJSON(JsonUtility.ToJson(packet));
    }

    public void SetPing(Vector3 position) {
        ScenePing ping = new ScenePing();
        ping.SetAttributes(position);
        JSONPacket packet = new JSONPacket("SET PING", JsonUtility.ToJson(ping));
        SendJSON(JsonUtility.ToJson(packet));
    }

    public void SendRay(Vector3 p1, Vector3 p2) {
        PointerRay ray = new PointerRay();
        ray.SetAttributes(p1, p2);
        JSONPacket packet = new JSONPacket("RAY", JsonUtility.ToJson(ray));
        SendJSON(JsonUtility.ToJson(packet));
    }

    private void SendJSON(string msg) {
        if (connectedTcpClient == null) {
            return;
        }

        try {
            // Get a stream object for writing.
            NetworkStream stream = connectedTcpClient.GetStream();
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

    private void ListenForIncommingRequests () {
        try {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Debug.Log("Server is listening");

            while (true) {
                // ISSUE: I BELIEVE THIS ONLY CREATES ONE THREAD FOR ALL LISTENERS :?
                using (connectedTcpClient = tcpListener.AcceptTcpClient()) {
                Debug.Log("client connected");
                    // Get a stream object for reading
                    using (NetworkStream stream = connectedTcpClient.GetStream()) {
                        StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                        while(true) {
                            string json = reader.ReadLine();
                            ct = JsonUtility.FromJson<CameraTransform>(json);
                            shouldUpdateTransform = true;
                        }
                    }
                }
            }
        }
        catch (SocketException socketException) {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }
}
