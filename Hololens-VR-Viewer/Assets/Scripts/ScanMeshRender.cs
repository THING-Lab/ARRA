using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanMeshRender : MonoBehaviour
{
    private Mesh mesh;
	private MeshFilter meshFilter;

    void Awake() {
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
		mesh.SetUVs(0, sm.uvs);
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
    }
}
