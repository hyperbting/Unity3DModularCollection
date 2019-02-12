using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SystemInfoChecker : MonoBehaviour {
    [Header("Settings")]
    public bool checkSystemMemoryAtStart;
    public bool checkInternetConnectionAtStart;
    public bool check360CapabilityAtStart;

    [Header("Settings: Internet Check Address")]
    [SerializeField]
    [Tooltip("Address to check")]
    private List<string> checkerAddress = new List<string>() { "https://www.google.com", "http://baidu.com" };

    [Header("Settings: 360Video Capability")]
    public UnityEngine.Video.VideoClip testVideoClip;
    public string testVideoURL;

}
