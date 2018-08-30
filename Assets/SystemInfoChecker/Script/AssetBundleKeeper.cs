using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleKeeper : MonoBehaviour {

    public AssetBundleManifest myABM;

    private string ABM_PATH;

    private void Start()
    {
        ABM_PATH = Application.persistentDataPath + "/ABM";
    }

    public void LoadABMFromDisk(bool _forceReload = false) {
        Debug.Log("Loading ABM");

        if(!_forceReload && myABM != null)
            return;

        //Load the manifest
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(ABM_PATH);
        if (manifestBundle == null)
        {
            Debug.LogError("No ABM in Disk");
            return;
        }

        myABM = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    /// <summary>
    /// Asset Bundle Manifest Download then save to disk
    /// </summary>
    /// <param name="_filePath"></param>
    /// <param name="_actSuccess"></param>
    /// <param name="_actFailure"></param>
    /// <returns></returns>
    public IEnumerator ABMDownloader(FileURL _fileURL)
    {
        Debug.Log("Try to Download ABM " + _fileURL);

        //Always download for ABM !
        using (UnityWebRequest _uwr = UnityWebRequest.Get(_fileURL.fullURL))
        {
            _uwr.timeout = 60; // manifest MUST be download in 60 sec Since it is smaller than 1 MB
            yield return _uwr.SendWebRequest();

            if (_uwr.isNetworkError || _uwr.isHttpError)
            {
                Debug.Log("ABM Download Failed " + _uwr.isNetworkError);
                yield break;
            }

            Debug.Log("ABM Downloaded");

            //save this ABM holding AB to disk
            if(_uwr.downloadHandler.data != null)
                File.WriteAllBytes(ABM_PATH, _uwr.downloadHandler.data);
            yield return null;
            yield return null;
            yield return null;
        }
    }

    //public IEnumerator ABDownloader(FileURL _fileURL, Action<string> _actFailure = null)
    //{

    //    Debug.Log("Try to Fetch Main AB");
    //    if (myABM == null)
    //    {
    //        Debug.LogError("myABM == null");
    //        yield break;
    //    }

    //    Hash128 h128 = myABM.GetAssetBundleHash(_fileURL.fileName);
    //    //Remove unwanted main when ABM is updated
    //    Caching.ClearOtherCachedVersions(_fileURL.fileName, h128);

    //    //either download or load from cache
    //    using (UnityWebRequest _uwr = UnityWebRequest.GetAssetBundle(_fileURL.fullURL, h128, 0))
    //    {
    //        Debug.Log("AB h128: " + h128);

    //        _uwr.SendWebRequest();
    //        yield return null;

    //        float lastProgress = 0f;
    //        int counter = 60;
    //        while (!_uwr.isDone)
    //        {
    //            if (lastProgress == _uwr.downloadProgress)
    //                counter--;
    //            else
    //            {
    //                counter = 60;
    //                lastProgress = _uwr.downloadProgress;
    //            }

    //            // wait too long, there is sth wrong with the Networking
    //            if (counter < 0)
    //                _uwr.Abort();

    //            yield return new WaitForSeconds(0.5f);
    //        }

    //        if (!String.IsNullOrEmpty(_uwr.error))
    //        {
    //            Debug.LogErrorFormat("ERR] {0}", _uwr.error);
    //            if (_actFailure != null)
    //                _actFailure(_uwr.error);
    //            downloaderCount--;
    //            yield break;
    //        }

    //        yield return null;

    //        SpriteAtlasManager.atlasRequested += RequestLateBindingAtlas;

    //        // Instantiate GOs immediately
    //        saab = ((DownloadHandlerAssetBundle)_uwr.downloadHandler).assetBundle;
    //        AssetBundleRequest abr = saab.LoadAllAssetsAsync<GameObject>();
    //        while (!abr.isDone)
    //            yield return null;

    //        foreach (var uObj in abr.allAssets)
    //            Instantiate(uObj as GameObject); // There will be GameObjects "PlaceInfo" and "LanguageInfo" to be Instantiated

    //        yield return null;
    //        yield return null;
    //        yield return null;

    //        saab.Unload(false);

    //        SpriteAtlasManager.atlasRequested -= RequestLateBindingAtlas;
    //        myProgressDelegate(myDRecordManager.GetRecord("CoreAB"), 1);
    //    }//www.Dispose();

    //    downloaderCount--;

    //    yield return null;

    //    Debug.Log("End Fetching Main AB");
    //}
}

[Serializable]
public class FileURL
{
    public string fullURL;
    public string fileName;

    public FileURL(string _url, string _fName)
    {
        fullURL = _url;
        fileName = _fName;
    }

    public override string ToString()
    {
        return fullURL;
    }
}

[Serializable]
public class VideoURL : FileURL
{
    public string videoName;

    public VideoURL(string _url, string _abName, string _vName) : base(_url, _abName)
    {
        videoName = _vName;
    }
}