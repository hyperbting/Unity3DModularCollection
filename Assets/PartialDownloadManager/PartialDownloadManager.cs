using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartialDownloadManager : MonoBehaviour {
    #region Delegate
    public delegate void FloatDelegate(float _fl);
    #endregion
    public FloatDelegate myProgressDelegate;

    [Header("Download Events Behaviour")]
    [Tooltip("Behaviour when local file size is LARGER than remote file size.")]
    public UnityWebRequestHelper.DownloadProcess localLarger = UnityWebRequestHelper.DownloadProcess.RedownloadFromBeginning;
    [Tooltip("Behaviour when local file size is SMALLER than remote file size.")]
    public UnityWebRequestHelper.DownloadProcess localSmaller = UnityWebRequestHelper.DownloadProcess.Resume;

    private UnityWebRequestHelper myUnityWebRequestHelper;

    [Header("Debug Info")]
    [SerializeField]
    private bool downloading;

    private FileIOHelper myFileIOHelper;

    protected void OnEnable()
    {
        myProgressDelegate += DefaultProgressEventHandler;
    }

    protected void OnDisable()
    {
        myProgressDelegate -= DefaultProgressEventHandler;
    }

    protected void Start()
    {
        myFileIOHelper = new FileIOHelper();
    }

    #region EventHandler
    private void DefaultProgressEventHandler(float _progress)
    {
        Debug.LogFormat("Downloading... {0}", _progress);
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_url">Target URL</param>
    /// <param name="_localPath">Path to Local File</param>
    /// <param name="_resultAct">First int is Local File Size; Second int tells Remote File Size</param>
    /// <returns></returns>
    public IEnumerator DownloadBehaviourCheck(FileURL _fileUrl, Action<UnityWebRequestHelper.DownloadProcess, int, int> _resultAct)
    {
        int localFileSize = myFileIOHelper.CheckFileSize(_fileUrl.localPath);

        bool processed = false;
        bool supportPartialDL = false;
        int remoteFileSize = -1;

        StartCoroutine(myUnityWebRequestHelper.CheckFileSize(_fileUrl.fullURL,
            (int _val, bool _supportPartialDL) =>
            {
                remoteFileSize = _val;
                supportPartialDL = _supportPartialDL;
                processed = true;
            }
            ));

        while (!processed)
            yield return null;

        //check remote server support
        if (!supportPartialDL)
        {
            //not support partial download
            Debug.Log("Remote Server Not Support Partial Download");

            _resultAct(UnityWebRequestHelper.DownloadProcess.RedownloadFromBeginning, localFileSize, remoteFileSize);
            yield break;
        }

        _resultAct(DownloadBehaviourCheck(localFileSize, remoteFileSize), localFileSize, remoteFileSize);
    }

    protected UnityWebRequestHelper.DownloadProcess DownloadBehaviourCheck(int localFileSize, int remoteFileSize)
    {
        if (remoteFileSize <= 0)//remote file not exist
        {
            Debug.Log("Remote File Not Found");
            return UnityWebRequestHelper.DownloadProcess.DoNothing;
        }

        if (localFileSize < 0)//local file not exist
        {
            return UnityWebRequestHelper.DownloadProcess.Resume;
        }

        if (localFileSize < remoteFileSize)
        {
            Debug.Log("Local < Remote");
            return localSmaller;
        }
        else if (localFileSize > remoteFileSize)//file must changed
        {
            Debug.Log("Local > Remote");
            return localLarger;
        }
        else//file size Matched?! job done here
        {
            Debug.Log("File Size Matched");
            return UnityWebRequestHelper.DownloadProcess.DoNothing;
        }
    }

    protected IEnumerator DownloadWholeFile(FileURL _fileUrl, int _windowSize)
    {
        int rfsize = -1;// remote file size
        int lfsize = -1;// local file size
        var pd = UnityWebRequestHelper.DownloadProcess.BeforeProcess;

        //check remote file status
        StartCoroutine(DownloadBehaviourCheck(_fileUrl,
            (UnityWebRequestHelper.DownloadProcess _dp, int _local, int _remote) =>
            {
                pd = _dp;
                rfsize = _remote;
                lfsize = _local;
            }
            ));

        // wait until DownloadBehaviourCheck finished
        while (pd == UnityWebRequestHelper.DownloadProcess.BeforeProcess)
            yield return null;

        Debug.LogFormat("remote file size {0}; local file size {1}; Next Process will be {2}", rfsize, lfsize, pd);

        switch (pd)
        {
            case UnityWebRequestHelper.DownloadProcess.RedownloadFromBeginning:
                myFileIOHelper.Remove(_fileUrl.localPath); //Delete TargetFile first
                lfsize = -1;
                break;
            case UnityWebRequestHelper.DownloadProcess.Resume: //just keep doing
                break;
            case UnityWebRequestHelper.DownloadProcess.DoNothing:
            default:
                Debug.Log("Do Nothing");
                yield break;
        }

        // ONLY ONE of this DownloadWholeFile() can be processed at any given time
        if (downloading)
            yield return new WaitForSeconds(1f);

        downloading = true;
        for (int i = lfsize + 1; i < rfsize; i += _windowSize)
        {
            yield return StartCoroutine(
                myUnityWebRequestHelper.DownloadParts(
                    (byte[] _bytes) => {
                        myFileIOHelper.AppendTo(_fileUrl.localPath, _bytes);
                        myProgressDelegate((float)i / (float)rfsize);
                    }, _fileUrl.fullURL, i, _windowSize)
            );
        }
        myProgressDelegate(1);
        downloading = false;
    }
}
