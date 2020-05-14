using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CaptureScreenshot : MonoBehaviour
{
    private int resWidth;
    private int resHeight;
    private Camera cameraCupture;
    public GameObject imageShare;
#pragma warning disable CS0649
    [SerializeField] 
    private string imageName;
#pragma warning restore CS0649
    private int cameraLayer = 28;

    // Use this for initialization
    public void Capture() {
        Debug.Log("CaptureScreenshot");
        cameraCupture = this.GetComponent<Camera>();
        // imageShare.layer = cameraLayer;
        RectTransform obj = (RectTransform)imageShare.transform;
        var rect = obj.rect;
        resWidth = (int)rect.width;
        resHeight = (int)rect.height;
        StartCoroutine(TakeScreenShot());
    }

    private IEnumerator TakeScreenShot()
    {
        yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        cameraCupture.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGBA32, false);
        cameraCupture.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        cameraCupture.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, imageName + ".png") ;
        System.IO.File.WriteAllBytes(path, bytes);
        Uploader.UploadFile(imageName+ ".png");
    }

}
