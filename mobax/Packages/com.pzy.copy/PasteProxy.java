package bridgeClass;

import android.app.Activity;
import android.content.ClipData;
import android.content.ClipDescription;
import android.content.ClipboardManager;
import android.os.Looper;
import com.unity3d.player.UnityPlayer;

public class PasteProxy
{
    public static String Paste(String arg)
    {
        Activity activity = UnityPlayer.currentActivity;
        if (Looper.myLooper() == null)
        {
            Looper.prepare();
        }
        ClipboardManager clipboard = (ClipboardManager)activity.getSystemService(Activity.CLIPBOARD_SERVICE);
        if  (clipboard.hasPrimaryClip()
                && clipboard.getPrimaryClipDescription().hasMimeType(ClipDescription.MIMETYPE_TEXT_PLAIN)) {
            ClipData textCd = clipboard.getPrimaryClip();
            ClipData.Item item = textCd.getItemAt(0);
            return  item.getText().toString();
        }
        return   "" ;
    }    
}