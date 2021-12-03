using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

public static class TestManager
{
    public static List<TestCase> testCaseList = new List<TestCase>();

    static TestGUI _testGUI;
    public static TestGUI TestGUI
    {
        get
        {
            if(!Application.isPlaying)
            {
                return null;
            }
            if(_testGUI == null)
            {
                var go = new GameObject();
                go.name = "TestGUI";
                var comp = go.AddComponent<TestGUI>();
                _testGUI = comp;
            }
            return _testGUI;
        }
    }

    static void RecreateTestCaseList(List<Type> selectedTypeList = null)
    {
        testCaseList.Clear();
        if(selectedTypeList == null)
        {
            var thisAssembly = typeof(TestManager).Assembly;
            var subClassTypeList = ReflectionUtil.GetSubClasses<TestCase>(thisAssembly);
            foreach (var type in subClassTypeList)
            {
                var testCase = Activator.CreateInstance(type) as TestCase;
                testCaseList.Add(testCase);
            }
        }
        else
        {
            foreach(var type in selectedTypeList)
            {
                var testCase = Activator.CreateInstance(type) as TestCase;
                testCaseList.Add(testCase);
            }
        }

        var count = testCaseList.Count;
        Debug.Log($"[TestManager] {count} TestCase created");
    }

    public static TestConfig config = null;

    static void RecreateConfigList()
    {
        var thisAssembly = typeof(TestManager).Assembly;
        var subClassTypeList = ReflectionUtil.GetSubClasses<TestConfig>(thisAssembly);
        var type = subClassTypeList[0];
        config = Activator.CreateInstance(type) as TestConfig;
    }

    static int successCount = 0;
    static int filedCount = 0;
    static int completeCount = 0;
    static int totalCount = 0;
    static TestCase currentTestCase;
    static string currentTestCaseName;


    public static void WriteToCurrentLogFile(string msg)
    {
        AppendLineToFile(currentLogFilePath, msg);
    }

    static string currentLogFilePath;
    static async Task RunAllCreatedTestCaseAsync()
    {
        successCount = 0;
        filedCount = 0;
        completeCount = 0;
        totalCount = testCaseList.Count;

        var logFilePath = CreateNewLogFilePath();
        currentLogFilePath = logFilePath;
        WriteToCurrentLogFile($"start date: {DateTime.Now}");
        WriteToCurrentLogFile($"total test count: {totalCount}");

        for (int i = 0; i < testCaseList.Count; i++)
        {
            var testCase = testCaseList[i];
            currentTestCase = testCase;
            currentTestCaseName = testCase.GetType().Name;

            Debug.Log($"[TestManager] Run TestCase {currentTestCaseName} ({completeCount+1}/{totalCount})");
            WriteToCurrentLogFile($"Run TestCase {currentTestCaseName} ({completeCount+1}/{totalCount})");

            RefreshGuiTitle();
            await config.OnPreOneCaseStart(testCase);
            bool isSuccess;
            Exception exception = null;
            try
            {
                await testCase.RunAsync();
                isSuccess = true;
                successCount++;
                completeCount++;
                Debug.Log($"[TestManager] Complete TestCase {currentTestCaseName}: Success");
                WriteToCurrentLogFile($"{currentTestCaseName} Success");
            }
            catch(Exception e)
            {
                isSuccess = false;
                exception = e;
                filedCount++;
                completeCount++;
                Debug.Log($"[TestManager] Complete TestCase {currentTestCaseName}: Fail");
                WriteToCurrentLogFile($"{currentTestCaseName} Fail");
                if (e is TestException)
                {
                    var msg = e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace;
                    Debug.LogError(msg);
                    WriteToCurrentLogFile(msg);
                }
                else
                {
                    Debug.LogException(e);

                    var msg = e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace;
                    WriteToCurrentLogFile(msg);
                }
            }
            await config.OnAfterOneCaseCompelte(testCase, isSuccess, exception);
            //currentTestCase = null;
        }
        Refres2();

        Debug.Log($"[TestManager] All {totalCount} Test Compelte, Success: {successCount}, Fail: {filedCount}");
        WriteToCurrentLogFile($"All {totalCount} Test Compelte, Success: {successCount}, Fail: {filedCount}");

    }

    static void Refres2()
    {
        var msg = $"Completed ({completeCount}/{totalCount}, success: {successCount}, failed: {filedCount})";
        TestGUI.title = msg;
    }


    static void RefreshGuiTitle()
    {
        var msg = $"Auto Testing: { currentTestCase.GetType().Name} ({completeCount+1}/{totalCount}, success: {successCount}, failed: {filedCount})";
        TestGUI.title = msg;
    }

    public static bool isTesting;
    public static async void CreateThenRunAllTest(List<Type> selectedTypeList = null)
    {
        if (!Application.isPlaying)
        {
            throw new Exception($"[TestManager] 需要运行游戏");
        }

        if(selectedTypeList != null)
        {
            var count = selectedTypeList.Count;
            if(count == 0)
            {
                throw new Exception($"[TestManager] 至少需要 1 个测试用例");
            }
        }

        try
        {
            isTesting = true;
            RecreateTestCaseList(selectedTypeList);
            RecreateConfigList();

            await RunAllCreatedTestCaseAsync();
        }
        finally
        {
            isTesting = false;
        }
       
    }

    [MenuItem("pzy.com.*/AutoTesting/Start")]
    public static void TopMenuStartTest()
    {
        CreateThenRunAllTest();
    }

    public static string CreateNewLogFilePath()
    {
        var now = DateTime.Now;
        var year = now.Year;
        var month = now.Month;
        var day = now.Day;
        var hour = now.Hour;
        var min = now.Minute;
        var sec = now.Second;
        var fileName = $"{year}-{month}-{day}-{hour}-{min}-{sec}.txt";
        var dir = $"AutoTesting/{year}-{month}-{day}";
        var path = $"{dir}/{fileName}";
        return path;
    }

    public static void AppendLineToFile(string filePath, string msg)
    {
        var parentDir = Path.GetDirectoryName(filePath);
        var isParentDirExtist = Directory.Exists(parentDir);
        if(!isParentDirExtist)
        {
            Directory.CreateDirectory(parentDir);
        }

        var date = DateTime.Now;
        
        File.AppendAllText(filePath, $"[{date}] {msg}\n");
        //Debug.LogError(filePath);

    }

}
