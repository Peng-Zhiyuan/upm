using System;
using System.Collections.Generic;
using UnityEngine;

namespace Table
{
    public class SceneInfo : ITableItem<string>
    {
        public int id;
        public string scene;
        public string panel;
        public int loading;
        public string resources;

        public string GetKey()
        {
            return scene;
        }
        public bool initialize()
        {
            //Debug.Log(stream);
            return true;
        }
    }
}

public class SceneTable2 : Table.TBaseTable<string, Table.SceneInfo, SceneTable2>
{
    protected override void initialize()
    {
        load("SceneInfo");
    }
}

public partial class ConfigLoader
{
    public object m_SceneTable = AddTable(SceneTable2.Instance);
}

