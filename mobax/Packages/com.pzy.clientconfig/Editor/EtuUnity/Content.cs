using System;
using System.Collections.Generic;
using System.IO;
using CustomLitJson;

namespace EtuUnity
{
    public static class Content
    {

        private static string _test;
        public static string Test
        {
            get
            {
                if (_test == null)
                {
                    var root = Util.FindScriptDir();
                    var path = $"{root}/Res_Test.txt";
                    _test = File.ReadAllText(path);
                }
                return _test;
            }
        }

        private static string _tablePropertyTemplate;
        public static string TablePropertyTemplate
        {
            get
            {
                if (_tablePropertyTemplate == null)
                {
                    var root = Util.FindScriptDir();
                    var path = $"{root}/Res_TableCode.txt";
                    _tablePropertyTemplate = File.ReadAllText(path);
                }
                return _tablePropertyTemplate;
            }
        }

        private static string _kvtablePropertyTemplate;
        public static string KvTablePropertyTemplate
        {
            get
            {
                if (_kvtablePropertyTemplate == null)
                {
                    var root = Util.FindScriptDir();
                    var path = $"{root}/Res_KeyValueTableCode.txt";
                    _kvtablePropertyTemplate = File.ReadAllText(path);
                }
                return _kvtablePropertyTemplate;
            }
        }

        private static string _staticDataTemplate;
        public static string StaticDataFrame
        {
            get
            {
                if (_staticDataTemplate == null)
                {
                    var root = Util.FindScriptDir();
                    var path = $"{root}/Res_ClientConfig+Frame.txt";
                    _staticDataTemplate = File.ReadAllText(path);
                }
                return _staticDataTemplate;
            }
        }


    }
}