using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotComicsFragCtrl
    {
        private PlotActionAbilityExecution _actionExecution;
        private PlotConfigAbilityExecution _configExecution;
        private PlotComicsConfigObject _comicsData;

        private bool _isPlay = false;
        private Action _onPlayCompleted;
        private int _totalEndFrame = 0;
        private bool _isLast;

        public PlotComicsFragCtrl(PlotComicsConfigObject comicsData, PlotActionAbilityExecution actionExecution,
            PlotConfigAbilityExecution configExecution, bool isLast)
        {
            _isLast = isLast;
            this._comicsData = comicsData;
            this._actionExecution = actionExecution;
            this._configExecution = configExecution;
            this.RefreshTotalPlayEndFrame();
        }

        public void AddOnCompletedAction(Action onComp)
        {
            this._onPlayCompleted = onComp;
        }

        public async Task BeginExecute()
        {
            await this._configExecution.BeginExecute(this._comicsData);
            await this._actionExecution.BeginExecute(this._comicsData);
        }

        public async void DoExecute()
        {
            this._isPlay = true;
            await this._configExecution.DoExecute();
            this._actionExecution.DoExecute(0);
            this._actionExecution.Start2Play(this._totalEndFrame, _isLast, this.OnPlayCompleted);
        }

        public async Task OnEnd()
        {
            // await this.BeginExecute();
            this._isPlay = false;
            await this._configExecution.OnEnd();
            await this._actionExecution.OnEnd();
        }

        public void SetSpeedUp(float speed)
        {
            this._actionExecution.Speed = speed;
        }

        public void SetSpeedNormal()
        {
            this._actionExecution.Speed = 1;
        }
        public void SetPause()
        {
            this._actionExecution.Pause = true;
        }

        public void SetReplay()
        {
            this._actionExecution.Pause = false;
        }
        private void RefreshTotalPlayEndFrame()
        {
            for (int i = 0; i < this._comicsData.actionElements.Count; i++)
            {
                if (this._totalEndFrame < this._comicsData.actionElements[i].endFrame)
                {
                    this._totalEndFrame = this._comicsData.actionElements[i].endFrame;
                }
            }

            // this._totalEndFrame += 20;
        }

        private void OnPlayCompleted()
        {
            this.Stop();
            this._onPlayCompleted?.Invoke();
        }

        public void Stop()
        {
            this._configExecution?.EndExecute();
            this._actionExecution?.EndExecute();
            this._isPlay = false;
        }

        public void OnUpdate()
        {
            if (this._isPlay)
            {
                this._actionExecution.OnUpdate();
            }
        }
    }
}