package bridgeClass;

import android.app.Activity;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.InputStream;
import java.io.InputStreamReader;

/**
 * Created by zhiyuan.peng on 2017/5/24.
 */

public class NativeLuncherManifestManager
{
    // c# read this
    static String manifestString = null;

    static JSONObject manifest = null;

    // call
    public static String GetRawString(String arg)
    {
        if(manifestString == null)
        {
            load();
        }
        return manifestString;
    }

    public static String Get(String key, String _default)
    {
        if(manifestString == null)
        {
            load();
        }
        try
        {
            if(!manifest.has(key))
            {
                return _default;
            }
            String value = manifest.getString(key);
            return value;
        }
        catch (Exception e)
        {
            throw new RuntimeException(e);
        }
    }

    public static void load()
    {
        try
        {
            InputStream stream = UnityPlayer.currentActivity.getAssets().open("luncher-manifest.json");
            String jsonString = readToEnd(stream);
            manifestString = jsonString;
            manifest = JSONObjectUtil.fromString(manifestString);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            manifestString = "{}";
        }
    }

    /**
     * 按行读取txt
     *
     * @param is
     * @return
     * @throws Exception
     */
    private static String readToEnd(InputStream is) throws Exception {
        InputStreamReader reader = new InputStreamReader(is);
        BufferedReader bufferedReader = new BufferedReader(reader);
        StringBuffer buffer = new StringBuffer("");
        String str;
        while ((str = bufferedReader.readLine()) != null) {
            buffer.append(str);
            buffer.append("\n");
        }
        return buffer.toString();
    }

}
