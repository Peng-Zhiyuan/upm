using UnityEngine;

public partial class HelpView : MonoBehaviour
{
    [Tooltip("留空表示与页面预制件同名")]
    public string key;

    private void Start()
    {
        if(string.IsNullOrEmpty(key))
        {
            key = UIEngine.Stuff.Top.name;
        }

        var helpRow = StaticData.HelpTable.TryGet(key);
        if(helpRow == null)
        {
            this.Text_title.text = $"{{help: {key}}}";
            this.Button_help.SetActive(false);
            return;
        }
        var title = helpRow.Title.Localize();
        this.Text_title.text = title;

        var help = helpRow.Helps;

        this.Button_help.SetActive(help.Count > 0);
    }


    public void OnClick(string msg)
    {
        if(msg == "help")
        {
            // ToastManager.ShowLocalize("Not Available");
            // return;
            UIEngine.Stuff.ForwardOrBackTo<HelpInfoPage>(key);
        }
    }
}