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
    public GameObject cameraObj;
    private bool isConnectedToClient = false;
    private string meshBeingBuilt = "";
    public int port;
    public float scale;
    private int countofscans;
    // CameraTransform ct;
    // bool shouldUpdateTransform = false;
    bool recievedPacket = false;
    JSONPacket newPacket;
    public GameObject scanMeshPrefab;
    public GameObject scanMeshParent;

    // Use this for initialization
    void Start () {
        // Start TcpServer background thread
        tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    void OnApplicationQuit(){
        SendJSON(JsonUtility.ToJson(new JSONPacket("CLOSE", JsonUtility.ToJson("END"))));
        connectedTcpClient.GetStream().Close();
        connectedTcpClient.Close();
    }

    // Update is called once per frame
    void Update () {
        if (recievedPacket) {
            switch (newPacket.type) {
                case "CAMERA":
                    CameraTransform ct = JsonUtility.FromJson<CameraTransform>(newPacket.data);
                    transform.position = new Vector3(ct.position[0] * scale, ct.position[1] * scale, ct.position[2] * scale);
                    cameraObj.transform.rotation = new Quaternion(ct.rotation[0], ct.rotation[1], ct.rotation[2], ct.rotation[3]);
                    recievedPacket = false;
                    break;
                case "SCAN_MESH":
                    countofscans += 1;
                    Debug.Log("Recieved a Scan Mesh");
                    GameObject newScan = Instantiate(scanMeshPrefab);
                    newScan.GetComponent<ScanMeshRenderer>().SetMesh(newPacket.data);
                    recievedPacket = false;
                    newScan.transform.parent = scanMeshParent.transform;
                    Debug.Log("Scan Counts: " + countofscans);
                    break;
                default:
                    recievedPacket = false;
                    Debug.Log("Recieved Packet is true with unhandled type! Type: " + newPacket.type);
                    break;
            }
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
        JSONPacket packet = new JSONPacket("SETPING", JsonUtility.ToJson(ping));
        SendJSON(JsonUtility.ToJson(packet));
    }

    public void SendRay(Vector3 p1, Vector3 p2) {
        PointerRay ray = new PointerRay();
        ray.SetAttributes(p1, p2);
        JSONPacket packet = new JSONPacket("RAY", JsonUtility.ToJson(ray));
        SendJSON(JsonUtility.ToJson(packet));
    }

    public void SendStroke(LineRenderer line) {
        StrokeData stroke = new StrokeData(line);
        JSONPacket packet = new JSONPacket("STROKE", JsonUtility.ToJson(stroke));
        SendJSON(JsonUtility.ToJson(packet));
    }

    public void SendClear() {
        JSONPacket packet = new JSONPacket("CLEAR", "");
        SendJSON(JsonUtility.ToJson(packet));
    }

    private void SendJSON(string msg) {
        Debug.Log(msg);
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
                    isConnectedToClient = true;

                    Debug.Log("Client connected");
                    // Get a stream object for reading
                    using (NetworkStream stream = connectedTcpClient.GetStream()) {
                        StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                        while(true) {
                            string json = reader.ReadLine();
                            newPacket = JsonUtility.FromJson<JSONPacket>(json);
                            if(newPacket.type == "CLOSE")
                            {
                                recievedPacket = false;
                                stream.Close();
                                connectedTcpClient.Close();
                                isConnectedToClient = false;
                                Debug.Log("Client Disconnected.");
                                break;
                            }
                            if(newPacket.type == "MESH_CHUNK")
                            {
                                Debug.Log("Recieved Mesh_CHONK");
                                MeshChunk meshChunk = JsonUtility.FromJson<MeshChunk>(newPacket.data);
                                meshBeingBuilt += meshChunk.data;
                                if (meshChunk.isLastChunk)
                                {
                                    Debug.Log("IS LAST!");
                                    countofscans += 1;
                                    Debug.Log("SCAN COUNT: " + countofscans);
                                    newPacket = new JSONPacket("SCAN_MESH", meshBeingBuilt);
                                    meshBeingBuilt = "";
                                    recievedPacket = true;
                                    continue;
                                }
                                recievedPacket = false;
                                continue;

                            }
                            recievedPacket = true;
                         
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
