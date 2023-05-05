using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;

public class RoleDatabase
{
    [ShowInInspector] string _myUID = "";

    [ShowInInspector] Dictionary<string, RoleInfo> _uidToRoleInfoDic = new Dictionary<string, RoleInfo>();

    public RoleInfo Me
    {
        get
        {
            if (this._myUID == null)
            {
                throw new Exception("[RoleDatabase] self role data not set yet, forget to call FetchMeAsync?");
            }

            var roleInfo = this.Find(this._myUID);
            return roleInfo;
        }
    }

    /// <summary>
    /// 客户端创建的GM账号
    /// </summary>
    public RoleInfo GM
    {
        get
        {
            return new RoleInfo()
            {
                _id = "z34fb12c",
                sid = 1,
                guid = "",
                name = LocalizationManager.Stuff.GetText("m3_GMName"),
                logon = 0,
                exp = 0,
                lv = 1,
                show = 0,
                assist = StaticData.BaseTable.TryGet("defaultAssistGMHeroId"),
                support = StaticData.BaseTable.TryGet("workshopDefaultItemId"),
                describe = LocalizationManager.Stuff.GetText("m3_GMSigned"),
            };
        }
    }

    public async Task FetchMyselfAsync()
    {
        var myself = await RoleApi.FetchMyselfAsync();
        this.Add(myself);
        this.Add(GM);
        this._myUID = myself._id;

        await GuideManagerV2.Stuff.CheckNeedResetGuide();
    }

    public static async Task<RoleInfo> RequestUserAsync(string uid, List<string> keyParam = null,
        bool allowNull = false)
    {
        var id = new List<string>() {uid};
        var keyList = new List<string>() {"name", "lv", "show", "login", "league", "headframe", "power"};
        if (keyParam != null)
        {
            keyList.AddRange(keyParam);
        }

        var infoList = await RoleApi.RequestRoleInfo(id, keyList, false);
        if (infoList == null || infoList.Length == 0)
        {
            if (allowNull)
            {
                return null;
            }

            throw new Exception($"[RoleDatabase] uid: {uid} not found");
        }

        return infoList[0];
    }

    Dictionary<string, Task> uidToRequestionTcsDic = new Dictionary<string, Task>();
    HashSet<string> uidToRoleInfoIsNullSet = new HashSet<string>();

    /// <summary>
    /// 同步 uid 对应的用户信息
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="keyParam"></param>
    /// <param name="force">如果本地有用户信息，依然同步</param>
    /// <returns></returns>
    public async Task SyncUserIfNeedAsync(string uid, List<string> keyParam = null, bool force = false)
    {
        // 如果已确认用户在服务器上不存在
        if (uidToRoleInfoIsNullSet.Contains(uid))
        {
            return;
        }

        // 如果本地已经有信息
        if (!force)
        {
            var roleInfo = this.Find(uid);
            if (roleInfo != null)
            {
                return;
            }
        }

        uidToRequestionTcsDic.TryGetValue(uid, out var preTask);
        if (preTask == null)
        {
            var task = RequestUserAsync(uid, keyParam, true);
            var tcs = new TaskCompletionSource<bool>();
            uidToRequestionTcsDic[uid] = tcs.Task;
            var info = await task;
            if (info != null)
            {
                this.Add(info);
            }
            else
            {
                uidToRoleInfoIsNullSet.Add(uid);
            }

            uidToRequestionTcsDic.Remove(uid);
            tcs.SetResult(true);
        }
        else
        {
            await preTask;
        }
    }


    public async Task<RoleInfo> GetOrRequestIfNeedAsync(string uid, bool allnowNull = false)
    {
        var info = this.Find(uid);
        if (info != null)
        {
            return info;
        }

        await SyncUserIfNeedAsync(uid);
        var postInfo = this.Find(uid);
        if (postInfo == null)
        {
            if (!allnowNull)
            {
                throw new Exception($"[RoleDatabase] no role info found for uid `{uid}`");
            }
        }

        return postInfo;
    }

    void Add(RoleInfo info)
    {
        var uid = info._id;
        this._uidToRoleInfoDic[uid] = info;
    }

    public void ApplyTransaction(DatabaseTransaction transaction)
    {
        DataTreeUpdater.Update(this.Me, transaction.k, transaction.r);
    }

    public RoleInfo Find(string uid)
    {
        if (uid == null)
        {
            return null;
        }

        if (this._uidToRoleInfoDic.TryGetValue(uid, out var roleInfo))
        {
            return roleInfo;
        }

        return null;
    }

    public void Clean()
    {
        this._uidToRoleInfoDic.Clear();
        this._myUID = "";
    }
}