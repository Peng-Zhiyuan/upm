package bridgeClass;

import org.json.JSONObject;

/**
 * Created by zhiyuan.peng on 2017/5/6.
 */

public class JSONObjectUtil
{

    public static void put(JSONObject jo, String key, Object obj)
    {
        try
        {
            jo.put(key, obj);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            throw new RuntimeException(e.getMessage());
        }
    }

    public static JSONObject fromString(String str)
    {
        try
        {
            JSONObject jo = new JSONObject(str);
            return jo;
        }
        catch (Exception e)
        {
            e.printStackTrace();
            throw new RuntimeException(e);
        }
    }

    public static String getString(JSONObject jo, String str)
    {
        try
        {
            return jo.getString(str);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            throw new RuntimeException(e);
        }
    }

    public static Double getDouble(JSONObject jo, String str)
    {
        try
        {
            return jo.getDouble(str);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            throw new RuntimeException(e);
        }
    }

    public static int getInt(JSONObject jo, String str)
    {
        try
        {
            return jo.getInt(str);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            throw new RuntimeException(e);
        }
    }

    public static JSONObject getJSONObject(JSONObject jo, String str)
    {
        try
        {
            return jo.getJSONObject(str);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            throw new RuntimeException(e);
        }
    }

    public static Object get(JSONObject jo, String str)
    {
        try
        {
            return jo.get(str);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            throw new RuntimeException(e);
        }
    }
}
