using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;

public class Agora_Reciever : MonoBehaviour
{

    //Variables for Agora Reciever
    private IRtcEngine mRtcEngine;
    // Start is called before the first frame update
    void Start()
    {
        mRtcEngine = IRtcEngine.GetEngine("84048bb3975f4e9aba267303d21e1df1");

        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.JoinChannelByKey(null, "remote", null, 2);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void join(string channel)
    {
        if (mRtcEngine == null)
            return;
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;

        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.JoinChannel(channel, null, 0);
        Debug.Log("initializeEngine done");
    }

    private void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("JoinChannelSuccessHandler: uid = " + uid);
    }

    private void OnUserJoined(uint uid, int elapsed)
    {
        Debug.Log("onUserJoined: uid = " + uid + " elapsed = " + elapsed);
        GameObject go = GameObject.Find("remotePreviewStreamPanel");
        if (!ReferenceEquals(go, null))
        {
            return;
        }

        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        if (!ReferenceEquals(videoSurface, null))
        {
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            videoSurface.SetGameFps(30);
        }
    }

    private void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        // remove video stream
        Debug.Log("onUserOffline: uid = " + uid + " reason = " + reason);
    }
}
