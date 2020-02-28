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
    public List<int> triangles = new List<int>();
    public List<Vector3> vert = new List<Vector3>();
    public List<Vector2> uv = new List<Vector2>();
    public int counter = 0;



    // Use this for initialization
    void OnEnable()
    {
        roomModel = target as RoomModel;

    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(20.0f);
        GUI.skin.label.fontSize = 15;
        GUILayout.Label("Room Model Importer");
        if(GUILayout.Button("Import Mesh"))
        {
            ImportMesh();
        }

        if (GUILayout.Button("Preview materal(click here if image coordinates are not correct in editor)"))
        {
            roomModel.SetShaderParams();
        }
        GUILayout.Space(20.0f);
        GUILayout.EndVertical();
    }

     public void ReadVert(string s)
     {
         float tempx;
         float tempy;
         float tempz;
         
         var charsToRemove = new string[] { "@", ",", ";", "'", "(", ")"};
         
         foreach (var c in charsToRemove)
         {
             s = s.Replace(c, string.Empty);
             
         }
 
         var commands = s.Split (' ');
         tempx = float.Parse(commands[0]);
         tempy = float.Parse(commands[1]);
         tempz = float.Parse(commands[2]);
         vert.Add (new Vector3(tempx,tempy,tempz));
     }
 
     public void ReadUV(string s)
     {
         float tempx;
         float tempy;
         
         var charsToRemove = new string[] { "@", ",", ";", "'", "(", ")"};
         
         foreach (var c in charsToRemove)
         {
             s = s.Replace(c, string.Empty);
             
         }
 
         var commands = s.Split (' ');
         tempx = float.Parse(commands[0]);
         tempy = float.Parse(commands[1]);
         uv.Add (new Vector2(tempx,tempy));
     }
 
     public void ReadTri(string s)
     {
         triangles.Add(int.Parse(s));
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

        if (File.Exists(Application.dataPath + "/TextureData/mesh.txt"))
        {
            string curline;

            StreamReader sr = File.OpenText(Application.dataPath + "/TextureData/mesh.txt");
/*
            while((curline = sr.ReadLine()) != null)
             {
                 counter = 0;
 
                 char[] temp;
                 for(int i = 0; i < curline.Length; i++)
                 {
                     temp = curline.ToCharArray();
                     if(temp[i] == ',' || temp[i] == '(' || temp[i] == ')')
                     {
                         counter++;
                     }
                 }
                 if(counter == 4)
                 {
                     ReadVert(curline);
                 }
                 if(counter == 3)
                 {
                     ReadUV(curline);
                 }
                 if(counter == 0)
                 {
                     ReadTri(curline);
                 }
             }*/
             int i = 0;
            while (sr.ReadLine() != null) { i++; }
            Mesh[] MeshArray = new Mesh[i];
            roomModel.mesh = meshArray;

            sr.Close();
                //roomModel.mesh = File.LoadArray<Mesh>(Application.dataPath + "/TextureData/mesh.txt");
            Debug.Log("exsit");
        }
        if (File.Exists(Application.dataPath + "/TextureData/position.txt"))
        {
            //roomModel.position = File.LoadArray<Vector3>(Application.dataPath + "/TextureData/position.txt");
            Debug.Log("exsit");
        }
        if (File.Exists(Application.dataPath + "/TextureData/rotation.txt"))
        {
            //roomModel.rotation = File.LoadArray<Quaternion>(Application.dataPath + "/TextureData/rotation.txt");
            Debug.Log("exsit");
        }

        if (File.Exists(Application.dataPath + "/TextureData/projectionMatrixArray.txt"))
        {
            //roomModel.projectionMatrixArray = File.LoadArray<Matrix4x4>(Application.dataPath + "/TextureData/projectionMatrixArray.txt");


        }
        if (File.Exists(Application.dataPath + "/TextureData/worldToCameraMatrixArray.txt"))
        {
            //roomModel.worldToCameraMatrixArray = File.LoadArray<Matrix4x4>(Application.dataPath + "/TextureData/worldToCameraMatrixArray.txt");

        }
        if (File.Exists(Application.dataPath + "/TextureData/myObjectToWorld.txt"))
        {
            //roomModel.myObjectToWorld = File.LoadArray<Matrix4x4>(Application.dataPath + "/TextureData/myObjectToWorld.txt");

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
