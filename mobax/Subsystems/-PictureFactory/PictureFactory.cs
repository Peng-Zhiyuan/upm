using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;
public class PictureFactory : StuffObject<PictureFactory>
{
    private Texture2D screen_texture = null;

    private IEnumerator CaptureScreen(bool autoSave)
    {
        yield return new WaitForEndOfFrame();
        if (screen_texture == null)
        {
            screen_texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        }
        screen_texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        Debug.LogError("CaptureScreen Success!!");
        if (autoSave)
        {
            this.SaveTexture(screen_texture);
        }
    }

    public async Task WaitCaptureScreen(bool autoSave = false)
    {
        this.StartCoroutine(CaptureScreen(autoSave));  
    }

    public Texture2D CatchTexture
    {
        get
        {
            return screen_texture;
        }
    }
    // Coroutine CaptureScreen()
    // {
    //      yield return new WaitForEndOfFrame();
    //     //await TimerManager.Stuff.DelayAsync(0);
    //     await  new WaitForEndOfFrame();
    //     if(screen_texture == null)
    //     { 
    //         screen_texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
    //     }
    //     screen_texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
    //     return screen_texture;
    // }

    public Texture2D BlendQRTexture(Texture2D orignTexture, Texture2D QRTexture, Vector2Int offset)
    {
        float ui_fit_scale = 1;//ScreenSizeHelper.Instance.FitInScale;

        int width = QRTexture.width;
        int height = QRTexture.height;
        int qrWidth = (int)(QRTexture.width * ui_fit_scale);
        int qrHeight = (int)(QRTexture.height * ui_fit_scale);

        offset = new Vector2Int((int)(offset.x * ui_fit_scale), (int)(offset.y * ui_fit_scale));
        var newTexture = orignTexture;

        int center_x = orignTexture.width / 2;
        int center_y = orignTexture.height / 2;
        int start_x = center_x + offset.x - qrWidth / 2;
        int start_y = center_y + offset.y - qrHeight / 2;
        for (int i = 0; i < qrWidth; i++)
        {
            for (int j = 0; j < qrHeight; j++)
            {
                int si = start_x + i;
                int sj = start_y + j;

                //newTexture.SetPixel(si, sj, QRTexture.GetPixel(Mathf.RoundToInt(i/ui_fit_scale), Mathf.RoundToInt(j/ui_fit_scale)));
                newTexture.SetPixel(si, sj, QRTexture.GetPixelBilinear((float)i / qrWidth, (float)j / qrHeight));

            }
        }
        return newTexture;
    }


    public string SaveTexture(Texture2D texture)
    {
        byte[] imagebytes = texture.EncodeToJPG();//转化为png图
                                                  //texture.Compress(false);//对屏幕缓存进行压缩

        string filename = "save_photo.jpg";

        string filePath = Application.persistentDataPath + "/screenShot/" + filename;
        if (Application.platform == RuntimePlatform.Android)
        {
            string destination = "/mnt/sdcard/DCIM/Camera";
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            filePath = destination + "/" + filename;

        }

        #if UNITY_EDITOR
                filePath = Application.dataPath + "/../screenShot/" + filename;
        #endif
        Debug.Log("filePath:" + filePath);
        this.WriteTexture(imagebytes, filePath);
        return filePath;
    }

    public void WriteTexture(byte[] bytes, string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        string directoryName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        string extension = Path.GetExtension(filePath);
        using (FileStream fs = File.OpenWrite(filePath))
        {
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
            fs.Dispose();
        }
    }
    
}
