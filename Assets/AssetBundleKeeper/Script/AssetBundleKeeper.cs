using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;

public class AssetBundleKeeper : MonoBehaviour
{
    public FileURL ABM_PATH;

    [Header("Debug Purpose")]
    [SerializeField]
    private AssetBundleManifest myABM;

    [SerializeField]
    private Texture tmpTexture;

    [SerializeField]
    private AssetBundle tmpAB;

    [SerializeField]
    private SpriteAtlas tmpSA;

    #region SpriteAtlas binder
    void RequestLateBindingAtlas(string _saName, Action<SpriteAtlas> _action)
    {
        Debug.LogWarning(_saName + " SA is working");
        tmpSA = tmpAB.LoadAsset<SpriteAtlas>(_saName);

        _action(tmpSA);
    }
    #endregion

    #region ABM Usage
    public string[] GetABNames()
    {
        return myABM.GetAllAssetBundles();
    }
    #endregion

    #region Downloader Wrapper
    public void DownloadABM()
    {
        //download ABM
        StartCoroutine(CoreDownloader(ABM_PATH, 
            (byte[] _bytes)=> {

                //load ABM after ABM is downloaded
                StartCoroutine(LoadABMFromDisk());
            }
        ));
    }
    #endregion

    #region Loaders are based on Core AB Downloader
    /// <summary>
    /// Directly load ABM from Disk
    /// </summary>
    /// <param name="_forceReload"></param>
    public IEnumerator LoadABMFromDisk(Action _failAct=null, bool _force = false)
    {

        Debug.Log("Loading ABM");
        if (!_force && myABM != null)
            yield break;

        //Load the manifest
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(ABM_PATH.localPath);
        if (manifestBundle == null)
        {
            Debug.LogError("No AB holding ABM in Disk");
            if(_failAct!=null)
                _failAct(); 
            yield break;
        }

        var abr = manifestBundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
        while (!abr.isDone)
            yield return null;
        myABM = abr.asset as AssetBundleManifest;

        yield return null;
        yield return null;
        yield return null;
        manifestBundle.Unload(true);
    }

    public IEnumerator LoadGameObjetFromAB(FileURL _fileURL, List<string> _GameObjectNames)
    {
        yield return StartCoroutine(CoreABDownloader(
            _fileURL,
            (UnityWebRequest _uwr) => {

                tmpAB = DownloadHandlerAssetBundle.GetContent(_uwr);
                SpriteAtlasManager.atlasRequested += RequestLateBindingAtlas;

                // Instantiate each GO
                for (int i = 0; i < _GameObjectNames.Count; i++)
                {
                    var tarGo = tmpAB.LoadAsset<GameObject>(_GameObjectNames[i]);
                    Instantiate(tarGo as GameObject);
                }

                SpriteAtlasManager.atlasRequested -= RequestLateBindingAtlas;
                tmpSA = null;
            },
            null,
            null
            ));
    }

    //// This one should be in either FileIOHelper or ?
    //public Texture2D LoadTextureFromDisk(string _path)
    //{
    //    // Create a texture. Texture size does not matter, since LoadImage will replace with with incoming image size.
    //    Texture2D tex = new Texture2D(320, 320, TextureFormat.RGB24, false, true)
    //    {// DXT5
    //        name = _path
    //    };

    //    if (!myFileIOHelper.CheckFileExist(_path))
    //    {
    //        Debug.LogWarning("Texture not found " + _path);
    //        return tex;
    //    }

    //    Debug.Log("LoadTextureFromDisk : " + _path);

    //    tex.LoadImage(myFileIOHelper.LoadbyteFromFile(_path)); // LoadImage will always RGBA32 for PNG/ RGB24 for JPG
    //    tex.Compress(true); // this will lower half the size but require some CPU power

    //    Debug.Log(tex.dimension + " " + tex.width + "x" + tex.height + " " + tex.format + " " + tex.filterMode);
    //    return tex;
    //}

    /// <summary>
    /// Wrapper of CoreABDownloader, grap texture from AB
    /// </summary>
    /// <param name="_fileURL"></param>
    /// <param name="_textureName"></param>
    /// <returns></returns>
    public IEnumerator LoadTextureFromAB(FileURL _fileURL, string _textureName)
    {
        // Check AB already in Disk
        yield return StartCoroutine(CoreABDownloader(
            _fileURL,
            (UnityWebRequest _uwr) => {
                AssetBundle ab = DownloadHandlerAssetBundle.GetContent(_uwr);
                if (ab != null)
                    tmpTexture = ab.LoadAsset<Texture2D>(_textureName);
                ab.Unload(false);
            },
            null,
            null
            ));
    }
    #endregion    

    #region TWO Core Downloader
    /// <summary>
    /// CoreABDownloader is BOTH AssetBundle DOWNLOADER and USER
    /// It checks AB_Hash with Cache then download Latest AB if it has to
    /// </summary>
    /// <param name="_fileURL"></param>
    /// <param name="_success"></param>
    /// <param name="_fail"></param>
    /// <param name="_progress"></param>
    IEnumerator CoreABDownloader(FileURL _fileURL, Action<UnityWebRequest> _success=null, Action<string> _fail=null, Action<float> _progress=null)
    {
        if (myABM == null)
            Debug.LogError("ABM not loaded!");

        Hash128 abHash = myABM.GetAssetBundleHash(_fileURL.fileName);

        //Clean up older AB in Cache
        Caching.ClearOtherCachedVersions(_fileURL.fileName, abHash);

        Debug.Log("Try to Download " + _fileURL);
        using (UnityWebRequest _uwr = UnityWebRequest.GetAssetBundle(_fileURL.fullURL, abHash, 0))
        {
            _uwr.SendWebRequest();

            yield return null;

            while (!_uwr.isDone)
            {
                if (_progress != null)
                    _progress(_uwr.downloadProgress);

                yield return null;
            }

            if (_uwr.isNetworkError || _uwr.isHttpError)
            {
                Debug.LogError("File Download Failed " + _uwr.error);
                if (_fail != null)
                    _fail(_uwr.error);
            }
            else
            {
                if (_success != null)
                    _success(_uwr);
            }

            yield return null;
            yield return null;
            yield return null;
        }
    }

    /// <summary>
    /// CoreDownloader is used to download ABM/ Video/ Texture
    /// SAVE to DISK directly
    /// </summary>
    /// <param name="_fileURL"></param>
    /// <param name="_fail"></param>
    /// <param name="_progress"></param>
    IEnumerator CoreDownloader(FileURL _fileURL, Action<byte[]> _success = null, Action<string> _fail=null, Action<float> _progress=null)
    {
        Debug.Log("Try to Download " + _fileURL);
        using (UnityWebRequest _uwr = UnityWebRequest.Get(_fileURL.fullURL))
        {
            _uwr.SendWebRequest();

            yield return null;

            while (!_uwr.isDone)
            {
                if (_progress != null)
                    _progress(_uwr.downloadProgress);

                yield return null;
            }

            if (_uwr.isNetworkError || _uwr.isHttpError)
            {
                Debug.LogError("File Download Failed " + _uwr.error);
                if (_fail != null)
                    _fail(_uwr.error);
            }
            else
            {
                Debug.Log("Downloaded: SAVE to DISK");
                if (_uwr.downloadHandler.data != null)
                {
                    File.WriteAllBytes(_fileURL.localPath, _uwr.downloadHandler.data);

                    yield return null;

                    if (_success != null)
                        _success(_uwr.downloadHandler.data);
                }
            }

            yield return null;
            yield return null;
            yield return null;
        }
    }
    #endregion

    #region status checker
    public bool IsManifestReady()
    {
        if (myABM == null)
            return false;

        return true;
    }
    #endregion
}

[Serializable]
public class FileURL
{
    /// <summary>
    /// file url, including filename and file extension
    /// </summary>
    public string fullURL;

    public string fileName;

    /// <summary>
    /// if this file have to save to disk
    /// </summary>
    public string localPath;

    public void Setup(string _baseurl, string _platform, string _localPath)
    {
        fullURL = _baseurl + _platform + "/" + _platform;
        fileName = _platform;
        localPath = _localPath;
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
}