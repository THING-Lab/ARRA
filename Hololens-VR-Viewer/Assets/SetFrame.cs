using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFrame : MonoBehaviour
{
    public double startTime;
    public GameObject hmdStreamPanel;
    public GameObject leftHandStreamPanel;
    public GameObject remotePreviewStreamPanel;
    public GameObject hmdBackPanel;
    public UnityEngine.Video.VideoClip videoToPlay;

    // Start is called before the first frame update
    void Start()
    {
        //Build array of the panels for for loop.
        GameObject[]  panels = new GameObject[] { hmdStreamPanel, leftHandStreamPanel, remotePreviewStreamPanel, hmdBackPanel };

        //Associate the Streamer to the Video Controller
        GameObject controller = GameObject.Find("Video Controller");

        //Build the Renderers
        UnityEngine.Video.VideoPlayer[] streamers = new UnityEngine.Video.VideoPlayer[4];
        for(int i=0; i<streamers.Length; i++)
        {
            streamers[i] = controller.AddComponent<UnityEngine.Video.VideoPlayer>();
            streamers[i].renderMode = UnityEngine.Video.VideoRenderMode.MaterialOverride;
            streamers[i].targetMaterialRenderer = panels[i].GetComponent<Renderer>();
            streamers[i].targetMaterialProperty = "_MainTex";
            streamers[i].audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.None;
            streamers[i].isLooping = true;
            streamers[i].clip = videoToPlay;
            streamers[i].time = startTime;
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
