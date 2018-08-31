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

    /// <summary>
    /// Directly load ABM from Disk
    /// </summary>
    /// <param name="_forceReload"></param>
    public IEnumerator LoadABMFromDisk(Action _failAct ,bool _force = false) {

        Debug.Log("Loading ABM");
        if(!_force && myABM != null)
            yield break;

        //Load the manifest
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(ABM_PATH);
        if (manifestBundle == null)
        {
            Debug.LogError("No ABM in Disk");
            _failAct();
            yield break;
        }

        myABM = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        yield return null;
        yield return null;
        yield return null;
        manifestBundle.Unload(true);
    }

    /// <summary>
    /// Asset Bundle Manifest Download then save to disk
    /// </summary>
    /// <param name="_fileURL"></param>
    /// <returns></returns>
    public IEnumerator ABMDownloader(FileURL _fileURL)
    {
        //Always download ABM !!
        Debug.Log("Try to Download ABM " + _fileURL);
        using (UnityWebRequest _uwr = UnityWebRequest.Get(_fileURL.fullURL))
        {
            _uwr.timeout = 60; // manifest MUST be download in 60 sec Since it is smaller than 1 MB
            yield return _uwr.SendWebRequest();

            if (_uwr.isNetworkError || _uwr.isHttpError)
            {
                Debug.LogError("ABM Download Failed " + _uwr.isNetworkError);
                yield break;
            }

            Debug.Log("ABM Downloaded to disk");
            if (_uwr.downloadHandler.data != null)
                File.WriteAllBytes(_fileURL.localPath, _uwr.downloadHandler.data);
            yield return null;
            yield return null;
            yield return null;
        }
    }
}

[Serializable]
public class FileURL
{
    public string fullURL;
    public string fileName;
    public string localPath;

    public FileURL(string _url, string _fName, string _lPath)
    {
        fullURL = _url;
        fileName = _fName;
        localPath = _lPath;
    }

    public FileURL(string _url, string _fName): this(_url, _fName, "")
    {
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

    public VideoURL(string _url, string _abName, string _lPath, string _vName) : base(_url, _abName, _lPath)
    {
        videoName = _vName;
    }

    public VideoURL(string _url, string _abName, string _videoName) : this(_url, _abName, "", _videoName)
    {
    }
}