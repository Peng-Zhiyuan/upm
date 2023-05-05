using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor;
using System;
using System.Threading.Tasks;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using System.IO;
using CustomLitJson;
using System.Security.Cryptography;

public class UpmSynchrom : BuildSchema
{
    string repoPath = "D:/upm";
    List<string> processedDomain = new List<string> { "com.pzy.", "com.tianyou" };

    public override async void Build(Dictionary<string, string> paramDic)
    {
        // 同步包与 UpmGit 工程
        var infoList = await PackageUtil.GetEmbededPackageInfoListAsync();
        var pushedCount = 0;
        foreach(var info in infoList)
        {
            var isPushed = ProcessOnePackage(info);
            if(isPushed)
            {
                pushedCount++;
            }
        }
        Debug.Log($"[UpmSycrhom] completed. {pushedCount} package(s) pushed");

        if(pushedCount > 0)
        {
            AssetDatabase.Refresh();
        }
        
    }

    bool IsProcessed(string domain)
    {
        foreach(var key in processedDomain)
        {
            if(domain.Contains(key))
            {
                return true;
            }
        }
        return false;
    }

    bool ProcessOnePackage(PackageInfo info)
    {
        var packageName = info.name;
        var projectPackagePath = info.resolvedPath;
        if(projectPackagePath.Contains("@"))
        {
            return false;
        }

        var isCare = IsProcessed(packageName);
        if(!isCare)
        {
            return false;
        }

        projectPackagePath = projectPackagePath.Replace('\\', '/');
        var repoPackagePath = $"{repoPath}/Packages/{packageName}";
        var repoPackageJsonPath = $"{repoPackagePath}/package.json";

        var isRepoPackageExists = Directory.Exists(repoPackagePath);
        var isRepoPackageJsonExists = File.Exists(repoPackageJsonPath);
            
        if(!isRepoPackageExists || !isRepoPackageJsonExists)
        {
            // 仓库中没有这个包，直接拷贝过去
            PShellUtil.CopyTo(projectPackagePath, repoPackagePath);
            return true;
        }


        // 比较文件是否相同
        var isSame = CompareDirectory(projectPackagePath, repoPackagePath, new List<string> {"package.json"});
        if(!isSame)
        {
            // 文件不同

            // 拷贝文件
            DeleteFileInDir(repoPackagePath, new List<string> { "package.json" });
            PShellUtil.CopyTo(projectPackagePath, repoPackagePath, PShellUtil.FileExsitsOption.Override, PShellUtil.DirectoryExsitsOption.Override, new string[] { "package.json"});
            
            // 升级仓库 pakcage 的版本号
            //var repoPackageJsonPath = $"{repoPackagePath}/package.json";
            var jo = ReadJson(repoPackageJsonPath);
            var oldVersion = jo["version"].ToString();
            var newVersion = IncreaseVersion(oldVersion);
            jo["version"] = newVersion;
            WriteJson(jo, repoPackageJsonPath);

            // 同步新版本号到本地工程的包
            var localPackageJsonPath = $"{projectPackagePath}/package.json";
            var jo2 = ReadJson(localPackageJsonPath);
            jo2["version"] = newVersion;
            WriteJson(jo2, localPackageJsonPath);

            Debug.Log($"{packageName} pushed: new version: {newVersion}");
            return true;
        }
        else
        {
            return false;
        }
       
    }

    JsonData ReadJson(string path)
    {
        var text = File.ReadAllText(path);
        var jo = JsonMapper.Instance.ToObject(text);
        return jo;
    }

    void WriteJson(JsonData jd, string path)
    {
        var text = JsonMapper.Instance.ToJson(jd);
        File.WriteAllText(path, text);
    }

    string IncreaseVersion(string version)
    {
        var parts = version.Split('.');
        var lastIndex = parts.Length - 1;
        var lastString = parts[lastIndex];
        var lastNumber = int.Parse(lastString);
        var newLastNumber = lastNumber + 1;
        parts[lastIndex] = newLastNumber.ToString();
        var newVersion = string.Join(".", parts);
        return newVersion;
    }

    JsonData ReadPackageJson(string packagePath)
    {
        var filePath = $"{packagePath}/package.json";
        var text = File.ReadAllText(filePath);
        var jo = JsonMapper.Instance.ToObject(text);
        return jo;
    }

    bool CompareDirectory(string aDirPath, string bDirPath, List<string> exludeFileNameList)
    {
        if(aDirPath.Contains("uiengine"))
        {
            Debug.Log("uiengine");
        }

        var aEntry = Directory.GetFileSystemEntries(aDirPath, "*", SearchOption.AllDirectories);
        var bEntry = Directory.GetFileSystemEntries(bDirPath, "*", SearchOption.AllDirectories);

        var aRelativePathToIsFileDic = new Dictionary<string, bool>();
        var bRelativePathToIsFileDic = new Dictionary<string, bool>();

        foreach(var path in aEntry)
        {
            var fileName = Path.GetFileName(path);
            if(exludeFileNameList.Contains(fileName))
            {
                continue;
            }
            
            var isFile = File.Exists(path);
            var relativePath = path.Replace(aDirPath, "");
            aRelativePathToIsFileDic[relativePath] = isFile;
        }

        foreach (var path in bEntry)
        {
            var fileName = Path.GetFileName(path);
            if (exludeFileNameList.Contains(fileName))
            {
                continue;
            }

            var isFile = File.Exists(path);
            var relativePath = path.Replace(bDirPath, "");
            bRelativePathToIsFileDic[relativePath] = isFile;
        }

        // 比较项目数量是否相等
        if(aRelativePathToIsFileDic.Count != bRelativePathToIsFileDic.Count)
        {
            return false;
        }

        // 比较项目名称
        foreach(var kv in aRelativePathToIsFileDic)
        {
            var aKey = kv.Key;
            var aIsFile = kv.Value;
            var isBKeyExists = bRelativePathToIsFileDic.ContainsKey(aKey);
            if(!isBKeyExists)
            {
                return false;
            }

            var isBKeyIsFile = bRelativePathToIsFileDic[aKey];
            if(isBKeyIsFile != aIsFile)
            {
                return false;
            }
        }

        // 比较文件 hash 是否相等
        foreach(var kv in aRelativePathToIsFileDic)
        {
            var relative = kv.Key;
            var isFile = kv.Value;
            if(isFile)
            {
                var aFullPath = $"{aDirPath}{relative}";
                var bFullPath = $"{bDirPath}{relative}";
                var isSame = IsFileHashSame(aFullPath, bFullPath);
                if(!isSame)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void DeleteFileInDir(string dirPath, List<string> keepFileNameList)
    {
        var fileList = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
        foreach(var file in fileList)
        {
            var name = Path.GetFileName(file);
            if(keepFileNameList.Contains(name))
            {
                continue;
            }
            else
            {
                File.Delete(file);
            }
        }

        // 删除空文件夹
        DeleteEmptyDir(dirPath);

    }

    void DeleteEmptyDir(string dirPath)
    {
        var subDirList = Directory.GetDirectories(dirPath);
        foreach(var subDir in subDirList)
        {
            DeleteEmptyDir(subDir);
        }

        if(IsDirEmpty(dirPath))
        {
            Directory.Delete(dirPath);
        }
    }

    bool IsDirEmpty(string dirPath)
    {
        var entresList = Directory.GetFileSystemEntries(dirPath);
        var count = entresList.Length;
        return count == 0;
    }

    bool IsFileHashSame(string aPath, string bPath)
    {
        var aExists = File.Exists(aPath);
        var bExists = File.Exists(bPath);
        if(!aExists)
        {
            throw new Exception($"file: {aPath} not exsits");
        }
        if (!bExists)
        {
            throw new Exception($"file: {bExists} not exsits");
        }
        using (var hash = HashAlgorithm.Create())
        {
            using (var aSteam = File.OpenRead(aPath))
            {
                using (var bSteam = File.OpenRead(bPath))
                {
                    var aHash = hash.ComputeHash(aSteam);
                    var bHash = hash.ComputeHash(bSteam);
                    var aString = BitConverter.ToString(aHash);
                    var bString = BitConverter.ToString(bHash);
                    if (aString == bString)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


    }
}
