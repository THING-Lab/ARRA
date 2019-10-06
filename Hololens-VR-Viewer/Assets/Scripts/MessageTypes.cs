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
