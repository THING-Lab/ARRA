using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class TrackingCapture {
    public List<Vector3> positions = new List<Vector3>();
    public List<Quaternion> rotations = new List<Quaternion>();
}

public class PositionCapture : MonoBehaviour
{
    // in seconds
    public float captureWindow;
    private float captureTime = 0;
    private float frameTime = 0;
    private float frameRate = 0.03f;
    private TrackingCapture captureData = new TrackingCapture();
    private bool hasSaved = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        captureTime += Time.deltaTime;
        frameTime += Time.deltaTime;

        if (captureTime <= captureWindow) {
            if (frameTime >= frameRate) {
                frameTime -= frameRate;
                captureData.positions.Add(new Vector3(transform.position.x, transform.position.y, transform.position.z));
                captureData.rotations.Add(new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w));
            }
        } else if (!hasSaved) {
            hasSaved = true;
            string json = JsonUtility.ToJson(captureData);
            Debug.Log("write tracking data");
            using (StreamWriter sw = new StreamWriter("recording.json")) {
                sw.Write(json);
            }
        }
    }
}