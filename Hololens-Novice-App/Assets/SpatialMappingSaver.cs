using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
public class SpatialMappingSaver : MonoBehaviour {



    //data that we need to save into disk 
    private List<Mesh> mesh;
    private List<Vector3> positionList;
    private List<Quaternion> rotationList;
    private List<Matrix4x4> MyObjectToWorld;
    private GameObject tempMesh;
    
    private GameObject spatialawarenesssystem;
    // Use this for initialization
    void Start () {
        positionList= new List<Vector3>();
        rotationList = new List<Quaternion>() ;
        MyObjectToWorld = new List<Matrix4x4>();
        
        spatialawarenesssystem = GameObject.Find("Spatial Awareness System");
        if(spatialawarenesssystem != null) Debug.Log("found spatial MEsh!");
    }

    void OnEnable()
    {
        //clean data before starting
        System.IO.DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/roomimages/");

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
    }

    //copy mesh gameobejcts from spatialmapping manager to our gameobjects and shrink
    public void ScaleDown()
    {
        //TextManager.Instance.setText("Shrinking Room");
       
        mesh = new List<Mesh>();
        tempMesh = new GameObject();

        Transform[] ts = spatialawarenesssystem.GetComponentsInChildren<Transform>();

        Debug.Log("Transform");

        //copy mesh from spatialmapping gameobject to a new gameobject
        for (int i=0;i<ts.Length;i++)
        {

            //Spatialmapping sometimes has children which is not mesh object
            if (ts[i].GetComponent<MeshRenderer>() == null || ts[i].gameObject.GetComponent<MeshFilter>().sharedMesh == null||
                ts[i].gameObject.GetComponent<MeshFilter>() == null)
                continue;

            GameObject temp = new GameObject();

            //keep mesh's position and rotation
            temp.transform.position = ts[i].gameObject.transform.position;
            temp.transform.rotation = ts[i].gameObject.transform.rotation;
            positionList.Add(temp.transform.position);
            rotationList.Add(temp.transform.rotation);

            Debug.Log(ts[i].gameObject.transform.localScale);
            temp.AddComponent<MeshFilter>();


           
            temp.GetComponent<MeshFilter>().sharedMesh = (Mesh)Instantiate(ts[i].gameObject.GetComponent<MeshFilter>().sharedMesh);
            mesh.Add(temp.GetComponent<MeshFilter>().sharedMesh);                         
            temp.AddComponent<MeshRenderer>();
            temp.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/ColorRoomShader"));
            
            TakePicture tp = new TakePicture();

            //get matrix from TakePhoto class
             Matrix4x4[] worldToCameraMatrixArray = tp.worldToCameraMatrixArray;         
             Matrix4x4[] projectionMatrixArray = tp.projectionMatrixArray;
             Texture2DArray array = tp.array;

            //set shader properties
            temp.GetComponent<MeshRenderer>().material.SetTexture("_MyArr", array);       
            temp.GetComponent<MeshRenderer>().material.SetMatrixArray("_WorldToCameraMatrixArray", worldToCameraMatrixArray);
            temp.GetComponent<MeshRenderer>().material.SetMatrixArray("_CameraProjectionMatrixArray", projectionMatrixArray);
            temp.GetComponent<MeshRenderer>().material.SetMatrix("_MyObjectToWorld", ts[i].gameObject.GetComponent<MeshRenderer>().localToWorldMatrix);

            //add localtoworld matrix to our list, so that we can save later
            MyObjectToWorld.Add(ts[i].gameObject.GetComponent<MeshRenderer>().localToWorldMatrix);
   
            temp.transform.SetParent(tempMesh.transform);
        }

        //shrink and adjust in front of player
        tempMesh.transform.localScale= new Vector3(0.05f, 0.05f, 0.05f);

        tempMesh.transform.localPosition = Camera.main.transform.position + new Vector3(0.5f, -0.5f, 0.5f);

        //TextManager.Instance.setText("Room Shrinked");
    }

    void Update() 
    {
        spatialawarenesssystem = GameObject.Find("Spatial Awareness System");
        if(GameObject.Find("SceneContent").GetComponent<RoomScanSender>().boundaryVisualizationFound == false && GameObject.Find("SceneContent").GetComponent<RoomScanSender>().boundaryVis != null) Debug.Log("found spatial MEsh!");

        if (Input.GetKeyDown(KeyCode.S))
        {
//            if(spatialawarenesssystem)
//            { 
                print("S key was pressed");
                Save();
//            }
//            else Debug.Log("you need to scan the room first!");
        }
    }


    public void Save()
    {

        //save data to disk

            //TextManager.Instance.setText("Saving room parameters to disk");
            ScaleDown();
            Mesh[] MeshArray = new Mesh[mesh.Count];
            MeshArray = mesh.ToArray();
            ES2.Save(MeshArray, "mesh.txt");

            ES2.Save(positionList.ToArray(), "position.txt");

            ES2.Save(rotationList.ToArray(), "rotation.txt");

            ES2.Save(GameObject.Find("Take Picture").GetComponent<TakePicture>().projectionMatrixArray, Application.dataPath + "/roomimages/projectionMatrixArray.txt");
            ES2.Save(GameObject.Find("Take Picture").GetComponent<TakePicture>().worldToCameraMatrixArray, Application.dataPath + "/roomimages/worldToCameraMatrixArray.txt");
            ES2.Save(MyObjectToWorld.ToArray(), "myObjectToWorld.txt");
            Debug.Log("saved!");
            //TextManager.Instance.setText("Saved!");
        
    }
}
