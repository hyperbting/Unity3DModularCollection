using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequestHelper: MonoBehaviour {

    public delegate void InternetConnectionCapabilityDelegate(InternetConnectionCapability _fl);
    public InternetConnectionCapabilityDelegate myInteConnCapabilityDelegate;

    [Header("Debug Info")]

    [SerializeField]
    private InternetConnectionCapability myInternetConnection = InternetConnectionCapability.Unknown;

    /// <summary>
    /// UnityWebRequest Version of getting HTTP header
    /// </summary>
    /// <param name="_url"></param>
    /// <param name="_resultAct"></param>
    /// <returns></returns>
    public IEnumerator CheckFileSize(string _url, Action<int, bool> _resultAct)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Head(_url))
        {
            uwr.timeout = 30;
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

    public IEnumerator CheckServerSupportPartialContent(string _url, Action<bool> _resultAct, Action<string> _errorAct)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Head(_url))
        {
            uwr.timeout = 30;
            yield return uwr.SendWebRequest();

            while (!uwr.isDone)
                yield return null;

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                _errorAct(uwr.error);
                _resultAct(false);
            }
            else
            {
                _resultAct(uwr.GetResponseHeaders().ContainsKey("Accept-Ranges"));
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

    /// <summary>
    /// Download Whole thing from start to end
    /// </summary>
    /// <param name="_url"></param>
    /// <param name="_successAct"></param>
    /// <param name="_progressAct"></param>
    /// <param name="_errorAct"></param>
    /// <returns></returns>
    public IEnumerator DownloadWholeFile(string _url, Action<byte[]> _successAct, Action<float> _progressAct, Action<string> _errorAct)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(_url))
        {
            request.SendWebRequest();

            float lastProgress = 0f;
            float lastMarkedTime = Time.time;

            while (!request.isDone)
            {
                _progressAct(request.downloadProgress*0.9f);
                yield return null;

                if ((request.downloadProgress==lastProgress) && (Time.time-lastMarkedTime>5))
                {
                    _errorAct(request.error);
                    yield break;
                }

                lastProgress = request.downloadProgress;
                lastMarkedTime = Time.time;
            }
            _progressAct(1);

            if (request.isNetworkError || request.isHttpError) // Error
            {
                _errorAct(request.error);
                yield break;
            }

            _successAct(request.downloadHandler.data);
        }
    }

    #region Utility
    public IEnumerator CheckInternetConnection(List<string> _urls, Action<bool> _successAct = null)
    {
        if (myInternetConnection == InternetConnectionCapability.Testing)
            yield break;

        myInternetConnection = InternetConnectionCapability.Testing;

        foreach (string ur in _urls)
        {
            if (myInternetConnection == InternetConnectionCapability.Okay)
                yield break;

            yield return StartCoroutine(CheckInternetConnection(ur, _successAct));
        }
    }

    public IEnumerator CheckInternetConnection(string _url, Action<bool> _successAct)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(_url))
        {
            uwr.timeout = 30; // InternetConnection check MUST be done in 30 seconds
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                myInternetConnection = InternetConnectionCapability.ConnectionBlocked;
                _successAct(false);
                yield break;
            }

            myInternetConnection = InternetConnectionCapability.Okay;
            _successAct(true);           
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
