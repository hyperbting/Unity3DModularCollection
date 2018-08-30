using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemInfoChecker : MonoBehaviour {

    [Header("Debug Info")]
    [SerializeField]
    private SystemMemoryCapability mySystemMemory = SystemMemoryCapability.Unknown;
    [SerializeField]
    private UnityWebRequestHelper.InternetConnectionCapability myInternetConnection = UnityWebRequestHelper.InternetConnectionCapability.Unknown;

    [Header("Settings")]
    public bool checkSystemMemoryAtStart;
    public bool checkInternetConnectionAtStart;

    [Header("Settings-Internet Check Address")]
    [SerializeField]
    [Tooltip("Address to check")]
    private List<string> checkerAddress = new List<string>() { "https://www.google.com", "http://baidu.com" };

    private UnityWebRequestHelper myUWRHelper;

    // Use this for initialization
    void Start () {

        myUWRHelper = gameObject.AddComponent(typeof(UnityWebRequestHelper)) as UnityWebRequestHelper;

        if (checkSystemMemoryAtStart)
            CheckSystemMem();

        if (checkInternetConnectionAtStart)
            CheckInternetConnection();
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

    public void CheckInternetConnection()
    {
        StartCoroutine(myUWRHelper.CheckInternetConnection(checkerAddress));
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
