using BattleSystem.ProjectCore;
using UnityEngine.SceneManagement;

public class TempEditorPage : Page
{
    public async void OnButton(string msg)
    {
        if (msg == "roleRoom")
        {
            SceneManager.LoadScene("RoleRoom", LoadSceneMode.Additive);
            UIEngine.Stuff.transform.gameObject.SetActive(false);
        }
        else if (msg == "skillEditor")
        {
            // var param = new BattleCoreParam();
            // param.mode = BattleModeType.SkillEditor;
            //BattlePipline.CreateBattleInstanceInBackground(param);
        }
        else if (msg == "stageEditor")
        {
            SceneManager.LoadScene("StageMaker", LoadSceneMode.Additive);
            UIEngine.Stuff.transform.gameObject.SetActive(false);
        }
        else if (msg == "plotChatEditor")
        {
            SceneManager.LoadScene("PlotChatPreview", LoadSceneMode.Additive);
            UIEngine.Stuff.transform.gameObject.SetActive(false);
        }
        else if (msg == "emotionEditor")
        {
            SceneManager.LoadScene("EmotionScreen", LoadSceneMode.Additive);
            UIEngine.Stuff.transform.gameObject.SetActive(false);
        }
        else if (msg == "plot_comics")
        {
            SceneManager.LoadScene("PlotComicsMaker", LoadSceneMode.Additive);
            UIEngine.Stuff.transform.gameObject.SetActive(false);
        }
        else if (msg == "stageWave")
        {
            SceneManager.LoadScene("StageWaveScene", LoadSceneMode.Additive);
            UIEngine.Stuff.transform.gameObject.SetActive(false);
        }
        else if (msg == "back")
        {
            UIEngine.Stuff.Back();
        }
    }
}