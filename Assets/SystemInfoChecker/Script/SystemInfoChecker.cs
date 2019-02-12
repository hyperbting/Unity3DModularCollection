using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SystemInfoChecker: MonoBehaviour {

    [Header("Info")]
    [SerializeField]
    private SystemMemoryCapability mySystemMemory = SystemMemoryCapability.Unknown;
    [SerializeField]
    private UnityWebRequestHelper.InternetConnectionCapability myInternetConnection = UnityWebRequestHelper.InternetConnectionCapability.Unknown;

    private bool hasGyroScope = false;

    private UnityWebRequestHelper myUWRHelper;

    #region Delegate
    delegate void DefaultDele();
    #endregion
    DefaultDele fUpdateDefaultDele;


    // Use this for initialization
    void Start () {

        myUWRHelper = gameObject.AddComponent(typeof(UnityWebRequestHelper)) as UnityWebRequestHelper;
        ResetStatus();

        hasGyroScope = SystemInfo.supportsGyroscope;

        ProcessOnStart();

        fUpdateDefaultDele += delegate { Debug.LogError(Time.realtimeSinceStartup); };
    }

    private void Update()
    {
        

    }

    private void FixedUpdate()
    {
        if(fUpdateDefaultDele != null)
            fUpdateDefaultDele();
    }

    private void ResetStatus()
    {
        hasGyroScope = false;
        mySystemMemory = SystemMemoryCapability.Unknown;
        myInternetConnection = UnityWebRequestHelper.InternetConnectionCapability.Unknown;
    }

    private void ProcessOnStart()
    {
        if (checkSystemMemoryAtStart)
            CheckSystemMem();

        if (checkInternetConnectionAtStart)
            StartCoroutine(myUWRHelper.CheckInternetConnection(checkerAddress));

        if (check360CapabilityAtStart)
            TestCardboard360VideoCapability();
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
