using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using BattleSystem.ProjectCore;
using System.Net;
using BattleEngine.Logic;

public class TempMainPage : Page
{
    public void OnButton(string msg)
    {
        if (msg == "offlinePve")
        {
            BattleUtil.EnterPveBattle(103);
            /*
            if (true)
                return;

            var param = new BattleParam();
            param.coreParam.mode = BattleModeType.PveTest;
            param.coreParam.pveTest_battleMapId = 1;
            param.coreParam.roguelike_SceneId = 102006;
            param.frameDrverType = FrameDriveType.Local;
            BattleContainer.CreateBattleInstanceInBackground(param);
            */
        }
        else if (msg == "roguelike")
        {
            UIEngine.Stuff.ForwardOrBackTo("TempRogueLikePage");
        }
        else if (msg == "roguelike_test")
        {
            var param = new BattleCoreParam();
            param.mode = BattleModeType.Roguelike;
            //param.roguelike_SceneId = 102001;
            //BattlePipline.CreateBattleInstanceInBackground(param);
            Debug.LogError("-----暂时关闭入口，陈飞");
        }
        else if (msg == "plot_test")
        {
            // var elemt = ProtoStaticData.StoryManageTable.ElementList;
            // UIEngine.Stuff.transform.gameObject.SetActive(false);
            var plotInfo = new PlotInfo
                {StageId = 101, EventType = EPlotEventType.StartBattle, OnComp = this.OnPlotTestComp};
            PlotJsUtil.Trigger(plotInfo);
        }

        else if (msg == "editor")
        {
            UIEngine.Stuff.ForwardOrBackTo("TempEditorPage");
        }
        else if (msg == "roleEnv")
        {
            SceneManager.LoadScene("RoleEnv", LoadSceneMode.Additive);
            UIEngine.Stuff.transform.gameObject.SetActive(false);
        }
        else if (msg == "mainPage")
        {
            UIEngine.Stuff.ForwardOrBackTo("MainPage");
        }
        else if (msg == "back")
        {
            UIEngine.Stuff.Back();
        }
        //else if(msg == "wenjuan")
        //{
        //    PpsManager.TrySurveyAsync();
        //}
    }

    private void OnPlotTestComp()
    {
        Debug.Log("当前剧情已播放完成");
    }
}