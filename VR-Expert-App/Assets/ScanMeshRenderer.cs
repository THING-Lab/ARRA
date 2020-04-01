using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScanMeshRenderer : MonoBehaviour
{
    private Mesh mesh;
	private MeshFilter meshFilter;

    void Awake() {
        //gameObject.AddComponent<UnityEngine.MeshRenderer>();
        //gameObject.AddComponent<UnityEngine.MeshCollider>();
        meshFilter = gameObject.GetComponent<MeshFilter>();
		mesh = new Mesh();
		meshFilter.mesh = mesh;
    }

    public void SetMesh(string meshPacket) {
        ScanMesh sm = JsonUtility.FromJson<ScanMesh>(meshPacket);
        transform.position = sm.position;
        transform.rotation = sm.rotation;


        mesh.vertices = sm.vertices;
		mesh.triangles = sm.triangles;
		mesh.SetUVs(0, sm.uvs.ToList());
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
    }
}
