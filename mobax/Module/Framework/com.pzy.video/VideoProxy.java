package bridgeClass;

import android.media.MediaCodecInfo;
import android.media.MediaCodecList;
import android.util.Log;

import org.json.JSONException;
import org.json.JSONObject;


public class VideoProxy
{
    public static String GetMax(String str) throws JSONException {
        MediaCodecList codecList = new MediaCodecList(MediaCodecList.ALL_CODECS);
        MediaCodecInfo[] codecInfos = codecList.getCodecInfos();
        for (MediaCodecInfo codecInfo : codecInfos) {
            if (codecInfo.isEncoder()) {
                continue;
            }
            String[] types = codecInfo.getSupportedTypes();
            for (String type : types) {
                if (!type.equals("video/mp4v-es")) {
                    continue;
                }
                MediaCodecInfo.CodecCapabilities capabilities = codecInfo.getCapabilitiesForType(type);
                MediaCodecInfo.VideoCapabilities videoCapabilities = capabilities.getVideoCapabilities();
                if (videoCapabilities != null) {
                    int maxWidth = videoCapabilities.getSupportedWidths().getUpper();
                    int maxHeight = videoCapabilities.getSupportedHeights().getUpper();
                    Log.d("MaxVideoResolution", "Width: " + maxWidth + ", Height: " + maxHeight);
                    try
                    {
                        JSONObject jo = new JSONObject("{}");
                        jo.put("width", maxWidth);
                        jo.put("height", maxHeight);
                        String json = jo.toString();
                        return json;
                    }
                    catch (Exception e)
                    {
                        e.printStackTrace();
                    }
                }
            }
        }
        throw new RuntimeException("[VideoProxy] Cannot found max size of mp4");
    }
}
