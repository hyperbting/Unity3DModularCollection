using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleKeeper : MonoBehaviour {

    public AssetBundleManifest myABM;

    public IEnumerator LoadABMFromDisk(bool _forceReload = false) {
        Debug.Log("Loading ABM");

        if(!_forceReload && myABM != null)
            yield break;



    }

    /// <summary>
    /// Asset Bundle Manifest Downloader
    /// </summary>
    /// <param name="_filePath"></param>
    /// <param name="_actSuccess"></param>
    /// <param name="_actFailure"></param>
    /// <returns></returns>
    public IEnumerator ABMDownloader(string _fileURL)
    {
        Debug.Log("Try to Download ABM " + _fileURL);

        //Always download for ABM !
        using (UnityWebRequest _uwr = UnityWebRequest.GetAssetBundle(_fileURL))
        {
            _uwr.timeout = 60; // manifest MUST be download in 60 sec Since it is smaller than 1 MB
            yield return _uwr.SendWebRequest();

            if (!string.IsNullOrEmpty(_uwr.error))
            {
                yield break;
            }

            AssetBundle ab = ((DownloadHandlerAssetBundle)_uwr.downloadHandler).assetBundle;
            myABM = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            //save this AB to disk

            yield return null;
            yield return null;
            yield return null;
        }
    }
}
