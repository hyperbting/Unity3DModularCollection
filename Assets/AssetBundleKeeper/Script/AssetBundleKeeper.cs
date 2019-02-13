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

    #region Loader Wrappers based on CoreABDownloader/ CoreDownloader
    public void DownloadABM()
    {
        //download ABM
        StartCoroutine(CoreDownloader(ABM_PATH,
            (byte[] _bytes) => {

                //load ABM after ABM is downloaded
                StartCoroutine(LoadABMFromDisk());
            }
        ));
    }

    /// <summary>
    /// Directly load ABM from Disk
    /// </summary>
    /// <param name="_forceReload"></param>
    public IEnumerator LoadABMFromDisk(bool _force=false, Action _failAct=null)
    {
        Debug.Log("Loading ABM");
        if (_force)
        {
            DestroyImmediate(myABM, true);
            myABM = null;
        }

        if (myABM != null)
            yield break;

        //Load the manifest
        var abcr = AssetBundle.LoadFromFileAsync(ABM_PATH.localPath);
        yield return abcr;

        AssetBundle manifestBundle = abcr.assetBundle;
        if (manifestBundle == null)
        {
            Debug.LogError("No AB holding ABM in Disk");
            if(_failAct!=null)
                _failAct(); 
            yield break;
        }

        var abr = manifestBundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
        yield return abr;

        myABM = abr.asset as AssetBundleManifest;

        yield return null;
        yield return null;
        yield return null;
        manifestBundle.Unload(false);
    }

    public IEnumerator LoadGameObjetFromAB(FileURL _fileURL, List<ObjectNamePosition> _GameObjDesc)
    {
        bool loadingGO = true;

        yield return StartCoroutine(CoreABDownloader(
            _fileURL,
            (UnityWebRequest _uwr) => {

                StartCoroutine(AsyncLoadGameObjectsFromAB( _uwr, _GameObjDesc,
                    () => { loadingGO = false; }
                    )
                );

            },
            null,
            null
            ));

        while (loadingGO)
            yield return null;

        Debug.Log("LoadGameObjetFromAB Finished");
    }

    public IEnumerator LoadTextureFromAB(FileURL _fileURL, params string[] _textureNames)
    {
        bool loadingTexture = true;
        var result = new List<UnityEngine.Object>();
        // Check AB already in Disk
        yield return StartCoroutine(CoreABDownloader(
            _fileURL,
            (UnityWebRequest _uwr) => {
                StartCoroutine(AsyncLoadFromAB<Texture>(_uwr,
                    (List<UnityEngine.Object> _txts) =>
                    {
                        loadingTexture = false;
                        result = _txts;
                    },
                    _textureNames
                ));
            },
            null,
            null
            ));

        while (loadingTexture)
            yield return null;

        yield return null;

        Debug.Log("Load Textures Finished");
    }

    public IEnumerator LoadSpriteAtlasFromAB(FileURL _fileURL, string _spriteAltasName)
    {
        bool loadingSA = true;

        // Check AB already in Disk
        yield return StartCoroutine(CoreABDownloader(
            _fileURL,
            (UnityWebRequest _uwr) => 
            {
                StartCoroutine(AsyncLoadSpriteAtlasFromAB(_uwr, 
                    _spriteAltasName, 
                    ()=> 
                    {
                        loadingSA = false;
                    }
                ));
            },
            null,
            null
            ));

        while (loadingSA)
            yield return null;

        Debug.Log("LoadSpriteAtlasFromAB Finished");
    }
    #endregion

    #region Async Load From AB
    IEnumerator AsyncLoadGameObjectsFromAB(UnityWebRequest _uwr, List<ObjectNamePosition> _GODescription, Action _end)
    {
        AssetBundle ab = DownloadHandlerAssetBundle.GetContent(_uwr);
        if (ab != null)
        {
            tmpAB = ab;
            SpriteAtlasManager.atlasRequested += RequestLateBindingAtlas;

            // Instantiate each GO
            for (int i = 0; i < _GODescription.Count; i++)
            {

                var abr = ab.LoadAsset<GameObject>(_GODescription[i].name);
                yield return abr;

                var go = Instantiate(abr, _GODescription[i].position, Quaternion.identity);
                yield return null;
            }

            SpriteAtlasManager.atlasRequested -= RequestLateBindingAtlas;
            tmpSA = null;
            tmpAB = null;
            yield return null;

            ab.Unload(false);
        }
        _end();
    }

    IEnumerator AsyncLoadSpriteAtlasFromAB(UnityWebRequest _uwr, string _spriteAltasName, Action _end)
    {
        AssetBundle ab = DownloadHandlerAssetBundle.GetContent(_uwr);
        if (ab != null)
        {
            var abr = ab.LoadAssetAsync<SpriteAtlas>(_spriteAltasName);
            yield return abr;
            tmpSA = abr.asset as SpriteAtlas;

            yield return null;

            ab.Unload(false);
        }
        _end();
    }

    //IEnumerator AsyncLoadTextureFromAB(UnityWebRequest _uwr, Action<List<Texture>> _end, params string[] _textureNames)
    //{

    //    var result = new List<Texture>();

    //    AssetBundle ab = DownloadHandlerAssetBundle.GetContent(_uwr);
    //    if (ab != null)
    //    {
    //        for (int i = 0; i < _textureNames.Length; i++)
    //        {
    //            var abr = ab.LoadAssetAsync<Texture2D>(_textureNames[i]);
    //            yield return abr;

    //            if (abr.asset != null)
    //                result.Add(abr.asset as Texture2D);
    //        }
            
    //        yield return null;

    //        ab.Unload(false);
    //    }

    //    _end(result);
    //}

    IEnumerator AsyncLoadFromAB<T>(UnityWebRequest _uwr, Action<List<UnityEngine.Object>> _end, params string[] _Names)
    {
        var result = new List<UnityEngine.Object>();

        AssetBundle ab = DownloadHandlerAssetBundle.GetContent(_uwr);
        if (ab != null)
        {
            for (int i = 0; i < _Names.Length; i++)
            {
                var abr = ab.LoadAssetAsync<T>(_Names[i]);
                yield return abr;

                if (abr.asset != null)                   
                    result.Add(abr.asset);
            }

            yield return null;

            ab.Unload(false);
        }

        _end(result);

        yield return null;
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
public class ObjectNamePosition
{
    public string name;
    public Vector3 position;
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