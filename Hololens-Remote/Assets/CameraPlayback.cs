using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class CameraPlayback : MonoBehaviour
{
    private float frameTime = 0;
    private float frameRate = 0.03f;
    private TrackingCapture captureData;

    private bool isPlaying = true;
    private int currentFrame = 0;
    // Start is called before the first frame update
    void Start()
    {
		captureData = JsonUtility.FromJson<TrackingCapture>(File.ReadAllText("recording.json"));
        transform.position = captureData.positions[currentFrame];
        transform.rotation = captureData.rotations[currentFrame];
    }

    public void play() {
        isPlaying = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying) {
            if (currentFrame < captureData.positions.Count - 1) {
                frameTime += Time.deltaTime;
                if (frameTime >= frameRate) {
                    frameTime -= frameRate;
                    currentFrame += 1;
                    transform.position = captureData.positions[currentFrame];
                    transform.rotation = captureData.rotations[currentFrame];
                }
            } else {
                isPlaying = false;
            }
        }
    }
}