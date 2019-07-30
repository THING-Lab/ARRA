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
public class ScenePing {
    public List<float> position = new List<float>();

    public void SetAttributes(Vector3 p) {
        position.Clear();
        position.Add(p.x);
        position.Add(p.y);
        position.Add(p.z);
    }
}
