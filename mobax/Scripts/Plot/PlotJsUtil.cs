using System;

public struct PlotInfo
{
    public int StageId;
    public EPlotEventType EventType;
    public Action OnComp;
}



public static class PlotJsUtil
{
    public static void Trigger(PlotInfo plotInfo)
    {
      //  var plotManager = JsEngine.Stuff.GetModulePathByTsClassName("PlotManager");
       // JsUtil.CallStaticMethod(plotManager, "PlotManager", "Trigger", plotInfo);
    }
}