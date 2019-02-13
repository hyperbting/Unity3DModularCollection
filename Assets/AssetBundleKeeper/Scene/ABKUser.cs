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
            fileName = SystemInfoChecker.GetPlatformName(),
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

        //LoadSpriteAtlasFromAB
        yield return StartCoroutine(myABK.LoadSpriteAtlasFromAB(new FileURL()
        {
            fullURL = "https://s3-ap-northeast-1.amazonaws.com/hooloopplayground/" + SystemInfoChecker.GetPlatformName() + "/icons",
            fileName = "icons"
        },
        "IconSA"// SA Name
        ));

        //LoadSpriteAtlasFromAB
        yield return StartCoroutine(myABK.LoadTextureFromAB(new FileURL()
        {
            fullURL = "https://s3-ap-northeast-1.amazonaws.com/hooloopplayground/" + SystemInfoChecker.GetPlatformName() + "/icons",
            fileName = "icons"
        },
        "round_alternate_email_black_48dp" // Texture Names
        ));

        //LoadGameObjectFromAB
        yield return StartCoroutine(myABK.LoadGameObjetFromAB(new FileURL()
        {
            fullURL = "https://s3-ap-northeast-1.amazonaws.com/hooloopplayground/" + SystemInfoChecker.GetPlatformName() + "/tester001",
            fileName = "tester001"
        },
        new List<ObjectNamePosition>()
        {
            new ObjectNamePosition(){ name="Cube", position=Vector3.down}
        } // List of ObjectNamePosition
        ));

        //Load Textures FromAB
        yield return StartCoroutine(myABK.LoadTextureFromAB(new FileURL()
        {
            fullURL = "https://s3-ap-northeast-1.amazonaws.com/hooloopplayground/" + SystemInfoChecker.GetPlatformName() + "/icons",
            fileName = "icons"
        },
        new string[]{ "baseline_3d_rotation_black_18dp", "round_alternate_email_black_48dp" } // ObjectNamePositions
        ));
    }
}
