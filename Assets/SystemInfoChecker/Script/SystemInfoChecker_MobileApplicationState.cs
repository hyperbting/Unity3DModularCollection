using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SystemInfoChecker : MonoBehaviour {

    //public bool isPaused;
    public ApplicationState appState;

    void OnApplicationFocus(bool hasFocus)
    {
        Debug.LogWarning("OnApplicationFocus " + hasFocus);

//        isPaused = !hasFocus;
        if (hasFocus)
        {
            appState = ApplicationState.InAppFocused;
        }

    }

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.LogWarning("OnApplicationPause " + pauseStatus);
        //isPaused = pauseStatus;
    }

    public enum ApplicationState {
        UnKnOwN,
        InAppFocused,
        InBackground
    }
}

