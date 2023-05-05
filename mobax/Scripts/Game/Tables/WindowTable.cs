using System;
using System.Collections.Generic;
using UnityEngine;

namespace Table
{
    public class WindowInfo : ITableItem<string>
    {
        public int id;
        public string panel;                //面板key
        public string name;         
        public string resourcePath;         //资源路径(文件夹名)
        public string resource;             //资源名（名字去除Panel）
        public string layer;
        public int type;                    //类型 0普通 1弹出
        public int showMode;                //类型 0普通 1回退 2隐藏其他
        public string ignore;               //不隐藏的面板key
        public int flash;                   //出现动画类型

        public string GetKey()
        {
            return panel;
        }
        public bool initialize()
        {
            //Debug.Log(stream);
            return true;
        }
    }
}

public class WindowTable : Table.TBaseTable<string, Table.WindowInfo, WindowTable>
{
    protected override void initialize()
    {
        load("WindowInfo");
    }
}

public partial class ConfigLoader
{
    public object m_WindowTable = AddTable(WindowTable.Instance);
}

