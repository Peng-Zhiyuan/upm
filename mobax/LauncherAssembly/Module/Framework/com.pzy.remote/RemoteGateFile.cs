using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteGateFile
{
    public List<string> lineList;

    public RemoteGateFile(string rawString)
    {
        var lines = rawString.Split('\n');
        lineList = new List<string>();
        lineList.AddRange(lines);
    }

    (string platform, string versionString, string channel) ParseLine(string line)
    {
        var parts = line.Split(':');
        var version = parts[0].Trim();
        var channel = parts[1].Trim();
        if(!version.Contains(" "))
        {
            return (platform: "", versionString: version, channel: channel);
        }
        else
        {
            var pp = version.Split(' ');
            var platform = pp[0].Trim();
            var finalVersion = pp[1].Trim();
            return (platform: platform, versionString: finalVersion, channel: channel);
        }
    }

    bool IsPlatformMatch(string targetPlatform)
    {
        if(string.IsNullOrEmpty(targetPlatform))
        {
            return true;
        }
        else if(targetPlatform == "android")
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if(targetPlatform == "ios")
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if(targetPlatform == "editor")
        {
            if(Application.isEditor)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public string GetCommand(string versionString)
    {
        foreach(var line in lineList)
        {
            var (platform, version, channel) = ParseLine(line);
            var isPlatformMatched = IsPlatformMatch(platform);
            if(!isPlatformMatched)
            {
                continue;
            }
            var isVersionMatched = IsVersionMatch(versionString, version);
            if(!isVersionMatched)
            {
                continue;
            }
            return channel;
        }
        return "";
    }

    bool IsVersionMatch(string versionString, string code)
    {
        var v = versionString.Split('.');
        var c = code.Split('.');
        for(int i = 0; i < 3; i++)
        {
            var vv = ArrayUtil.TryGet(v, i, "0");
            var cc = ArrayUtil.TryGet(c, i, "0");
            if (!IsSingleDigitalMatch(vv, cc))
            {
                return false;
            }
        }
        return true;
    }

    bool IsSingleDigitalMatch(string v, string code)
    {
        if(code == "*")
        {
            return true;
        }

        if(code.EndsWith("+"))
        {
            var intV = int.Parse(v);
            var trimedCode = code.Trim('+');
            var intCode = int.Parse(trimedCode);
            if(intV >= intCode)
            {
                return true;
            }
        }

        if (code.EndsWith("-"))
        {
            var intV = int.Parse(v);
            var trimedCode = code.Trim('-');
            var intCode = int.Parse(trimedCode);
            if (intV <= intCode)
            {
                return true;
            }
        }

        if(v == code)
        {
            return true;
        }
        return false;
    }


}
