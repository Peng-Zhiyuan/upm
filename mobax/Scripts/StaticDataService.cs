using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;


public class StaticDataService : Service
{
    public override void OnCreate()
    {
        StaticDataRuntime.requestDataHandler = LoadProtobufStaticData;
        LanguageDataRuntime.requestDataHandler = LoadLanguageData;
        this.AddTableMetadata();
    }

    public void AddTableMetadata()
    {
        StaticDataRuntime.AddMetadata("puzzle", "itype", IType.Puzzle.ToString());
        StaticDataRuntime.AddMetadata("daily", "itype", IType.Daily.ToString());
        StaticDataRuntime.AddMetadata("hero", "itype", IType.Hero.ToString());
        StaticDataRuntime.AddMetadata("itemGroup", "itype", IType.itemGroup.ToString());
        StaticDataRuntime.AddMetadata("itemPacks", "itype", IType.Pack.ToString());
        StaticDataRuntime.AddMetadata("record", "itype", IType.Record.ToString());
        StaticDataRuntime.AddMetadata("avatar", "itype", IType.Avatar.ToString());
        StaticDataRuntime.AddMetadata("tactic", "itype", IType.Tactic.ToString());
        StaticDataRuntime.AddMetadata("talCard", "itype", IType.TalCard.ToString());
        StaticDataRuntime.AddMetadata("itemWish", "itype", IType.Wish.ToString());
        StaticDataRuntime.AddMetadata("viper", "itype", IType.viper.ToString());

        StaticDataRuntime.AddFirstGroupTable("ticket");
        StaticDataRuntime.AddFirstGroupTable("item");
        StaticDataRuntime.AddFirstGroupTable("puzzle");
        StaticDataRuntime.AddFirstGroupTable("daily");
        StaticDataRuntime.AddFirstGroupTable("hero");
        StaticDataRuntime.AddFirstGroupTable("itemGroup");
        StaticDataRuntime.AddFirstGroupTable("itemPacks");
        StaticDataRuntime.AddFirstGroupTable("record");
        StaticDataRuntime.AddFirstGroupTable("avatar");
        StaticDataRuntime.AddFirstGroupTable("tactic");
        StaticDataRuntime.AddFirstGroupTable("talCard");
        StaticDataRuntime.AddFirstGroupTable("itemWish");
        StaticDataRuntime.AddFirstGroupTable("viper");
        StaticDataRuntime.AddFirstGroupTable("itemPacksText");
    }

    /// <summary>
    /// 在 NoRemoteMode 下，强制使用内置数据
    /// </summary>
    /// <param name="isNoRemoteMode">无远端模式</param>
    /// <returns></returns>
    public static async Task<byte[]> LoadProtobufStaticData()
    {
        byte[] fileData = null;

        var connectRemote = DeveloperLocalSettings.ConnectRemote;

        if (connectRemote)
        {
            var remoteConf = Remote.Stuff.RemoteConf;
            var staticDataVersion = remoteConf.staticDataVersion;
            var path = $"StaticData/{staticDataVersion}/separatedBuffer.zip";
            fileData = await Remote.Stuff.LoadAsync<byte[]>(RemoteLocation.SubEnv, path, CacheType.File);

            // 删除旧文件
            var dirPath = Remote.Stuff.ToStoragePath(RemoteLocation.SubEnv, $"StaticData");
            var keepPath = Remote.Stuff.ToStoragePath(RemoteLocation.SubEnv, $"StaticData/{staticDataVersion}");
            FileManager.DeleteAllSubDir(dirPath, keepPath);
        }
        else
        {
            var asset = Resources.Load<TextAsset>("embededStaticData.buffer");
            if (asset == null)
            {
                throw new Exception("[StaticDataUtil] embeded embededStaticData.buffer not found");
            }
            fileData = asset.bytes;
        }

        return fileData;
    }

    /// <summary>
    /// 在 NoRemoteMode 下，强制使用内置数据
    /// </summary>
    /// <param name="isNoRemoteMode">无远端模式</param>
    /// <returns></returns>
    public static async Task<byte[]> LoadLanguageData()
    {
        byte[] fileData = null;

        var connectRemote = DeveloperLocalSettings.ConnectRemote;
        //var language = LanguageManager.Language;
        var language = LanguageUtil.ProcessedLangauge;

        if (connectRemote)
        {
            var remoteConf = Remote.Stuff.RemoteConf;
            var languageDataVersion = remoteConf.GetLanguageDataVersion(language);
            var path = $"LanguageData/{languageDataVersion}/{language}.zip";
            fileData = await Remote.Stuff.LoadAsync<byte[]>(RemoteLocation.SubEnv, path, CacheType.File);

            // 删除旧文件
            var dirPath = Remote.Stuff.ToStoragePath(RemoteLocation.SubEnv, $"LanguageData");
            var keepPath = Remote.Stuff.ToStoragePath(RemoteLocation.SubEnv, $"LanguageData/{languageDataVersion}");
            FileManager.DeleteAllSubDir(dirPath, keepPath);
        }
        else
        {
            var asset = Resources.Load<TextAsset>($"LanguageData/{language}.buffer");
            if (asset == null)
            {
                throw new Exception($"[StaticDataUtil] embeded LanguageData/{language}.buffer not found");
            }
            fileData = asset.bytes;
        }

        return fileData;
    }
}