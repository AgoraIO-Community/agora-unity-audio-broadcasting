using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class AgoraProfile : Photon.PunBehaviour
{
    [Header("Agora Properties")]
    [SerializeField]
    private string channel;
    private string originalChannel;
    [SerializeField]
    private uint myUID = 0;
    [SerializeField]
    CLIENT_ROLE_TYPE myClientRole;
    [Header("UI Elements")]
    [SerializeField]
    private Text text_ChannelName;
    [SerializeField]
    private GameObject chatBubble;
    
    void Start()
    {
        if (photonView.isMine)
        {
            AgoraEngine.mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
            AgoraEngine.mRtcEngine.OnUserJoined = OnUserJoinedHandler;
            AgoraEngine.mRtcEngine.OnLeaveChannel = OnLeaveChannelHandler;
            AgoraEngine.mRtcEngine.OnUserOffline = OnUserOfflineHandler;
            AgoraEngine.mRtcEngine.OnClientRoleChanged = OnClientRoleChangedHandler;
            AgoraEngine.mRtcEngine.OnVolumeIndication = OnVolumeChangedHandler;

            myClientRole = CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE;

            isSmoothingTalkBubble = false;

            SetChatBubbleVisibility();
        }
    }

    public void JoinChannel()
    {
        AgoraEngine.mRtcEngine.EnableAudioVolumeIndication(250, 3, true);

        channel = text_ChannelName.text;
        AgoraEngine.mRtcEngine.JoinChannel(text_ChannelName.text, null, 0);
    }

    #region Agora Callbacks
    // Local Client Joins Channel.
    private void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        if (!photonView.isMine)
            return;

        Debug.Log("Local user joined - uid: " + uid);
        myUID = uid;
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

        Debug.Log("Local user left channel");
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

    private void OnVolumeChangedHandler(AudioVolumeInfo[] speakers, int speakerNumber, int totalVolume)
    {
        if(speakers != null)
        {
            int speakerLength = speakers.Length;

            for(int i = 0; i < speakerLength; i++)
            {
                print("speaker: " + i);
                if(speakers[i].uid == 0)
                {
                    if(speakers[i].vad == 0)
                    {
                        isLocalPlayerTalking = false;
                    }
                    else
                    {
                        isLocalPlayerTalking = true;
                        StartCoroutine(SpeechBubbleSmoothing());
                        isChatBubbleVisible = true;
                        //chatBubble.SetActive(true);
                        photonView.RPC("NetworkedChatBubbleVisibility", PhotonTargets.All);
                        SetChatBubbleVisibility();
                    }
                }
            }
        }
    }
    #endregion

    private bool isLocalPlayerTalking = false;
    [SerializeField] private float talkBubbleBuffer = .75f;
    private bool isSmoothingTalkBubble = false;
    private bool isChatBubbleVisible = false;

    private IEnumerator SpeechBubbleSmoothing()
    {
        if(isSmoothingTalkBubble == true)
        {
            yield break;
        }

        isSmoothingTalkBubble = true;
        float talkBubbleDisableTimer = talkBubbleBuffer;

        while(talkBubbleDisableTimer > 0f)
        {
            if(isLocalPlayerTalking)
            {
                talkBubbleDisableTimer = talkBubbleBuffer;
            }
            talkBubbleDisableTimer -= Time.deltaTime;

            yield return null;
        }

        isChatBubbleVisible = false;
        //SetChatBubbleVisibility();
        //chatBubble.SetActive(false);
        photonView.RPC("DisableSpeechBubble", PhotonTargets.All);
        isSmoothingTalkBubble = false;
    }

    public void SetChatBubbleVisibility()
    {
        photonView.RPC("NetworkedChatBubbleVisibility", PhotonTargets.All);   
    }

    [PunRPC]
    public void DisableSpeechBubble()
    {
        chatBubble.SetActive(false);
    }

    [PunRPC]
    public void NetworkedChatBubbleVisibility()
    {
        print("my uid: " + myUID);

        chatBubble.SetActive(true);

        //vikingMesh.material = broadcasterMaterial;
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        SetChatBubbleVisibility();
    }
    

    private IEnumerator OnLeftRoom()
    {
        //Wait untill Photon is properly disconnected (empty room, and connected back to main server)
        while (PhotonNetwork.room != null || PhotonNetwork.connected == false)
            yield return 0;

        IRtcEngine.Destroy();
    }
}
