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
    public GameObject hmdStreamPanel;
    public GameObject leftHandStreamPanel;
    public GameObject remotePreviewStreamPanel;
    public GameObject hmdBackPanel;
    private GameObject activePanel;
    private int panelIndex = 0;
    private Thread udpListenerThread;
    public int messageByteLength = 24;
    public int port = 4444;

    public int texWidth = 640;
    public int texHeight = 480;
    private Texture2D tex;
    byte[] imageToShow;
    bool shouldShowImage = false;

    // Start is called before the first frame update
    void Awake()
    {
        //Start UDP reciever background thread
        udpListenerThread = new Thread(new ThreadStart(ListenForTextures));
        udpListenerThread.IsBackground = true;
        udpListenerThread.Start();
        tex = new Texture2D(texWidth, texHeight);
        hmdStreamPanel.GetComponent<Renderer>().material.mainTexture = tex;
        leftHandStreamPanel.GetComponent<Renderer>().material.mainTexture = tex;
        remotePreviewStreamPanel.GetComponent<Renderer>().material.mainTexture = tex;

        hmdStreamPanel.SetActive(false);
        leftHandStreamPanel.SetActive(false);
        remotePreviewStreamPanel.SetActive(true);
        activePanel = remotePreviewStreamPanel;
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
        ShowImage(stream);
    }

    private void ListenForTextures() {
        try {
            var udpListener = new UdpClient(4444);
            Debug.Log("Server is listening");
            var remoteEP =  new IPEndPoint(IPAddress.Any, 4444);
                while (true) {
                    var c = udpListener.Receive(ref remoteEP);

                    int imageSize =  frameByteArrayToByteLength(c);
                    //Debug.Log(imageSize);
                    c = udpListener.Receive(ref remoteEP);
                    //Read Image Bytes and Display it
                    if(c.Length == imageSize){
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

    void Update()
    {
        if (shouldShowImage) {
            shouldShowImage = false;
            if(ImageConversion.LoadImage(tex,imageToShow, false)){
              //Debug.Log("SUCCESS!");
            } else {
              //Debug.Log("ERROR!");
            }
        }

        // For toggling panels
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch)) {
            panelIndex += 1;
            if (panelIndex == 1) {
                hmdStreamPanel.SetActive(false);
                hmdBackPanel.SetActive(false);
                leftHandStreamPanel.SetActive(true);
                remotePreviewStreamPanel.SetActive(false);
                activePanel = leftHandStreamPanel;
            } else if (panelIndex == 2) {
                hmdStreamPanel.SetActive(true);
                hmdBackPanel.SetActive(false);
                leftHandStreamPanel.SetActive(false);
                remotePreviewStreamPanel.SetActive(false);
                activePanel = hmdStreamPanel;
            } else if (panelIndex == 3) {
                panelIndex = 0;
                hmdStreamPanel.SetActive(false);
                hmdBackPanel.SetActive(true);
                leftHandStreamPanel.SetActive(false);
                remotePreviewStreamPanel.SetActive(true);
                activePanel = remotePreviewStreamPanel;
            }
        }
    }
}
