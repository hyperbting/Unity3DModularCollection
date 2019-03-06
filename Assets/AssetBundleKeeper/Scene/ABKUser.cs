using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (AssetBundleKeeper))]
public class ABKUser : MonoBehaviour {

    public AssetBundleKeeper myABK;

    // Use this for initialization
    IEnumerator Start ()
    {
        //Setup
        myABK.ABM_PATH = new FileURL()
        {
            fullURL = "https://s3-ap-northeast-1.amazonaws.com/hooloopplayground/" + SystemInfoChecker.GetPlatformName() + "/"  + SystemInfoChecker.GetPlatformName(),
            localPath = Application.persistentDataPath+"/ABM"
        };

        yield return null;

        //download ABM
        myABK.DownloadABM();

        while (!myABK.IsManifestReady())
            yield return null;

        Debug.Log("ABM is ready");

        foreach (var name in myABK.GetABNames())
            Debug.Log(name);

        UnityEngine.U2D.SpriteAtlas tmpSASA = null;
        //LoadSpriteAtlasFromAB
        yield return StartCoroutine(myABK.LoadSpriteAtlasFromAB(new ABFileURL()
        {
            fullURL = "https://s3-ap-northeast-1.amazonaws.com/hooloopplayground/" + SystemInfoChecker.GetPlatformName() + "/icons",
            fileName = "icons"
        },
        (UnityEngine.U2D.SpriteAtlas _sa) =>
        {
            tmpSASA = _sa;
        },
        "IconSA"// SA Name
        ));

        Sprite tmptmp = null;
        for (int i = 0; i < 500; i++)
        {
            tmptmp = tmpSASA.GetSprite("baseline_backup_black_18dp");
            yield return null;
        }
        Resources.UnloadUnusedAssets();
        yield return null;
        //DestroyImmediate(tmptmp, true);
        tmptmp = null;

        Debug.Log("IconSA Finished");

        //LoadTextureFromAB
        yield return StartCoroutine(myABK.LoadTextureFromAB(new ABFileURL()
        {
            fullURL = "https://s3-ap-northeast-1.amazonaws.com/hooloopplayground/" + SystemInfoChecker.GetPlatformName() + "/icons",
            fileName = "icons"
        },
        (List<Texture2D> _textures) =>
        {

        },
        "round_alternate_email_black_48dp" // Texture Names
        ));

        Debug.Log("round_alternate_email_black_48dp Finished");

        ////LoadGameObjectFromAB
        //yield return StartCoroutine(myABK.LoadGameObjetFromAB(new ABFileURL()
        //{
        //    fullURL = "https://s3-ap-northeast-1.amazonaws.com/hooloopplayground/" + SystemInfoChecker.GetPlatformName() + "/tester001",
        //    fileName = "tester001"
        //},
        //new List<ObjectNamePosition>()
        //{
        //    new ObjectNamePosition(){ name="Cube", position=Vector3.down}
        //} // List of ObjectNamePosition
        //));

        //Debug.Log("Cube Finished");

        ////Load Textures FromAB
        //yield return StartCoroutine(myABK.LoadTextureFromAB(new ABFileURL()
        //{
        //    fullURL = "https://s3-ap-northeast-1.amazonaws.com/hooloopplayground/" + SystemInfoChecker.GetPlatformName() + "/icons",
        //    fileName = "icons"
        //},
        //(List<Texture2D> _textures) =>
        //{

        //},
        //new string[]{ "baseline_3d_rotation_black_18dp", "round_alternate_email_black_48dp" } // ObjectNamePositions
        //));

        //Debug.Log("baseline_3d_rotation_black_18dp  round_alternate_email_black_48dp Finished");
    }
}
