using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class MainConsoleWind : MonoBehaviour
{
    public Text consoleTextPrefab;

    public InputField inputField;

    public Transform content;
    public MainConsoleOKBtn okBtn;

    public List<string> inputLogs = new List<string>();

    Queue<Text> outputLogs = new Queue<Text>();

    int maxInputLogCount = 200;

    int maxOutputLogCount = 1000;

    static Dictionary<string, List<string>> alias = new Dictionary<string, List<string>>();
    static Dictionary<string, List<string>> methodsTags = new Dictionary<string, List<string>>();

    static Type _consoleCmdClassType = typeof(GameConsoleCmds);
    public static Type consoleCmdClassType{
        get{
            return _consoleCmdClassType;
        }
        set{
            _consoleCmdClassType = value;
        }
    }

    static bool _showGameLog = false;

    public static bool ShowGameLog{
        set{
            _showGameLog = value;
            if(value){
                Application.logMessageReceived -= GameLogCallback;
                Application.logMessageReceived += GameLogCallback;
            }
            else{
                Application.logMessageReceived -= GameLogCallback;
            }
        }
        get{
            return _showGameLog;
        }
    }

    public static bool showLogStack = false;

    // 被禁止掉的C#/Unity自带的方法
    public static List<string> banFunctions = new List<string>(){"Equals", "GetHashCode", "GetType", "ToString"};

    // 隐藏的方法，不会显示在Help中
    public static List<string> hideFunctions = new List<string>();

    // 仅开发模式可用的方法
    public static List<string> developOnlyFunctions = new List<string>();

    public static string ErrorLog = "";

    static string _nextGuessCmd = "";
    
		
    /// <summary>
    /// 下一次Enter键自动出现的猜测指令
    /// </summary>
    public static string NextGuessCmd{
        get{
            return _nextGuessCmd;
        }
        set{
            _nextGuessCmd = value.Trim();
            Debug.Log($"[MainConsoleWind] Next guess cmd : {_nextGuessCmd}");
        }
    }

    static List<Type> GetCmdClassTypes(){
        // List<Type> types = new List<Type>();
        
        // var assemblyList = AppDomain.CurrentDomain.GetAssemblies();
        // foreach(var asm in assemblyList){            
        //     var asmName = asm.GetName().Name;
        //     var isUseless = IsUselessAssembly(asmName);
        //     if(isUseless){
        //         continue;
        //     }
        //     UnityEngine.Debug.Log(asm.GetName());
        //     foreach (Type type in asm.GetTypes())
        //     {
        //         if(type.IsSubclassOf(consoleCmdClassType) || type == consoleCmdClassType){
        //             types.Add(type);
        //         }
        //     }
        //     types.Remove(consoleCmdClassType);
        //     types.Insert(0, consoleCmdClassType);
        // }
        var types = ReflectionUtil.GetSubClassesInAllAssemblies<GameConsoleCmds>();
        types.Add(typeof(GameConsoleCmds));
        return types;
        return types;
    }

    static Type GetCmdOriginClass(string method){
        var types = GetCmdClassTypes();
        foreach(var t in types){
            var m = t.GetMethod(method);
            if(m != null){
                return t;
            }
        }
        return null;
    }

    public static MethodInfo GetCmdMethod(string methodName){
        var classes = GetCmdClassTypes();
        foreach(var c in classes){
            var m = c.GetMethod(methodName);
            if(m != null){
                return m;
            }
        }
        return null;
    }

    public static List<MethodInfo> GetCmdMethods(){
        List<MethodInfo> methods = new List<MethodInfo>();
        var classes = GetCmdClassTypes();
        foreach(var c in classes){
            var ms = c.GetMethods();
            foreach(var m in ms){
                methods.Add(m);
            }
        }
        return methods;
    }

    public static List<FieldInfo> GetCmdFields(){
        List<FieldInfo> fields = new List<FieldInfo>();
        var classes = GetCmdClassTypes();
        foreach(var c in classes){
            var ms = c.GetFields();
            foreach(var m in ms){
                fields.Add(m);
            }
        }
        return fields;
    }

    public static FieldInfo GetCmdField(string methodName){
        var classes = GetCmdClassTypes();
        foreach(var c in classes){
            var m = c.GetField(methodName);
            if(m != null){
                return m;
            }
        }
        return null;
    }

    public static void ShowStringToCmd(string str, Color color){
        var floating = UIEngine.Stuff.FindFloating<GameDashboardFloating>();
        if(floating != null){
            var wind = floating.mainConsoleWind;
            wind.ShowString(str, color);
        }
        else{
            Debug.LogWarning("[MainConsoleWind] Console not opening");
            Debug.Log(str);
        }
    }

    public void Awake()
    {
        consoleTextPrefab.gameObject.SetActive(false);

        var methods = GetAvaliableMethods();
        foreach(var m in methods){
            var aliasNames = GetCmdField($"{m}_Alias");
            if(aliasNames != null){
                if(aliasNames.FieldType != typeof(List<string>)){
                    throw new Exception($"[MainConsoleWind] {m}_Alias is not a List<string> value function");
                }
                alias[m] = aliasNames.GetValue(null) as List<string>;
            }

            var hiden = GetCmdField($"{m}_Hide");
            if(hiden != null){
                if(hiden.FieldType != typeof(bool)){
                    throw new Exception($"[MainConsoleWind] {m}_Hide is not a bool value function");
                }
                var hidenValue = (bool)hiden.GetValue(null);
                if(hidenValue){
                    hideFunctions.Add(m);
                }
            }

            var developOnly = GetCmdField($"{m}_DevelopmentOnly");
            if(developOnly != null){
                if(developOnly.FieldType != typeof(bool)){
                    throw new Exception($"[MainConsoleWind] {m}_DevelopmentOnly is not a bool value function");
                }
                var developValue = (bool)developOnly.GetValue(null);
                if(developValue){
                    developOnlyFunctions.Add(m);
                }
            }

            var tags = GetCmdField($"{m}_Tags");
            if(tags != null){
                if(tags.FieldType != typeof(List<string>)){
                    throw new Exception($"[MainConsoleWind] {m}_Tags is not a List<string> value function");
                }
                var tagsValue = (tags.GetValue(null)) as List<string>;
                foreach(var tag in tagsValue){
                    if(methodsTags.ContainsKey(tag)){
                        methodsTags[tag].Add(m);
                    }
                    else{
                        methodsTags[tag] = new List<string>(){m};
                    }
                }
            }

            try{
                var listStr = PlayerPrefs.GetString($"{nameof(MainConsoleWind)}_InputLogs", "");
                if(listStr != ""){
                    inputLogs = StringUtil.String2List(listStr);
                    okBtn.inputLogIndex = inputLogs.Count;
                }
            }
            catch(Exception e){
                Debug.LogError(e);
                inputLogs = new List<string>();
                okBtn.inputLogIndex = 0;
                PlayerPrefs.SetString($"{nameof(MainConsoleWind)}_InputLogs", "");
            }
        }

        // 子类的总tag
        var types = GetCmdClassTypes();
        foreach(var type in types){
            var sumTag = type.GetField($"{type.Name}_Tags");
            if(sumTag != null){
                
                if(sumTag.FieldType != typeof(List<string>)){
                    throw new Exception($"[MainConsoleWind] {type.Name}_Tags is not a List<string> value function");
                }
                var tagsValue = (sumTag.GetValue(null)) as List<string>;
                foreach(var m in type.GetMethods()){
                    foreach(var tag in tagsValue){
                        if(methodsTags.ContainsKey(tag)){
                            methodsTags[tag].Add(m.Name);
                        }
                        else{
                            methodsTags[tag] = new List<string>(){m.Name};
                        }
                    }
                }
            }
        }
        
        AliasCheckPass();
    }

    static void GameLogCallback(string condition, string stackTrace, LogType type){
        if(showLogStack){
            ShowStringToCmd($"{condition}\n{stackTrace}", GetLogColor(type));
        }
        else{
            ShowStringToCmd(condition, GetLogColor(type));
        }
    }

    public static int GetMethodParaNumber(string funcName){
        if(!ConstainsFunction(funcName)){
            throw new Exception($"[MainConsoleWind] Method {funcName} do not exist");
        }
        return GetCmdMethod(funcName).GetParameters().Length;
    }

    public static void ShowHelp(bool showAlias = false){
        var methods = GetHelpableMethods();
        
        foreach(var m in methods){
            ShowHelp(m, showAlias);
        }
    }

    public static void ShowTagHelp(string tag, bool showAlias = false){
        if(!methodsTags.ContainsKey(tag)){
            
            var tags = new List<string>(methodsTags.Keys);
            var guess = MainConsoleWind.GetSimilarityStringList(tag, tags)[0];
            MainConsoleWind.NextGuessCmd = $"HelpTag {guess}";
            MainConsoleWind.ShowStringToCmd($"HelpTag [{tag}] not found. Are you looking for [{guess}] ?", Color.red);
            return;
        }

        var methods = methodsTags[tag];
        
        foreach(var m in methods){
            ShowHelp(m, showAlias);
        }
    }

    public static List<string> GetHelpableMethods(bool aliasInclude = false){
        var methods = GetCmdMethods();
        List<string> strs = new List<string>();
        
        foreach(var m in methods){
            if(banFunctions.Contains(m.Name)){
                continue;
            }
            if(hideFunctions.Contains(m.Name)){
                continue;
            }
            if(!Debug.isDebugBuild && developOnlyFunctions.Contains(m.Name)){
                continue;
            }
            if(!m.IsPublic){
                continue;
            }
            strs.Add(m.Name);
            
            if(aliasInclude && alias.ContainsKey(m.Name)){
                foreach(var al in alias[m.Name]){
                    strs.Add(al);
                }
            }
        }

        return strs;
    }

    public static List<string> GetAvaliableMethods(bool aliasInclude = false){
        var methods = GetCmdMethods();
        List<string> strs = new List<string>();
        
        foreach(var m in methods){
            if(banFunctions.Contains(m.Name)){
                continue;
            }
            if(!m.IsPublic){
                continue;
            }
            strs.Add(m.Name);
            
            if(aliasInclude && alias.ContainsKey(m.Name)){
                foreach(var al in alias[m.Name]){
                    strs.Add(al);
                }
            }
        }
        return strs;
    }

    public static List<string> GetSimilarityCommand(string cmd){
        cmd = cmd.Trim();
        List<string> names = GetHelpableMethods(true);
        return GetSimilarityStringList(cmd, names);
    }

		
    /// <summary>
    /// 对数组strs按照cmd进行相似度排序
    /// 数组原地排序
    /// </summary>
    public static List<string> GetSimilarityStringList(string cmd, List<string> strs, bool showDebugLog = false){
        strs.Sort((x, y) => StringUtil.LevenshteinConsoleDistancePercent(y, cmd).CompareTo(StringUtil.LevenshteinConsoleDistancePercent(x, cmd)));
        
        if(showDebugLog){
            foreach(var n in strs){
                Debug.Log($"{n} : {cmd} : {StringUtil.LevenshteinConsoleDistancePercent(n, cmd)}");
            }
        }

        return strs;
    }

    public static void ShowHelp(string methodName, bool showAlias){
        methodName = GetAliasOriginalName(methodName);
        if(banFunctions.Contains(methodName)){
            ShowStringToCmd($"[MainConsoleWind] Command {methodName} is not valid.", Color.red);
            return;
        }
        if(!Debug.isDebugBuild && developOnlyFunctions.Contains(methodName)){
            ShowStringToCmd($"[MainConsoleWind] Command {methodName} is a development build only command.", Color.red);
            return;
        }

        // 显示指令名，参数
        // Command [arg1] [arg2]
        var method = GetCmdMethod(methodName);
        StringBuilder sb = new StringBuilder();
        sb.Append(method.Name);
        var paras = method.GetParameters();
        foreach(var p in paras){
            sb.Append(" [");
            sb.Append(p.Name);
            sb.Append("]");
        }
        ShowStringToCmd(sb.ToString(), Color.yellow);

        
        // 显示标签名
        // -Tags : (base) (console)
        StringBuilder sbTag = new StringBuilder();
        sbTag.Append("    -Tags : ");
        foreach(var p in methodsTags.Keys){
            if(methodsTags[p].Contains(method.Name)){
                sbTag.Append(" (");
                sbTag.Append(p);
                sbTag.Append(")");
            }
        }
        var sbTagString = sbTag.ToString();
        if(sbTagString.Trim() != "-Tags :"){
            ShowStringToCmd(sbTagString, Color.yellow);
        }

        // 显示帮助
        try{
            var field = GetCmdField($"{methodName}_Help");
            var helpStr = field.GetValue(null);
            ShowStringToCmd($"{helpStr}", Color.white);
        }
        catch(Exception e){
            Debug.Log($"[MainConsoleWind] Error during get filed {methodName}_Help");
            Debug.LogWarning(e);
        }

        // 显示别名
        if(showAlias && alias.ContainsKey(methodName)){
            foreach(var al in alias[methodName]){
                ShowStringToCmd($"    -Or : {al}", Color.white);
            }
        }
        ShowStringToCmd("", Color.yellow);
    }

    public void ClearConsole(){
        while(outputLogs.Count > 0){
            var delete = outputLogs.Dequeue();
            GameObject.DestroyImmediate(delete.gameObject);
        }
        MoveToScrollBottomInBackground();
    }

    public async void OnEnable()
    {
        MoveToScrollBottomInBackground();
        //await TimeWaiterManager.Stuff.WaitFrameAsync(1);
        await Task.Delay(1);
        inputField.ActivateInputField();
        inputField.MoveTextEnd(false);
        if(inputField.text.StartsWith("`") || inputField.text.StartsWith("·")){
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        }
    }

    async void MoveToScrollBottomInBackground(){
        //await TimeWaiterManager.Stuff.WaitFrameAsync(1);
        await Task.Delay(1);
        var trans = (content as RectTransform);
        trans.anchoredPosition = new Vector2(0, trans.sizeDelta.y);
    }

    public void OnDisable()
    {
        ShowGameLog = false;
        if(inputField.text.EndsWith("`") || inputField.text.StartsWith("·")){
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        }
    }

    public static string GetAliasOriginalName(string name){
        if(alias.ContainsKey(name)){
            return name;
        }
        else{
            foreach(var k in alias.Keys){
                foreach(var n in alias[k]){
                    if(n == name){
                        return k;
                    }
                }
            }
        }
        return name;
    }

    static bool AliasCheckPass(){
        Dictionary<string, string> strTemp = new Dictionary<string, string>();
        var methods = GetAvaliableMethods();
        foreach(var m in methods){
            strTemp[m] = m;
        }
        foreach(var key in alias.Keys){
            foreach(var name in alias[key]){
                if(strTemp.ContainsKey(name)){
                    throw new System.Exception($"[MainConsoleWind] Alias got multiple definition : {name}");
                }
                else{
                    strTemp[name] = name;
                }
            }
        }
        return true;
    }

    public static bool InputWorking{
        get{
            var floating = UIEngine.Stuff.FindFloating<GameDashboardFloating>();
            if(floating != null){
                return floating.mainConsoleWind.inputField.isFocused;
            }
            else{
                return false;
            }
        }
    }

    static bool ConstainsFunction(string funcName){
        var f = GetCmdMethod(funcName);
        if(f != null){
            return true;
        }
        return false;
    }

    void CallFunction(string funcName, string[] args){
        GetCmdOriginClass(funcName).InvokeMember(funcName,
        System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, null,
        args);
    }

    public void ShowString(string str, Color color){
        var newTextGo = GameObject.Instantiate(consoleTextPrefab.gameObject);
        newTextGo.SetActive(true);
        var newText = newTextGo.GetComponent<Text>();
        var newTextBtn = newTextGo.GetComponent<Button>();
        outputLogs.Enqueue(newText);
        newText.text = str;
        newText.color = color;
        newText.transform.SetParent(content, false);
        newTextBtn.onClick.AddListener(async () => {
            if(newText.color != Color.red){
                var targetText = newText.text.Trim();
                if(targetText.Contains("-> ")){
                    targetText = targetText.Replace("-> ", "");
                }
                inputField.text += targetText;
                
                inputField.ActivateInputField();
                //await TimeWaiterManager.Stuff.WaitFrameAsync(1);
                await Task.Delay(1);
                inputField.MoveTextEnd(false);
            }
        });

        RefreshOutput();
    }

    public void InputCmd(string cmd){
        try{
            if(inputLogs.Count == 0 || cmd.Trim() != inputLogs[inputLogs.Count - 1].Trim()){
                inputLogs.Add(cmd);
                // 超过一万行记录就清除
                RefreshInputLog();
            }
        }
        catch(Exception e){
            Debug.LogError(e);
        }

        ShowString($" -> {cmd}", Color.white);

        var cmds = GetClearCmds(cmd);
        
        if(cmds.Count == 0){
            return;
        }
        
        if(!Debug.isDebugBuild && developOnlyFunctions.Contains(cmds[0])){
            ShowString($" -Command : [{cmds[0]}] is a development build only command.", Color.red);
            return;
        }

        if(!ConstainsFunction(cmds[0])){
            NextGuessCmd = GetSimilarityCommand(cmds[0])[0];
            ShowString($" -Command : [{cmds[0]}] do not exist. Are you looking for [{NextGuessCmd}]?", Color.red);
            return;
        }

        NextGuessCmd = "";

        if(banFunctions.Contains(cmds[0])){
            ShowString($" -Command : [{cmds[0]}] is not valid.", Color.red);
            return;
        }
        if(cmds.Count - 1 != GetMethodParaNumber(cmds[0])){
            ShowString($" -Command : [{cmds[0]}] param number wrong.", Color.red);
            ShowHelp(cmds[0], false);
            return;
        }

        var mainCmd = cmds[0];
        string[] arg = new string[]{};
        if(cmds.Count > 1){
            cmds.RemoveAt(0);
            arg = cmds.ToArray();
        }

        try{
            CallFunction(mainCmd, arg);
        }
        catch(Exception e){
            Debug.LogError(e);
            ErrorLog = e.ToString();
            ShowString("发生错误，使用指令ShowConsoleError查看.", Color.red);
        }
    }

    public static void ShowConsoleError(){
        ShowStringToCmd(ErrorLog, Color.red);
    }

    List<string> GetClearCmds(string cmd){
        var cmds = cmd.Split(' ');
        List<string> clearCmds = new List<string>();
        foreach(var c in cmds){
            var cmdClear = c.Trim();
            if(cmdClear != ""){
                var alias = GetAliasOriginalName(cmdClear);
                clearCmds.Add(alias);
            }
        }
        return clearCmds;
    }

    void RefreshInputLog(){
        while(inputLogs.Count > maxInputLogCount){
            inputLogs.RemoveAt(0);
        }
        
        var logStr = StringUtil.List2String(inputLogs);
        PlayerPrefs.SetString($"{nameof(MainConsoleWind)}_InputLogs", logStr);
    }    
    
    void RefreshOutput(){
        while(outputLogs.Count > maxOutputLogCount){
            var delete = outputLogs.Dequeue();
            GameObject.DestroyImmediate(delete.gameObject);
        }
        MoveToScrollBottomInBackground();
    }

    public static Color GetLogColor(LogType logType){
        if(logType == LogType.Assert || logType == LogType.Error || logType == LogType.Exception){
            return Color.red;
        }
        else if(logType == LogType.Warning){
            return Color.yellow;
        }
        else{
            return Color.white;
        }
    }
}
