using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.XR.WSA.Input;
using System.IO;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

//Imports to interact with Camera
#if WINDOWS_UWP
using System.Runtime.InteropServices;
using Windows.Media.Devices;
#endif



/// <summary>
/// Take photos and store them in the disk and apply matrix to preview shader, because the helpermesh usees the 
/// same material as spatialmapping, spatialmapping will update too
/// </summary>
public class TakePicture : MonoBehaviour {

    WebCamTexture webcam;

    //data copying fronm list to pass to shader
    public Matrix4x4[] worldToCameraMatrixArray;
    public Matrix4x4[] projectionMatrixArray;
    public Texture2DArray array;

    //the max photos numbers that can be taken, shader doesn't support dynamic array
    public int maxPhotoNum;
    //Locks in the exposure and white balance after first photo.
    public bool lockCameraSettings = true;
    Texture2D destTex;

    //camera paramaters
    private UnityEngine.XR.WSA.WebCam.CameraParameters m_CameraParameters;
    Texture2D m_Texture;
    byte[] bytes;

    //temp data waiting to pass to shader
    //private List<Texture2D> textureList;
    private List<Matrix4x4> projectionMatrixList;
    private List<Matrix4x4> worldToCameraMatrixList;


    private bool isCapturingPhoto = false;
    private Texture2D photoCaptureObj;
    private int currentPhoto = 0;
    //private GestureRecognizer recognizer;

    //private TextManager textmanager;


    // Use this for initialization
    void Start () {
        //init
        //textureList = new List<Texture2D>();
        projectionMatrixList = new List<Matrix4x4>();
        worldToCameraMatrixArray = new Matrix4x4[maxPhotoNum];
        projectionMatrixArray = new Matrix4x4[maxPhotoNum];
        worldToCameraMatrixList = new List<Matrix4x4>();


        //init camera
        InitCamera();

    }




    void InitCamera()
    {
        //**List<Resolution> resolutions = new List<Resolution>(UnityEngine.XR.WSA.WebCam.PhotoCapture.SupportedResolutions);

        //default using 1280*720,considering hololens' performance and unity auto crop texture to 1024
        //**Resolution selectedResolution = resolutions[0];


        //camera params
        /**
        m_CameraParameters = new UnityEngine.XR.WSA.WebCam.CameraParameters(UnityEngine.XR.WSA.WebCam.WebCamMode.PhotoMode);
        m_CameraParameters.cameraResolutionWidth = selectedResolution.width;
        m_CameraParameters.cameraResolutionHeight = selectedResolution.height;
        m_CameraParameters.hologramOpacity = 0.0f;
        m_CameraParameters.pixelFormat = UnityEngine.XR.WSA.WebCam.CapturePixelFormat.BGRA32;
        **/
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            print("Webcam available: " + devices[i].name);
        }

        webcam = new WebCamTexture(devices[0].name, 1280, 720, 30);
        GetComponent<Renderer>().material.mainTexture = webcam;
        webcam.Play();

        //create texture array, its size has to be power of 2, so we have to crop from 1280*720 to 1024*512
        //DXT5 requires that its size also has to be a power of 4
        array = new Texture2DArray(1024, 512, maxPhotoNum, TextureFormat.DXT5, false);
        //   m_Texture = new Texture2D(m_CameraParameters.cameraResolutionWidth, m_CameraParameters.cameraResolutionHeight, TextureFormat.ARGB32, false);
        //init photocaptureobject
        //**UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnCreatedPhotoCaptureObject);
        
        
    }

    void Update() 
    {      
        if (Input.GetKeyDown("space"))
        {
            print("space key was pressed");
            TakeSnapshot();
        }      
    }
   
    public void OnPhotoKeyWordDetected()
    {
        //TakeSnapshot
    }
    void TakeSnapshot()
    {
        
        //After the first photo, we want to lock in the current exposure and white balance settings.
        if (lockCameraSettings && currentPhoto == 1)
        {
#if WINDOWS_UWP
        unsafe{
            //This is how you access a COM object.
            VideoDeviceController vdm = (VideoDeviceController)Marshal.GetObjectForIUnknown(photoCaptureObj.GetUnsafePointerToVideoDeviceController());
            //Locks current exposure for all future images
            vdm.ExposureControl.SetAutoAsync(false); //Figureout how to better handle the Async
            //Locks the current WhiteBalance for all future images
            vdm.WhiteBalanceControl.SetPresetAsync(ColorTemperaturePreset.Manual); 
        }
#endif
        }
        //get this renderer
        Renderer m_CanvasRenderer = GetComponent<Renderer>() as Renderer;
        // m_CanvasRenderer.material = new Material(Shader.Find("Unlit/ColorRoomShader"));
        //temp to store the matrix
        Matrix4x4 cameraToWorldMatrix = Camera.main.cameraToWorldMatrix;
        Matrix4x4 worldToCameraMatrix = cameraToWorldMatrix.inverse;

        Matrix4x4 projectionMatrix = Camera.main.projectionMatrix;

        //add to matrixList
        projectionMatrixList.Add(projectionMatrix);
        worldToCameraMatrixList.Add(worldToCameraMatrix);


        m_Texture = new Texture2D(webcam.width, webcam.height, TextureFormat.RGBA32, false);
        m_Texture.SetPixels(webcam.GetPixels());
        
        m_Texture.wrapMode = TextureWrapMode.Clamp;

        m_Texture = resizeTexture(m_Texture, 1024, 512);
        //textureList.Add(m_Texture);

        //save room to png
        bytes = m_Texture.EncodeToPNG();
        //write to LocalState folder
        Debug.Log("saved to : " + Application.dataPath + "/roomimages/");
        File.WriteAllBytes(Application.dataPath + "/roomimages/NEWROOM" + (currentPhoto+1) + ".png", bytes);

        //update matrix and texturearray in shader
        //Graphics.CopyTexture(textureList[textureList.Count - 1], 0, 0, array, textureList.Count - 1, 0);
        m_Texture.Compress(true);
        Debug.Log(m_Texture.format);
        Graphics.CopyTexture(m_Texture, 0, 0, array, currentPhoto, 0); //Copies the last texture
        worldToCameraMatrixArray[currentPhoto] = worldToCameraMatrixList[currentPhoto];
        projectionMatrixArray[currentPhoto] = projectionMatrixList[currentPhoto];

        //set shader properties
        GetComponent<Renderer>().sharedMaterial.SetTexture("_MyArr", array);
        GetComponent<Renderer>().sharedMaterial.SetMatrixArray("_WorldToCameraMatrixArray", worldToCameraMatrixArray);
        GetComponent<Renderer>().sharedMaterial.SetMatrixArray("_CameraProjectionMatrixArray", projectionMatrixArray);
        //textmanager.setText(currentPhoto+1 + " Photos Taken");
        currentPhoto += 1; //Increments the counter

        isCapturingPhoto = false;

        Resources.UnloadUnusedAssets();
    }

    //Helper function to resize from top left
    Texture2D resizeTexture(Texture2D input, int width, int height)
    {
        Debug.Log(input);
        Debug.Log(width);
        Debug.Log(height);
        Color[] pix = input.GetPixels(0, input.height - height, width, height);
        input.Resize(width, height);
        input.SetPixels(pix);
        input.Apply();
        return input;
    }

}
