using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (AssetBundleKeeper))]
public class ABKUser : MonoBehaviour {

    public AssetBundleKeeper myABK;

    // Use this for initialization
    IEnumerator Start ()
    {
        //Setup
        myABK.ABM_PATH = new FileURL()
        {
            fullURL = "https://s3-ap-northeast-1.amazonaws.com/conciegeplusstorage/AssetBundle/" + SystemInfoChecker.GetPlatformName() + "/"  + SystemInfoChecker.GetPlatformName(),
            fileName = SystemInfoChecker.GetPlatformName(),
            localPath = Application.persistentDataPath+"/ABM"
        };

        yield return null;

        //download ABM
        myABK.DownloadABM();

        while (!myABK.IsManifestReady())
            yield return null;

        Debug.Log("ABM is ready");
        
        foreach (var name in myABK.GetABNames())
            Debug.Log(name);

    }
}
