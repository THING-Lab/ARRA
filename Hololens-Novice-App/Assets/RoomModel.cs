using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class RoomModel : MonoBehaviour {

    public Mesh[] mesh;
    public Vector3[] position;
    public Quaternion[] rotation;
    public Texture2DArray array;
    public Matrix4x4[] projectionMatrixArray;
    public Matrix4x4[] worldToCameraMatrixArray;
    public Matrix4x4[] myObjectToWorld;

    public List<GameObject> goList;
    // Use this for initialization
    void Start () {
        SetShaderParams();

    }
	
	// Update is called once per frame
	void Update () {
	
	}



    /// <summary>
    /// When game is played, reapply those matrixes
    /// </summary>
    public void SetShaderParams()
    {
        for (int i = 0; i < goList.Count; i++)
        {
            goList[i].GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MyArr", array);
            goList[i].GetComponent<MeshRenderer>().sharedMaterial.SetMatrixArray("_WorldToCameraMatrixArray", worldToCameraMatrixArray);
            goList[i].GetComponent<MeshRenderer>().sharedMaterial.SetMatrixArray("_CameraProjectionMatrixArray", projectionMatrixArray);

            goList[i].GetComponent<MeshRenderer>().sharedMaterial.SetMatrix("_MyObjectToWorld", myObjectToWorld[i]);
        }
    }
}
