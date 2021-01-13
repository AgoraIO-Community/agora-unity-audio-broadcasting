using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;

public class AgoraProfile : Photon.MonoBehaviour
{
    [Header("Agora Properties")]
    [SerializeField]
    private string channel = "unity3d";
    private string originalChannel;
    [SerializeField]
    private uint myUID = 0;
    [SerializeField]
    CLIENT_ROLE_TYPE myClientRole;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.isMine)
        {
            AgoraEngine.mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
            AgoraEngine.mRtcEngine.OnUserJoined = OnUserJoinedHandler;
            AgoraEngine.mRtcEngine.OnLeaveChannel = OnLeaveChannelHandler;
            AgoraEngine.mRtcEngine.OnUserOffline = OnUserOfflineHandler;
            AgoraEngine.mRtcEngine.OnClientRoleChanged = OnClientRoleChangedHandler;
        }
    }

    public void JoinChannel()
    {
        AgoraEngine.mRtcEngine.JoinChannel(channel, null, 0);
    }

    #region Agora Callbacks
    // Local Client Joins Channel.
    private void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        if (!photonView.isMine)
            return;

        myUID = uid;

        if (myClientRole == CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER)
        {
            
        }
    }

    // Remote Client Joins Channel.
    private void OnUserJoinedHandler(uint uid, int elapsed)
    {
        if (!photonView.isMine)
            return;

        Debug.Log("Remote user joined - uid: " + uid);
    }

    // Local user leaves channel.
    private void OnLeaveChannelHandler(RtcStats stats)
    {
        if (!photonView.isMine)
            return;

        
    }

    // Remote User Leaves the Channel.
    private void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        if (!photonView.isMine)
            return;

        Debug.Log("Remote user left - uid: " + uid);
    }

    private void OnClientRoleChangedHandler(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
    {
        myClientRole = newRole;
    }
    #endregion

    private IEnumerator OnLeftRoom()
    {
        //Wait untill Photon is properly disconnected (empty room, and connected back to main server)
        while (PhotonNetwork.room != null || PhotonNetwork.connected == false)
            yield return 0;

        IRtcEngine.Destroy();
    }
}
