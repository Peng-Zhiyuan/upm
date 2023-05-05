using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public struct SocketMessage
{
    public SocketMessageHead head;
    public byte[] data;

    public int Code
    {
        get
        {
            return this.head.code;
        }
    }

    private string _fullPath;
    public string FullPath
    {
        get
        {
            if(this._fullPath == null)
            {
                if(data == null)
                {
                    throw new Exception("[NetMessage] data not set yet");
                }
                var length = this.head.path;
                this._fullPath = Encoding.UTF8.GetString(this.data, 0, length);
            }
            return this._fullPath;
        }
    }

    private string _nonArgPath;
    public string NonArgPath
    {
        get
        {
            if(_nonArgPath == null)
            {
                var fullPath = this.FullPath;
                var index = fullPath.IndexOf('?');
                if(index != -1)
                {
                    _nonArgPath = fullPath.Substring(0, index);
                }
                else
                {
                    _nonArgPath = fullPath;
                }
            }
            return _nonArgPath;
        }
    }

    static readonly Dictionary<string, string> EMPTY = new Dictionary<string, string>();

    private Dictionary<string, string> _arg;
    public Dictionary<string, string> Arg
    {
        get
        {
            if (_arg == null)
            {
                var fullPath = this.FullPath;
                var index = fullPath.IndexOf('?');
                if(index != -1)
                {
                    var argString = fullPath.Substring(index + 1);
                    _arg = ParseUrlArg(argString);
                }
                else
                {
                    _arg = EMPTY;
                }
            }
            return _arg;
        }
    }

    static Dictionary<string, string> ParseUrlArg(string argString)
    {
        var ret = new Dictionary<string, string>();
        var parts = argString.Split('&');
        foreach(var part in parts)
        {
            var pparts = part.Split('=');
            var key = pparts[0];
            var value = "";
            if(pparts.Length > 1)
            {
                value = pparts[1];
            }
            ret[key] = value;
        }
        return ret;
    }

    private byte[] _body;
    public byte[] Body
    {
        get
        {
            if(this._body == null)
            {
                if (data == null)
                {
                    throw new Exception("[NetMessage] data not set yet");
                }
                var length = this.head.body;
                var ret = new byte[length];
                var startIndex = this.head.path;
                for (int i = 0; i < length; i++)
                {
                    var index = startIndex + i;
                    ret[i] = this.data[index];
                }
                this._body = ret;

            }
            return _body;
        }
    }

    public override string ToString()
    {
        string errorMsg = null;
        try
        {
            errorMsg = Encoding.Default.GetString(this.Body);
        }
        catch
        {
           
        }

        if(!string.IsNullOrEmpty(errorMsg))
        {
            return $"[code: {head.code}, Path: {this.FullPath}, Body.Length: {this.Body.Length} ({errorMsg})]";
        }
        else
        {
            return $"[code: {head.code}, Path: {this.FullPath}, Body.Length: {this.Body.Length}]";
        }

    }

    public int BodyAsInt32
    {
        get
        {
            var data = this.Body;
            var ret = BitUtil.ToInt32(data, 0, true);
            return ret;
        }
    }

    public UInt32 BodyAsUInt32
    {
        get
        {
            var data = this.Body;
            var ret = BitUtil.ToUInt32(data, 0, true);
            return ret;
        }
    }

    public UInt16 BodyAsUInt16
    {
        get
        {
            var data = this.Body;
            var ret = BitUtil.ToUInt16(data, 0, true);
            return ret;
        }
    }

    public UInt64 BodyAsUInt64
    {
        get
        {
            var data = this.Body;
            var ret = BitUtil.ToUInt64(data, 0, true);
            return ret;
        }
    }

    public string BodyAsString
    {
        get
        {
            var data = this.Body;
            var ret = Encoding.Default.GetString(data);
            return ret;
        }
    }
}