using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public partial class SystemInfoChecker : MonoBehaviour {
    [Header("Info: 360Video Capability")]
    [SerializeField]
    private Cardboard360VideoCapability my360VideoCapability = Cardboard360VideoCapability.Unknown;

    private VideoPlayer myVideoPlayer;

    #region default EventHandler
    public void DedaultVplayerErr(VideoPlayer source, string message)
    {
        Debug.LogError("Video Preparation Failed");
        my360VideoCapability = Cardboard360VideoCapability.PictureMode;
        myVideoPlayer.errorReceived -= DedaultVplayerErr;
    }
    #endregion

    public bool SetTestVideo()
    {
        if (testVideoClip == null && testVideoURL == null)
        {
            Debug.LogError("Test File not set");
            return false;
        }

        if (testVideoURL.Length > 0)
        {
            myVideoPlayer.source = VideoSource.Url;
            myVideoPlayer.url = testVideoURL;
        }
        else
        {
            myVideoPlayer.source = VideoSource.VideoClip;
            myVideoPlayer.clip = testVideoClip;
        }
        return true;
    }

    public IEnumerator TestWithVideoPlayer()
    {
        if (my360VideoCapability == Cardboard360VideoCapability.PictureMode)
            yield break;

        myVideoPlayer = gameObject.AddComponent<VideoPlayer>();

        if (!SetTestVideo())
        {
            Debug.LogError("Unable to test with VideoPlayer");
            yield break;
        }

        myVideoPlayer.errorReceived += DedaultVplayerErr;
        myVideoPlayer.Prepare();
        yield return null;

        while (!myVideoPlayer.isPrepared)
        {
            yield return null;
        }

        myVideoPlayer.errorReceived -= DedaultVplayerErr;
    }

    public void TestCardboard360VideoCapability()
    {
        my360VideoCapability = Cardboard360VideoCapability.Testing;

        // wait for CheckSystemMem();
        if (mySystemMemory == SystemMemoryCapability.Unknown)
            CheckSystemMem();

        switch (mySystemMemory)
        {
            case SystemMemoryCapability.LowerThanOne:
                my360VideoCapability = Cardboard360VideoCapability.PictureMode;
                break;
            case SystemMemoryCapability.BetweenOneTwo:
                my360VideoCapability = Cardboard360VideoCapability.VideoMode;
                break;
            case SystemMemoryCapability.LargetThanTwo:
                my360VideoCapability = Cardboard360VideoCapability.VideoMode;
                break;
            default:
                Debug.LogErrorFormat("Unexpected Status: {0}", mySystemMemory);
                break;
        }

        StartCoroutine(TestWithVideoPlayer());
    }

    public enum Cardboard360VideoCapability
    {
        Unknown,
        Testing,
        VideoMode,
        PictureMode
    }
}
