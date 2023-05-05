namespace Plot.Runtime
{
    public class PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType;
        public int StartFrame;
        public int EndFrame;

        public virtual void Init(PlotComicsActionElementItem element)
        {
            this.ActionType = element.type;
            this.StartFrame = element.startFrame;
            this.EndFrame = element.endFrame;
        }
    }
}