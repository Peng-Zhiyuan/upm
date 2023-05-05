using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotActionAbilityTask
    {
        public object TaskInitData { get; set; }
        public Transform ParentRoot { get; set; }

        public void SetInitData(object initData, Transform parentRoot)
        {
            this.TaskInitData = initData;
            this.ParentRoot = parentRoot;
        }

        // 开始执行
        public virtual async void BeginExecute(int frameIdx)
        {
        }

        // 每帧执行
        public virtual void DoExecute(int frameIdx)
        {
        }

        // 重新执行某些逻辑
        public virtual void ReExecute()
        {
            
        }

        // 预加载
        public virtual async Task Preload()
        {
        }

        public virtual void PauseExecute(int frameIdx)
        {
        }

        public virtual async void BreakExecute(int frameIdx)
        {
           await EndExecute();
        }

        public virtual  async Task EndExecute()
        {
            // this.TaskInitData = null;
        }
    }
}