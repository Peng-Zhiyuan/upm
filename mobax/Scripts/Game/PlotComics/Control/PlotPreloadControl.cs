using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plot.Runtime
{
    /// <summary>
    /// 预加载策略
    /// <para>1.如果数组长度 超过2 则先加载2屏资源 播放第1屏的时候加载第3屏的资源</para>
    /// <para>2.如果数组长度 不到2 则先加载1屏资源 播放第1屏的时候加载第2屏的资源</para>
    /// </summary>
    public class PlotPreloadControl
    {
        private Bucket Bucket => BucketManager.Stuff.Plot;

        private PlotActionAbilityExecution _actionExecution;
        private PlotConfigAbilityExecution _configExecution;

        private List<PlotComicsPreviewInfo> _previewList;


        public PlotPreloadControl()
        {
            this._actionExecution = new PlotActionAbilityExecution(null);
            this._configExecution = new PlotConfigAbilityExecution(null);
        }

        public async Task Preload(List<string> comicsAddress)
        {
            // await PlotComicsManager.Stuff.LoadComicsPage();
            for (int i = 0; i < comicsAddress.Count; i++)
            {
                await this.Preload(comicsAddress[i]);
            }
        }

        public async Task Preload(string comicsAddress)
        {
            var comicsData = await Bucket.GetOrAquireAsync<PlotComicsPreviewConfigObject>($"{comicsAddress}.asset");
            await this.Preload(comicsData.previewList);
        }

        public async Task Preload(List<PlotComicsPreviewInfo> previewList)
        {
            this._previewList = previewList;
            await this.Preload();
        }

        // 预加载所有的资源 
        private async Task Preload()
        {
            for (int i = 0; i < this._previewList.Count; i++)
            {
                var preview = this._previewList[i];

                var comicsData = await this.Bucket.GetOrAquireAsync<PlotComicsConfigObject>(preview.comicsRes);
                await this._configExecution.BeginExecute(comicsData);
                await this._configExecution.Preload();

                await this._actionExecution.BeginExecute(comicsData);
                await this._actionExecution.Preload();
            }
        }
    }
}