using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainConsoleOKBtn : MonoBehaviour
{
    public InputField inputField;

    public MainConsoleWind mainConsole;

    int _inputLogIndex = 0;
    public int inputLogIndex{
        get{
            return _inputLogIndex;
        }
        set{
            _inputLogIndex = value;
        }
    }

    List<string> _similarCommands = null;
    List<string> similarCommands{
        get{
            return _similarCommands;
        }
        set{
            _similarCommands = value;
        }
    }

    int similarCommandIndex = 0;

    public void OnButton(string msg)
    {
        if(inputField.text.Trim() == ""){
            inputField.text = MainConsoleWind.NextGuessCmd;
            inputField.ActivateInputField();
            inputField.MoveTextEnd(false);
            return;
        }
        similarCommands = null;
        var inputStr = inputField.text;
        inputStr = inputStr.Replace("\n","");
        inputField.text = "";
        inputStr = inputStr.Trim();
        if(inputStr != ""){
            mainConsole.InputCmd(inputStr);
        }
        inputField.ActivateInputField();

        inputLogIndex = mainConsole.inputLogs.Count;
    }

    public void Awake()
    {
        inputField.onValueChanged.AddListener((val) => {
            // 中文模式下输入~号时也关闭控制台
            if(val.TrimEnd() == "·"){
				UIEngine.Stuff.RemoveFloating<GameDashboardFloating>();
                return;
            }
            if(similarCommands != null){
                var trimed = val.Replace("\t", "");
                trimed = trimed.Trim();
                if(!similarCommands.Contains(trimed)){
                    similarCommands = null;
                }
            }
            if(mainConsole.inputLogs.Count == 0){
                return;
            }

            // 重置指令提示index
            if(inputLogIndex != mainConsole.inputLogs.Count && val != mainConsole.inputLogs[inputLogIndex]){
                inputLogIndex = mainConsole.inputLogs.Count;
            }
        });

    }

    void OldCommand(bool up){
        if(mainConsole.inputLogs.Count == 0){
            return;
        }
        inputLogIndex += up ? -1 : 1;

        if(inputLogIndex < 0){
            inputLogIndex = 0;
        }
        if(inputLogIndex > mainConsole.inputLogs.Count - 1){
            inputLogIndex = mainConsole.inputLogs.Count - 1;
        }

        var cmd = mainConsole.inputLogs[inputLogIndex];
        inputField.text = cmd.Trim();
        inputField.ActivateInputField();
        inputField.MoveTextEnd(false);
    }

    string SetSimilarCommand(){
        if(similarCommands == null){
            similarCommands = MainConsoleWind.GetSimilarityCommand(inputField.text);
            similarCommandIndex = 0;
        }

        var result = similarCommands[similarCommandIndex];
        similarCommandIndex ++;
        if(similarCommandIndex >= similarCommands.Count){
            similarCommandIndex = 0;
        }
        return result;
    }
    
    public void Update(){
        if(!Application.isEditor){
            return;
        }
        if(!inputField.isFocused && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))){
            inputField.ActivateInputField();
            return;
        }
        if(!inputField.isFocused){
            return;
        }
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)){
            OnButton("");
        }
        if(Input.GetKeyDown(KeyCode.UpArrow)){
            OldCommand(true);
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow)){
            OldCommand(false);
        }
        if(Input.GetKeyDown(KeyCode.Tab)){
            var str = SetSimilarCommand();
            inputField.text = str.Trim();
            inputField.ActivateInputField();
            inputField.MoveTextEnd(false);
        }
    }
}
