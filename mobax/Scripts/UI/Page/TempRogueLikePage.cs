using BattleSystem.ProjectCore;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempRogueLikePage : Page
{
    public async void OnButton(string msg)
    {
        if (msg == "streetBusiness")
        {
            var param = new BattleCoreParam();
            param.mode = BattleModeType.Roguelike;
            //param.roguelike_SceneId = 102007;
            //BattlePipline.CreateBattleInstanceInBackground(param);
            Debug.LogError("-----暂时关闭入口，陈飞");
        }
        else if (msg == "street")
        {
            var param = new BattleCoreParam();
            param.mode = BattleModeType.Roguelike;
            //param.roguelike_SceneId = 102006;
            //BattlePipline.CreateBattleInstanceInBackground(param);
            Debug.LogError("-----暂时关闭入口，陈飞");
        }
        else if (msg == "seaBoard")
        {
            var param = new BattleCoreParam();
            param.mode = BattleModeType.Roguelike;
            //param.roguelike_SceneId = 102005;
            //BattlePipline.CreateBattleInstanceInBackground(param);
            Debug.LogError("-----暂时关闭入口，陈飞");
        }

        if (msg == "streetBusinessPreview")
        {
            var corePreview = new MapGenerateCorePreview(102002);
            corePreview.Generate();
            await SceneUtil.AddressableLoadSceneAsync("Env_AKY_Street_New_All", LoadSceneMode.Additive);
        }
        else if (msg == "streetPreview")
        {
            var corePreview = new MapGenerateCorePreview(102006);
            corePreview.Generate();
            SceneManager.LoadScene("RoguelikePreview", LoadSceneMode.Additive);
        }
        else if (msg == "seaBoardPreview")
        {
            var corePreview = new MapGenerateCorePreview(102005);
            corePreview.Generate();
            SceneManager.LoadScene("RoguelikePreview", LoadSceneMode.Additive);
        }
        else if (msg == "back")
        {
            UIEngine.Stuff.Back();
        }
    }
}