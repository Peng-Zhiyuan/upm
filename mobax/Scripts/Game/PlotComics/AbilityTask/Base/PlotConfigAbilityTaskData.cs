namespace Plot.Runtime
{
    public class PlotConfigAbilityTaskData
    {
        public EPlotComicsElementType ElementType;
        public EConfigPriority Priority;

        public virtual void Init(PlotComicsConfigElementItem element)
        {
            this.ElementType = element.type;
            this.Priority = element.priority;
        }
    }
}