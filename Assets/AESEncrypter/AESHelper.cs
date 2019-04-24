
/// <summary>
/// https://qiita.com/OnederAppli/items/06795254652db0492d32
/// </summary>
public class AESHelper
{
    const int aesKeySize = 128;
    const int aesBlockSize = 128;
    const string aesIv = "1234123456789056";
    const string aesKey = "1234123654567890";

    ///// <summary>
    ///// AES暗号化サンプル
    ///// </summary>
    //private string AesEncryptSample(byte[] arr)
    //{
    //    // AES設定値
    //    //===================================
    //    int aesKeySize = 128;
    //    int aesBlockSize = 128;
    //    string aesIv = "1234567890123456";
    //    string aesKey = "1234567890123456";
    //    //===================================

    //    // AES暗号化
    //    byte[] arrEncrypted = AesEncrypt(arr, aesKeySize, aesBlockSize, aesIv, aesKey);

    //    // ファイル書き込み
    //    string path = System.IO.Path.Combine(Application.temporaryCachePath, "UserDataAES");
    //    System.IO.File.WriteAllBytes(path, arrEncrypted);

    //    // ファイル読み込み
    //    byte[] arrRead = System.IO.File.ReadAllBytes(path);

    //    // 復号化
    //    byte[] arrDecrypt = AesDecrypt(arrRead, aesKeySize, aesBlockSize, aesIv, aesKey);

    //    // byte配列を文字列に変換
    //    string decryptStr = System.Text.Encoding.UTF8.GetString(arrDecrypt);

    //    return decryptStr;
    //}

    ///// <summary>
    ///// XORサンプル
    ///// </summary>
    //private string XorEncrypt(byte[] arr)
    //{
    //    // 暗号化文字列
    //    string keyString = "123456789";

    //    // XOR
    //    byte[] keyArr = System.Text.Encoding.UTF8.GetBytes(keyString);
    //    byte[] arrEncrypted = Xor(arr, keyArr);

    //    // ファイル書き込み
    //    string path = System.IO.Path.Combine(Application.temporaryCachePath, "UserDataXOR");
    //    System.IO.File.WriteAllBytes(path, arrEncrypted);

    //    // ファイル読み込み
    //    byte[] arrRead = System.IO.File.ReadAllBytes(path);

    //    // XOR
    //    byte[] arrDecrypt = Xor(arrRead, keyArr);

    //    // byte配列を文字列に変換
    //    return System.Text.Encoding.UTF8.GetString(arrDecrypt);
    //}

    /// <summary>
    /// AES暗号化
    /// </summary>
    public string AesEncrypt(string byteTexty)
    {
        var encBytes = AesEncrypt(System.Text.Encoding.UTF8.GetBytes(byteTexty), aesKeySize, aesBlockSize, aesIv, aesKey);
        return System.Convert.ToBase64String(encBytes);
    }

    /// <summary>
    /// AES復号化
    /// </summary>
    public string AesDecrypt(string byteText)
    {
        var decBytes = AesDecrypt(System.Convert.FromBase64String(byteText), aesKeySize, aesBlockSize, aesIv, aesKey);
        return System.Text.Encoding.UTF8.GetString(decBytes);
    }

    /// <summary>
    /// AES暗号化
    /// </summary>
    public byte[] AesEncrypt(byte[] byteText, int aesKeySize, int aesBlockSize, string aesIv, string aesKey)
    {
        // AESマネージャー取得
        var aes = GetAesManager(aesKeySize, aesBlockSize, aesIv, aesKey);
        // 暗号化
        byte[] encryptText = aes.CreateEncryptor().TransformFinalBlock(byteText, 0, byteText.Length);

        return encryptText;
    }

    /// <summary>
    /// AES復号化
    /// </summary>
    public byte[] AesDecrypt(byte[] byteText, int aesKeySize, int aesBlockSize, string aesIv, string aesKey)
    {
        // AESマネージャー取得
        var aes = GetAesManager(aesKeySize, aesBlockSize, aesIv, aesKey);
        // 復号化
        byte[] decryptText = aes.CreateDecryptor().TransformFinalBlock(byteText, 0, byteText.Length);

        return decryptText;
    }

    /// <summary>
    /// AesManagedを取得
    /// </summary>
    /// <param name="keySize">暗号化鍵の長さ</param>
    /// <param name="blockSize">ブロックサイズ</param>
    /// <param name="iv">初期化ベクトル(半角X文字（8bit * X = [keySize]bit))</param>
    /// <param name="key">暗号化鍵 (半X文字（8bit * X文字 = [keySize]bit）)</param>
    private System.Security.Cryptography.AesManaged GetAesManager(int keySize, int blockSize, string iv, string key)
    {
        var aes = new System.Security.Cryptography.AesManaged();
        aes.KeySize = keySize;
        aes.BlockSize = blockSize;
        aes.Mode = System.Security.Cryptography.CipherMode.CBC;
        aes.IV = System.Text.Encoding.UTF8.GetBytes(iv);
        aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
        aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
        return aes;
    }

    /// <summary>
    /// XOR
    /// </summary>
    public byte[] Xor(byte[] a, byte[] b)
    {
        int j = 0;
        for (int i = 0; i < a.Length; i++)
        {
            if (j < b.Length)
            {
                j++;
            }
            else
            {
                j = 1;
            }
            a[i] = (byte)(a[i] ^ b[j - 1]);
        }
        return a;
    }
}
