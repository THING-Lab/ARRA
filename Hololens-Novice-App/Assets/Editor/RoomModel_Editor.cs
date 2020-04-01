using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;




/// <summary>
/// Editor script to import meshes from folder, and apply matrixes
/// </summary>
[CustomEditor(typeof(RoomModel))]
public class RoomModel_Editor : Editor
{

    RoomModel roomModel;



    // Use this for initialization
    void OnEnable()
    {
        roomModel = target as RoomModel;

    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 25;
        GUILayout.Label("Room Model Importer");
        if(GUILayout.Button("Import Mesh"))
        {
            ImportMesh();
        }

        if (GUILayout.Button("Preview materal(click here if image coordinates are not correct in editor)"))
        {
            roomModel.SetShaderParams();
        }
        GUILayout.Space(10);
        GUILayout.EndVertical();
    }



    /// <summary>
    /// extract textures and matrixes from TextureData folder 
    /// file names have to be the same, textures have to be Room*
    /// </summary>
    private void ImportMesh()
    {
        //delete old meshes
        Transform[] allTransforms = roomModel.GetComponentsInChildren<Transform>();

        foreach (Transform childObjects in allTransforms)
        {
            if (roomModel.transform.IsChildOf(childObjects.transform) == false)
                DestroyImmediate(childObjects.gameObject);
        }
        roomModel.goList = new List<GameObject>();
        List<Texture2D> texList = new List<Texture2D>();
       

        for (int i = 0; i < Directory.GetFiles(Application.dataPath + "/TextureData", "*.png", SearchOption.AllDirectories).Length; i++)
        {
            if (File.Exists(Application.dataPath + "/TextureData" + "/Room" + (i + 1).ToString() + ".png"))
            {
                byte[] fileData;
                fileData = File.ReadAllBytes(Application.dataPath + "/TextureData" + "/Room" + (i + 1).ToString() + ".png");
                Texture2D temp = new Texture2D(2, 2);
                temp.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                Debug.Log(temp.width);
                texList.Add(temp);
            }
        }

        roomModel.array = new Texture2DArray(1024, 512, 40, TextureFormat.ARGB32, false);
        Debug.Log(texList.Count);
        for (int i = 0; i < texList.Count; i++)
        {
            //copy to textureArray so that we can pass it to Shader
            Graphics.CopyTexture(texList[i], 0, 0, roomModel.array, i, 0);
        }
        AssetDatabase.CreateAsset(roomModel.array, "Assets/TextureData/2d.asset");
       // texture2dArray = array;

        if (ES3.FileExists(Application.dataPath + "/TextureData/mesh.txt"))
        {
            Debug.Log("exists");
            roomModel.mesh = ES2.LoadArray<Mesh>(Application.dataPath + "/TextureData/mesh.txt");
        }
        
        if (ES3.FileExists(Application.dataPath + "/TextureData/position.txt"))
        {
            roomModel.position = ES2.LoadArray<Vector3>(Application.dataPath + "/TextureData/position.txt");
            Debug.Log("exsit");
        }
        if (ES3.FileExists(Application.dataPath + "/TextureData/rotation.txt"))
        {
            roomModel.rotation = ES2.LoadArray<Quaternion>(Application.dataPath + "/TextureData/rotation.txt");
            Debug.Log("exsit");
        }

        if (ES3.FileExists(Application.dataPath + "/TextureData/projectionMatrixArray.txt"))
        {
            roomModel.projectionMatrixArray = ES2.LoadArray<Matrix4x4>(Application.dataPath + "/TextureData/projectionMatrixArray.txt");


        }
        if (ES3.FileExists(Application.dataPath + "/TextureData/worldToCameraMatrixArray.txt"))
        {
            roomModel.worldToCameraMatrixArray = ES2.LoadArray<Matrix4x4>(Application.dataPath + "/TextureData/worldToCameraMatrixArray.txt");

        }
        if (ES3.FileExists(Application.dataPath + "/TextureData/myObjectToWorld.txt"))
        {
            roomModel.myObjectToWorld = ES2.LoadArray<Matrix4x4>(Application.dataPath + "/TextureData/myObjectToWorld.txt");

        }
        
        
        for (int i = 0; i < roomModel.mesh.Length; i++)
        {
            GameObject temp = new GameObject();
            temp.name = "Mesh" + i.ToString();
            temp.transform.position = roomModel.position[i];
            temp.transform.rotation = roomModel.rotation[i];
            temp.AddComponent<MeshFilter>();
            temp.GetComponent<MeshFilter>().sharedMesh = roomModel.mesh[i];
            temp.AddComponent<MeshRenderer>();
            
            Material material = new Material(Shader.Find("Unlit/ColorRoomShader"));
            AssetDatabase.CreateAsset(material, "Assets/Resources/Materials/" + Selection.activeGameObject.name + i + ".mat");

            

            temp.GetComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("Materials/"+Selection.activeGameObject.name + i);



            temp.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MyArr", roomModel.array);
            temp.GetComponent<MeshRenderer>().sharedMaterial.SetMatrixArray("_WorldToCameraMatrixArray", roomModel.worldToCameraMatrixArray);
            temp.GetComponent<MeshRenderer>().sharedMaterial.SetMatrixArray("_CameraProjectionMatrixArray", roomModel.projectionMatrixArray);


            temp.GetComponent<MeshRenderer>().sharedMaterial.SetMatrix("_MyObjectToWorld", roomModel.myObjectToWorld[i]);
            temp.transform.SetParent(roomModel.gameObject.transform);
            roomModel.goList.Add(temp);            
        }
    }


}
