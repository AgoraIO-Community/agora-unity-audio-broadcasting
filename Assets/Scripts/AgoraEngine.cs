using UnityEngine;
using agora_gaming_rtc;

public class AgoraEngine : MonoBehaviour
{
    [Header("Agora Properties")]
    [SerializeField]
    private string appID = "";
    public static IRtcEngine mRtcEngine;

    void Start()
    {
        if(mRtcEngine != null)
        {
            IRtcEngine.Destroy();
        }

        // Initialize Agora engine
        mRtcEngine = IRtcEngine.GetEngine(appID);
    }

    // Cleaning up the Agora engine during OnApplicationQuit() is an essential part of the Agora process with Unity. 
    private void OnApplicationQuit()
    {
        TerminateAgoraEngine();
    }

    public static void TerminateAgoraEngine()
    {
        if (mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            mRtcEngine = null;
            IRtcEngine.Destroy();
        }
    }
}