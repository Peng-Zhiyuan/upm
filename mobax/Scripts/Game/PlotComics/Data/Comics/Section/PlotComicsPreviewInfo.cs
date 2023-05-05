using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [Serializable]
    public class PlotComicsPreviewInfo
    {
        [FilePath(ParentFolder = "Assets/res/$Data/PlotConfigData")] [LabelWidth(100)] [LabelText("分镜配置:")]
        public string comicsRes;

        [LabelWidth(100)] [LabelText("互动界面:")] [OnValueChanged("OnPageNameTypeChanged")]
        public EPlotComicsInteractivePageName pageNameType;

        [LabelWidth(100)] [LabelText("暂停播放:")] [Tooltip("多用于打开界面类互动操作")] [ShowIf("ShowPause")]
        public bool needPause = false;

        private void OnPageNameTypeChanged()
        {
            this.pageName = DictionaryUtil.TryGet(this._pageNameDic, this.pageNameType, default);
            this.needPause = this.pageNameType != EPlotComicsInteractivePageName.None;
        }

        private bool ShowPause()
        {
            return this.pageNameType != EPlotComicsInteractivePageName.None;
        }

        [HideInInspector] public string pageName;

        private Dictionary<EPlotComicsInteractivePageName, string> _pageNameDic =
            new Dictionary<EPlotComicsInteractivePageName, string>()
            {
                {EPlotComicsInteractivePageName.UserRename, "UserRenamePage"}
            };
    }
}