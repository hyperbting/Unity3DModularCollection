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

    public IEnumerator LoadGameObjetFromAB(ABFileURL _fileURL, List<ObjectNamePosition> _GameObjDesc)
    {
        bool loadingGO = true;

        var nameArr = new string[_GameObjDesc.Count];
        for (int i = 0; i < _GameObjDesc.Count; i++)
            nameArr[i] = _GameObjDesc[i].name;

        yield return StartCoroutine(CoreABDownloader(
            _fileURL,
            (UnityWebRequest _uwr) => {
                StartCoroutine(AsyncLoadFromAB<GameObject>(_uwr,
                    (List<UnityEngine.Object> _objs) =>
                    {
                        for (int i = 0; i < _objs.Count; i++)
                            Instantiate((_objs[i] as GameObject), _GameObjDesc[i].position, Quaternion.identity);

                        loadingGO = false;
                    },
                    nameArr
                ));
            },
            null,
            null
            ));

        while (loadingGO)
            yield return null;

        Debug.Log("LoadGameObjetFromAB Finished");
    }

    public IEnumerator LoadTextureFromAB(ABFileURL _fileURL, Action<List<Texture2D>> _textureDealer, params string[] _textureNames)
    {
        bool loadingTexture = true;
        // Check AB already in Disk
        yield return StartCoroutine(CoreABDownloader(
            _fileURL,
            (UnityWebRequest _uwr) => {
                StartCoroutine(AsyncLoadFromAB<Texture>(_uwr,
                    (List<UnityEngine.Object> _txts) =>
                    {
                        var result = new List<Texture2D>();
                        for (int i = 0; i < _txts.Count; i++)
                            result.Add(_txts[i] as Texture2D);
                        _textureDealer(result);
                        loadingTexture = false;
                    },
                    _textureNames
                ));
            },
            null,
            null
            ));

        while (loadingTexture)
            yield return null;

        Debug.Log("Load Textures Finished");
    }

    public IEnumerator LoadSpriteAtlasFromAB(ABFileURL _fileURL, Action<List<SpriteAtlas>> _saDealer, params string[] _spriteAltasNames)
    {
        bool loadingSA = true;
        // Check AB already in Disk
        yield return StartCoroutine(CoreABDownloader(
            _fileURL,
            (UnityWebRequest _uwr) => {
                StartCoroutine(AsyncLoadFromAB<SpriteAtlas>(_uwr,
                    (List<UnityEngine.Object> _objs) =>
                    {
                        var sas = new List<SpriteAtlas>();
                        for(int i = 0; i < _objs.Count; i++)
                            sas.Add( _objs[i] as SpriteAtlas);

                        _saDealer(sas);
                        loadingSA = false;
                    },
                    _spriteAltasNames
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
    /// <summary>
    /// Load specific Type T By Name _Names
    /// </summary>
    /// <typeparam name="T">Specific type to load</typeparam>
    /// <param name="_uwr"></param>
    /// <param name="_end">What to do for these loaded objects</param>
    /// <param name="_Names">Names of Specific type object</param>
    /// <returns></returns>
    IEnumerator AsyncLoadFromAB<T>(UnityWebRequest _uwr, Action<List<UnityEngine.Object>> _end, params string[] _Names)
    {
        var result = new List<UnityEngine.Object>();

        AssetBundle ab = DownloadHandlerAssetBundle.GetContent(_uwr);
        if (ab != null)
        {
            tmpAB = ab;
            SpriteAtlasManager.atlasRequested += RequestLateBindingAtlas;

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

        SpriteAtlasManager.atlasRequested -= RequestLateBindingAtlas;
        tmpSA = null;
        tmpAB = null;

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
    IEnumerator CoreABDownloader(ABFileURL _fileURL, Action<UnityWebRequest> _success=null, Action<string> _fail=null, Action<float> _progress=null)
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
public struct ABFileURL
{
    /// <summary>
    /// file url, including filename and file extension
    /// </summary>
    public string fullURL;

    public string fileName;

    public override string ToString()
    {
        return fullURL;
    }
}

[Serializable]
public class FileURL
{
    /// <summary>
    /// file url, including filename and file extension
    /// </summary>
    public string fullURL;

    /// <summary>
    /// if this file have to save to disk
    /// </summary>
    public string localPath;

    public void Setup(string _baseurl, string _platform, string _fileName, string _localPath)
    {
        fullURL = _baseurl + _platform + "/" + _fileName;
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