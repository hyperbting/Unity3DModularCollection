using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequestHelper : MonoBehaviour {

    public delegate void InternetConnectionCapabilityDelegate(InternetConnectionCapability _fl);
    public InternetConnectionCapabilityDelegate myInteConnCapabilityDelegate;

    [Header("Debug Info")]

    [SerializeField]
    private InternetConnectionCapability myInternetConnection = InternetConnectionCapability.Unknown;

    /// <summary>
    /// UnityWebRequest Version of get header
    /// </summary>
    /// <param name="_url"></param>
    /// <param name="_resultAct"></param>
    /// <returns></returns>
    public IEnumerator CheckFileSize(string _url, Action<int, bool> _resultAct)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Head(_url))
        {
            uwr.timeout = 60;
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                _resultAct(-(int)uwr.responseCode, false);
            }
            else
            {
                string result = uwr.GetResponseHeader("Content-Length");
                _resultAct(int.Parse(result), uwr.GetResponseHeaders().ContainsKey("Accept-Ranges"));
            }
        }
    }

    public IEnumerator CheckServerSupportPartialContent(string _url, Action<bool> _resultAct, Action<string> _errorAct = null)
    {
        using (UnityWebRequest www = UnityWebRequest.Head(_url))
        {
            www.timeout = 60;
            yield return www.SendWebRequest();

            while (!www.isDone)
                yield return null;

            if (www.isNetworkError || www.isHttpError)
            {
                if (_errorAct != null)
                    _errorAct(www.error);

                _resultAct(false);
            }
            else
            {
                _resultAct(www.GetResponseHeaders().ContainsKey("Accept-Ranges"));
            }
        }
    }

    public IEnumerator DownloadParts(Action<byte[]> _successAct, string _url, int _start, int _windowSize = 1048576, Action<float> _progressAct = null, Action<string> _errorAct = null)
    {
        DownloadHandler downloadHandler = new DownloadHandlerBuffer();

        using (UnityWebRequest www = new UnityWebRequest(_url, UnityWebRequest.kHttpVerbGET, downloadHandler, null))
        {
            www.SetRequestHeader("Range", string.Format("bytes={0}-{1}", _start, (_start + _windowSize - 1)));
            www.timeout = 60;
            yield return www.SendWebRequest();

            while (!www.isDone)
            {
                if (_progressAct != null)
                    _progressAct(www.downloadProgress);
                yield return null;
            }

            if (www.isNetworkError || www.isHttpError)
            {
                if (_errorAct != null)
                    _errorAct(www.error);
            }
            else
            {
                _successAct(www.downloadHandler.data);
            }
        }
    }

    //public IEnumerator Download(string _abName, Action<AssetBundle> _successAct, Action<float> _pregressAct = null, Action<string> _errorAct = null)
    //{
    //    Hash128 desiredH128 = GetHash128FromABMI(_fileUrl.fileName);

    //    //Clean up older AB in Cache
    //    Caching.ClearOtherCachedVersions(_abName, desiredH128);

    //    Debug.LogWarningFormat("Start to Download {0} from {1}", desiredH128, _fileUrl);
    //    using (UnityWebRequest _uwr = UnityWebRequest.GetAssetBundle(_fileUrl.fullURL, desiredH128, 0))
    //    {
    //        _uwr.SendWebRequest();

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
    //            //myProgressDelegate(myDRC, _uwr.downloadProgress * 0.9f);
    //        }
    //        // Debug.LogWarning("it is done");
    //        if (!String.IsNullOrEmpty(_uwr.error))
    //        {
    //            if (_actFailure != null)
    //                _actFailure(_uwr.error);
    //            downloaderCount--;
    //            myDRecordManager.ForceRemoveFinishedRecord(_fileUrl.fileName);
    //            yield break;
    //        }
    //    } //www.Dispose();

    //    yield return new WaitForSeconds(1f);
    //    // Debug.LogWarning("telling you it is one");
    //    myProgressDelegate(myDRC, 1);

    //    myDRecordManager.UpdateRecord(_fileUrl.fileName); //currentFinished ++;
    //    myDRecordManager.RemoveFinishedRecord(_fileUrl.fileName);
    //    downloaderCount--;

    //    yield return null;
    //    if (successAction != null)
    //        successAction();
    //}

    /// <summary>
    /// Download Whole thing from start to end
    /// </summary>
    /// <param name="_url"></param>
    /// <param name="_successAct"></param>
    /// <param name="_pregressAct"></param>
    /// <param name="_errorAct"></param>
    /// <returns></returns>
    public IEnumerator Download(string _url, Action<byte[]> _successAct, Action<float> _pregressAct = null, Action<string> _errorAct = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(_url))
        {
            request.timeout = 60;
            request.SendWebRequest();

            while (!request.isDone)
            {
                if (_pregressAct != null)
                    _pregressAct(request.downloadProgress);
                yield return null;
            }

            if (request.isNetworkError || request.isHttpError) // Error
            {
                if (_errorAct != null)
                    _errorAct(request.error);
                yield break;
            }

            _successAct(request.downloadHandler.data);
        }
    }

    #region Utility
    public IEnumerator CheckInternetConnection(List<string> _urls, Action<bool> _act = null)
    {
        if (myInternetConnection == InternetConnectionCapability.Testing)
            yield break;

        myInternetConnection = InternetConnectionCapability.Testing;

        foreach (string ur in _urls)
        {
            if (myInternetConnection == InternetConnectionCapability.Okay)
                yield break;

            yield return StartCoroutine(CheckInternetConnection(ur, _act));
        }
    }

    public IEnumerator CheckInternetConnection(string _url, Action<bool> _act = null)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(_url))
        {
            uwr.timeout = 30;
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                myInternetConnection = InternetConnectionCapability.ConnectionBlocked;

                if(_act != null)
                    _act(false);
            }
            else
            {
                myInternetConnection = InternetConnectionCapability.Okay;

                if (_act != null)
                    _act(true);
            }                
        }
    }
    #endregion

    public enum DownloadProcess
    {
        BeforeProcess,
        DoNothing,
        RedownloadFromBeginning,
        Resume
    }

    public enum InternetConnectionCapability
    {
        Unknown,
        Testing,
        Okay,
        ConnectionBlocked
    }
}
