using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Plot.Runtime
{
    [CreateAssetMenu(fileName = "漫画配置", menuName = "剧情配置")]
    public class PlotComicsConfigObject : SerializedScriptableObject
    {
        #region ---Left 配置设置等---

        //TODO:这里最好不要用0.000多位小数点
        [HorizontalGroup("Split", 0.569f, LabelWidth = 80)]
        [BoxGroup("Split/Left")]
        [LabelText("分镜漫画内容列表-初始化")]
        [ListDrawerSettings(Expanded = true, DraggableItems = false, ShowItemCount = true, HideAddButton = true,
            NumberOfItemsPerPage = 10)]
        [HideReferenceObjectPicker]
        public List<PlotComicsConfigElementItem> comicsElements = new List<PlotComicsConfigElementItem>();

        [BoxGroup("Split/Left")]
        [HideLabel]
        [OnValueChanged("AddComicsElement")]
        [ValueDropdown("ComicsElementTypeSelect", NumberOfItemsBeforeEnablingSearch = 10)]
        public string comicsElementTypeName = "(添加漫画内容)";

        private bool _isComicsDelete = false;

        [UsedImplicitly]
        public IEnumerable<string> ComicsElementTypeSelect()
        {
            var types = typeof(PlotComicsConfigElementItem).Assembly.GetTypes().Where(x => !x.IsAbstract)
                .Where(x => typeof(PlotComicsConfigElementItem).IsAssignableFrom(x))
                .Where(x => x.GetCustomAttribute<PlotComicsConfigElementItemAttribute>() != null)
                .OrderBy(x => x.GetCustomAttribute<PlotComicsConfigElementItemAttribute>().Order).Select(x =>
                    x.GetCustomAttribute<PlotComicsConfigElementItemAttribute>().ElementType);
            var results = types.ToList();
            results.Insert(0, "(添加漫画内容)");
            return results;
        }

        [UsedImplicitly]
        private void AddComicsElement()
        {
            //TODO:这里理论上不应该存在重新赋值的现象 但是列表删除的时候还会走到这里  先临时添加的容错机制
            if (this.comicsElementTypeName == "(添加漫画内容)" || this._isComicsDelete)
            {
                this.comicsElementTypeName = "(添加漫画内容)";
                this._isComicsDelete = false;
                return;
            }

            var elementType = typeof(PlotComicsConfigElementItem).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(PlotComicsConfigElementItem).IsAssignableFrom(x)).Where(x =>
                    x.GetCustomAttribute<PlotComicsConfigElementItemAttribute>() != null)
                .First(x => x.GetCustomAttribute<PlotComicsConfigElementItemAttribute>().ElementType ==
                            comicsElementTypeName);

            var element = Activator.CreateInstance(elementType) as PlotComicsConfigElementItem;
            element.type =
                (EPlotComicsElementType) elementType.GetCustomAttribute<PlotComicsConfigElementItemAttribute>().Order;
            element.priority =
                (EConfigPriority) elementType.GetCustomAttribute<PlotComicsConfigElementItemAttribute>().Priority;
            this.comicsElements.Add(element);
            this.comicsElementTypeName = "(添加漫画内容)";
            this._isComicsDelete = true;
        }

        #endregion

        #region ---Right 帧操作等设置---

        [HorizontalGroup("Split", 0.368f, LabelWidth = 80)]
        [BoxGroup("Split/Right")]
        [LabelText("帧行为列表")]
        [ListDrawerSettings(Expanded = true, DraggableItems = false, ShowItemCount = true, HideAddButton = true,
            NumberOfItemsPerPage = 10)]
        [HideReferenceObjectPicker]
        // [OnValueChanged("OnComicsElementsChanged")]
        public List<PlotComicsActionElementItem> actionElements = new List<PlotComicsActionElementItem>();

        [BoxGroup("Split/Right")]
        [HideLabel]
        [OnValueChanged("AddActionElement")]
        [ValueDropdown("ActionElementTypeSelect", NumberOfItemsBeforeEnablingSearch = 10)]
        public string actionElementTypeName = "(添加帧行为)";

        private bool _isActionDelete = false;

        [UsedImplicitly]
        public IEnumerable<string> ActionElementTypeSelect()
        {
            var types = typeof(PlotComicsActionElementItem).Assembly.GetTypes().Where(x => !x.IsAbstract)
                .Where(x => typeof(PlotComicsActionElementItem).IsAssignableFrom(x))
                .Where(x => x.GetCustomAttribute<PlotComicsActionElementItemAttribute>() != null)
                .OrderBy(x => x.GetCustomAttribute<PlotComicsActionElementItemAttribute>().Order).Select(x =>
                    x.GetCustomAttribute<PlotComicsActionElementItemAttribute>().ActionType);
            var results = types.ToList();
            results.Insert(0, "(添加帧行为)");
            return results;
        }

        [UsedImplicitly]
        private void AddActionElement()
        {
            //TODO:这里理论上不应该存在重新赋值的现象 但是列表删除的时候还会走到这里  先临时添加的容错机制
            if (this.actionElementTypeName == "(添加帧行为)" || this._isActionDelete)
            {
                this.actionElementTypeName = "(添加帧行为)";
                this._isActionDelete = false;
                return;
            }

            var elementType = typeof(PlotComicsActionElementItem).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(PlotComicsActionElementItem).IsAssignableFrom(x)).Where(x =>
                    x.GetCustomAttribute<PlotComicsActionElementItemAttribute>() != null)
                .First(x => x.GetCustomAttribute<PlotComicsActionElementItemAttribute>().ActionType ==
                            this.actionElementTypeName);

            var actionElement = Activator.CreateInstance(elementType) as PlotComicsActionElementItem;
            actionElement.type =
                (EPlotActionType) elementType.GetCustomAttribute<PlotComicsActionElementItemAttribute>().Order;
            this.actionElements.Add(actionElement);
            this.actionElementTypeName = "(添加帧行为)";
            this._isActionDelete = true;
        }

        #endregion
    }
}