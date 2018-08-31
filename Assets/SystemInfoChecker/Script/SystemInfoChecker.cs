using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemInfoChecker: MonoBehaviour {

    [Header("Info")]
    [SerializeField]
    private SystemMemoryCapability mySystemMemory = SystemMemoryCapability.Unknown;
    [SerializeField]
    private UnityWebRequestHelper.InternetConnectionCapability myInternetConnection = UnityWebRequestHelper.InternetConnectionCapability.Unknown;

    [Header("Settings")]
    public bool checkSystemMemoryAtStart;
    public bool checkInternetConnectionAtStart;

    [Header("Settings: Internet Check Address")]
    [SerializeField]
    [Tooltip("Address to check")]
    private List<string> checkerAddress = new List<string>() { "https://www.google.com", "http://baidu.com" };

    private UnityWebRequestHelper myUWRHelper;

    // Use this for initialization
    void Start () {

        myUWRHelper = gameObject.AddComponent(typeof(UnityWebRequestHelper)) as UnityWebRequestHelper;
        ResetStatus();
        ProcessOnStart();
    }

    private void ResetStatus()
    {
        mySystemMemory = SystemMemoryCapability.Unknown;
        myInternetConnection = UnityWebRequestHelper.InternetConnectionCapability.Unknown;
    }

    private void ProcessOnStart()
    {
        if (checkSystemMemoryAtStart)
            CheckSystemMem();

        if (checkInternetConnectionAtStart)
            StartCoroutine(myUWRHelper.CheckInternetConnection(checkerAddress));
    }

    public void CheckSystemMem()
    {
        mySystemMemory = SystemMemoryCapability.Testing;
        int sysMem = SystemInfo.systemMemorySize;

        if (sysMem <= 1024)
            mySystemMemory = SystemMemoryCapability.LowerThanOne;
        else if (sysMem <= 2048)
            mySystemMemory = SystemMemoryCapability.BetweenOneTwo;
        else
            mySystemMemory = SystemMemoryCapability.LargetThanTwo;
    }

    public enum SystemMemoryCapability
    {
        Unknown,
        Testing,
        LowerThanOne,
        BetweenOneTwo,
        LargetThanTwo
    }
}
