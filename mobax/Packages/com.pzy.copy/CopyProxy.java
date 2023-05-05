package bridgeClass;

import android.app.Activity;
import android.content.ClipData;

import android.content.ClipboardManager;
import android.os.Looper;
import com.unity3d.player.UnityPlayer;


public class CopyProxy
{
    public static void Copy(String str)
    {
        Activity activity = UnityPlayer.currentActivity;
        if (Looper.myLooper() == null)
        {
            Looper.prepare();
        }
        ClipboardManager clipboard = (ClipboardManager)activity.getSystemService(Activity.CLIPBOARD_SERVICE);
        ClipData textCd = ClipData.newPlainText("data", str);
        clipboard.setPrimaryClip(textCd);
    }
}
