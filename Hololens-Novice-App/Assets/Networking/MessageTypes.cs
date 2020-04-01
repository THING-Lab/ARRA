using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraTransform {
    public List<float> position = new List<float>();
    public List<float> rotation = new List<float>();

    public void SetAttributes(Transform t) {
        position.Clear();
        position.Add(t.position.x);
        position.Add(t.position.y);
        position.Add(t.position.z);
        rotation.Clear();
        rotation.Add(t.rotation.x);
        rotation.Add(t.rotation.y);
        rotation.Add(t.rotation.z);
        rotation.Add(t.rotation.w);
    }
}

[System.Serializable]
public class JSONPacket {
    public string type;
    public string data;
    public JSONPacket(string t, string msg) {
        type = t;
        data = msg;
    }
}

[System.Serializable]
public class ScanMesh {
    // Transform components
    public Vector3 position;
    public Quaternion rotation;

    // Mesh components
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    public ScanMesh(Vector3 pos, Quaternion rot, Vector3[] verts, int[] tris, Vector2[] inputuvs) {
      // Assigning copies of these params so that we don't have to worry about future under the hood changes by the hololens
      position = new Vector3(pos.x, pos.y, pos.z);
      rotation = new Quaternion(rot.x, rot.y, rot.z, rot.w);

      // Make a copy of mesh components as well
      vertices = new Vector3[verts.Length];
      for (int v = 0; v < verts.Length; v++) {
        vertices[v] = new Vector3(verts[v].x, verts[v].y, verts[v].z);
      }

      triangles = new int[tris.Length];
      for (int t = 0; t < tris.Length; t++) {
        triangles[t] = tris[t];
      }

      uvs = new Vector2[inputuvs.Length];
      for (int u = 0; u < inputuvs.Length; u++) {
        uvs[u] = new Vector2(inputuvs[u].x, inputuvs[u].y);
      }
    }
}

[System.Serializable]
public class MeshChunk {
    public string data;
    public bool isLastChunk;

    public MeshChunk(string d, bool isLast) {
        data = d;
        isLastChunk = isLast;
    }
}

[System.Serializable]
public class ScanTexture {
    public Texture array;
    public Matrix4x4[] projectionMatrixArray;
    public Matrix4x4[] worldToCameraMatrixArray;
    public Matrix4x4[] myObjectToWorld;

    public ScanTexture(Texture arr, Matrix4x4[] projmat, Matrix4x4[] W2CMat, Matrix4x4[] myobj) {
    }
}

[System.Serializable]
public class StrokeData {
    public List<Vector3> points = new List<Vector3>();
    public StrokeData(LineRenderer data) {
        for (int i = 0; i < data.positionCount; i++) {
            Vector3 newPoint = new Vector3();
            newPoint.x = data.GetPosition(i).x;
            newPoint.y = data.GetPosition(i).y;
            newPoint.z = data.GetPosition(i).z;
            points.Add(newPoint);
        }
    }
}

[System.Serializable]
public class ScenePing {
    public List<float> position = new List<float>();
    public void SetAttributes(Vector3 p) {
        position.Clear();
        position.Add(p.x);
        position.Add(p.y);
        position.Add(p.z);
    }
}

[System.Serializable]
public class PointerRay {
    public List<float> position1 = new List<float>();
    public List<float> position2 = new List<float>();

    public void SetAttributes(Vector3 p1, Vector3 p2) {
        position1.Clear();
        position1.Add(p1.x);
        position1.Add(p1.y);
        position1.Add(p1.z);

        position2.Clear();
        position2.Add(p2.x);
        position2.Add(p2.y);
        position2.Add(p2.z);
    }
}
