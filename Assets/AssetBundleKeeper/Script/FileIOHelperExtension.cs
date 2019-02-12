using UnityEngine;

namespace ExtensionMethods
{
    public static class FileIOHelperExtension
    {
        public static Texture2D LoadTextureFromDisk(this FileIOHelper _fileIOHelper, string _path)
        {
            // Create a texture. Texture size does not matter, since LoadImage will replace with with incoming image size.
            Texture2D tex = new Texture2D(320, 320, TextureFormat.RGB24, false, true)
            {// DXT5
                name = _path
            };

            if (!_fileIOHelper.CheckFileExist(_path))
            {
                Debug.LogWarning("Texture not found " + _path);
                return tex;
            }

            Debug.Log("LoadTextureFromDisk : " + _path);

            tex.LoadImage(_fileIOHelper.LoadbyteFromFile(_path)); // LoadImage will always RGBA32 for PNG/ RGB24 for JPG
            tex.Compress(true); // this will lower half the size but require some CPU power

            Debug.Log(tex.dimension + ":x" + tex.width + ",y" + tex.height + "," + tex.format + "," + tex.filterMode);
            return tex;
        }
    }
}
