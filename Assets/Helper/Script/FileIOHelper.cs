using System;
using System.Collections.Generic;
using System.IO;

public class FileIOHelper
{
    #region Check
    public bool CheckDirectoryExist(string _localPath)
    {
        return Directory.Exists(_localPath);
    }

    public bool CheckFileExist(string _localPath)
    {
        return File.Exists(_localPath);
    }

    public int CheckFileSize(string _localPath)
    {
        FileInfo finfo = new FileInfo(_localPath);

        if (finfo.Exists)
            return (int)finfo.Length;
        else
            return -1;
    }
    #endregion

    #region Save and Append
    public void AppendTo(string _localPath, Stream _sdata, int _windowsSize = 1048576)
    {
        byte[] buffer = new byte[_windowsSize];

        using (MemoryStream ms = new MemoryStream())
        {
            int bytesRead;
            while ((bytesRead = _sdata.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, bytesRead);
            }

            byte[] result = ms.ToArray();
            AppendTo(_localPath, result);
        }
    }

    public void AppendTo(string _localPath, byte[] _data)
    {
        using (var fstream = new FileStream(_localPath, FileMode.Append))
        {
            fstream.Write(_data, 0, _data.Length);
        }
    }

    public void SaveTo(string _localPath, string _content)
    {
        File.WriteAllText(_localPath, _content);
    }

    public void SaveTo(string _localPath, byte[] _content)
    {
        File.WriteAllBytes(_localPath, _content);
    }
    #endregion

    #region Load
    public string LoadStringFromFile(string _localPath)
    {
        return File.ReadAllText(_localPath);
    }

    public byte[] LoadbyteFromFile(string _localPath)
    {
        return File.ReadAllBytes(_localPath);
    }
    #endregion

    #region Directory
    public void MakeDirectory(List<string> _dirPaths)
    {
        for (int i = 0; i < _dirPaths.Count; i++)
            MakeDirectory(_dirPaths[i]);
    }

    public void MakeDirectory(string _dirPAth)
    {
        if (!Directory.Exists(_dirPAth))
        {
            Directory.CreateDirectory(_dirPAth);
        }
    }
    #endregion

    public void Remove(string _localPath)
    {
        if (!CheckFileExist(_localPath))
            return;

        File.Delete(_localPath);
    }

    public void Touch(string _localPath)
    {
        if (!CheckFileExist(_localPath))
        {
            using (File.Create(_localPath))
            {
            }
            return;
        }

        File.SetLastWriteTimeUtc(_localPath, DateTime.UtcNow);
    }
}