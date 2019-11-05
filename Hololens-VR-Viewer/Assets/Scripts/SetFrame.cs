using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFrame : MonoBehaviour
{
    public double startTime;
    public UnityEngine.Video.VideoClip videoToPlay;
    public GameObject[] panels;

    // Start is called before the first frame update
    void Start()
    {
        //Associate the Streamer to the Video Controller
        GameObject controller = GameObject.Find("Video Controller");

        //Build the Renderers
        int totalPanels = panels.Length;
        UnityEngine.Video.VideoPlayer[] streamers = new UnityEngine.Video.VideoPlayer[totalPanels];
        for(int i=0; i< totalPanels; i++)
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
