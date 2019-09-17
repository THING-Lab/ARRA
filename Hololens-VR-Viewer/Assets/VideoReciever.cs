using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class VideoReciever : MonoBehaviour
{
    private TcpListener tcpListener;
    private Thread udpListenerThread;
    private TcpClient connectedTcpClient;
    public int messageByteLength = 24;
    public int port = 4444;
    private Texture2D tex;
    byte[] imageToShow;
    bool shouldShowImage = false;

    // Start is called before the first frame update
    void Start()
    {
        //Start UDP reciever background thread
        udpListenerThread = new Thread(new ThreadStart(ListenForTextures));
        udpListenerThread.IsBackground = true;
        udpListenerThread.Start();
        tex = new Texture2D(640, 480);
        GetComponent<Renderer>().material.mainTexture = tex;
    }

    int frameByteArrayToByteLength(byte[] frameBytesLength) {
        int byteLength = BitConverter.ToInt32(frameBytesLength, 0);
        return byteLength;
    }

    private int readImageByteSize(NetworkStream stream, int size) {
        bool disconnected = false;
        byte[] imageBytesCount = new byte[size];
        var total = 0;
        do {
            var read = stream.Read(imageBytesCount, total, size - total);
            if (read == 0) {
                disconnected = true;
                break;
            }
            total += read;
        } while (total != size);
        int byteLength;
        if (disconnected) {
                byteLength = -1;
        } else {
                byteLength = frameByteArrayToByteLength(imageBytesCount);
        }
        return byteLength;
    }

    private void readFrameByteArray(byte[] stream, int size) {
        // bool disconnected = false;
        // byte[] imageBytes = new byte[size];
        // var total = 0;
        // do {
        //         var read = stream.Read(imageBytes, total, size - total);
        //         if (read == 0)
        //         {
        //         disconnected = true;
        //         break;
        //         }
        //         total += read;
        // } while (total != size);
        // bool readyToReadAgain = false;
        // //Display Image
        // if (!disconnected) {
        //      //Display Image on the main Thread
        //      Loom.QueueOnMainThread(() => {
        //              loadReceivedImage(imageBytes);
        //              readyToReadAgain = true;
        //      });
        // }
        ShowImage(stream);
        //Wait until old Image is displayed
        // while (!readyToReadAgain) {
        //      System.Threading.Thread.Sleep(1);
        // }
    }

    private void ListenForTextures() {
        try {
            // Create listener on localhost port 8052.
            // tcpListener = new TcpListener(IPAddress.Any, 4444);
            // tcpListener.Start();
            var udpListener = new UdpClient(8052);
            Debug.Log("Server is listening");
            while (true) {
                var remoteEP =  new IPEndPoint(IPAddress.Any, 8052);
                var c = udpListener.Receive(ref remoteEP);
                while (true) {
                    // Read Image Count
                    Debug.Log(c);
                    int imageSize = c.Length;
                    //Read Image Bytes and Display it
                    readFrameByteArray(c, imageSize);
                }
            }
        }
        catch (SocketException socketException) {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    public bool ShowImage(byte[] imageData) {
        imageToShow = (byte[]) imageData.Clone();
        shouldShowImage = true;
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldShowImage) {
            shouldShowImage = false;
            tex.LoadImage(imageToShow);
            tex.Apply();
        }
    }
}
