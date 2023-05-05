using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotConfigAbilityTask
    {
        public object TaskInitData { get; set; }
        public Transform ParentRoot { get; set; }

        public void SetInitData(object initData, Transform parentRoot)
        {
            this.TaskInitData = initData;
            this.ParentRoot = parentRoot;
        }
        
        // 开始执行
        public virtual async Task BeginExecute()
        {
        }

        // 预加载
        public virtual async Task Preload()
        {
        }

        public virtual async Task EndExecute()
        {
            // this.TaskInitData = null;
        }
    }
}