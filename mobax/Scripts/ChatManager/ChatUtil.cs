using GPC.SDK.Chat;
using GPC.SDK.Chat.VO;

using CustomLitJson;
using System.Collections.Generic;
using System.Threading.Tasks;
public static class ChatUtil
{
    /// <summary>
    /// 如果角色数据库有信息，则从角色数据库查询，否则从消息的 meta 数据中获取
    /// </summary>
    /// <param name="chatInfo"></param>
    /// <returns></returns>
    public static RoleInfo AutoGetRoleInfo(ChatInfo chatInfo)
    {
        if (chatInfo == null)
        {
            return null;
        }
        var uid = chatInfo.SenderId;
        var roleInfo = Database.Stuff.roleDatabase.Find(uid);
        if (roleInfo == null)
        {
            var ret = GetRoleInfoFromMeta(chatInfo);
            return ret;
        }
        else
        {
            return roleInfo;
        }
    }

    static Dictionary<string, RoleInfo> chatUserIdToRoleInfoDic = new Dictionary<string, RoleInfo>();
    public static RoleInfo GetRoleInfoFromMeta(ChatInfo info)
    {
        var sender = info.rawMessage.Sender;
        // 自己发送的消息没有 sender，
        // 因此，如果发现没有 sender 的消息，都认为是自己发的
        if (sender == null)
        {
            //sender = ChatManager.Stuff.userInfo;
            return Database.Stuff.roleDatabase.Me;
        }
        var chatUserId = sender.ID;
        if(chatUserIdToRoleInfoDic.ContainsKey(chatUserId))
        {
            return chatUserIdToRoleInfoDic[chatUserId];
        }
        else
        {
            var meta = sender.MetaData;
            if(string.IsNullOrEmpty(meta))
            {
                // 当从历史数据读取消息时
                // 自己发送的消息会有 UserInfo，但是却没有 MetaData
                // 所以，如果发现没有 MetaData 的都认为是自己
                return Database.Stuff.roleDatabase.Me;
            }
            else
            {
                var roleInfo = JsonMapper.Instance.ToObject<RoleInfo>(meta);
                chatUserIdToRoleInfoDic[chatUserId] = roleInfo;
                return roleInfo;
            }

        }
    }

    public static List<ChatInfo> CreateGameChatInfoList(ChatMessage[] chatMessage, string exludeId)
    {
        var ret = new List<ChatInfo>();
        foreach (var one in chatMessage)
        {
            if(one.ID == exludeId)
            {
                continue;
            }
            var info = new ChatInfo(one);
            ret.Add(info);
        }
        return ret;
    }

    public static Task<GameChatConversation> ShowPageToAddFriendConvAsync()
    {
        var tcs = new TaskCompletionSource<GameChatConversation>();
        UIEngine.Stuff.ForwardOrBackTo<ChatSelectFriendPage>();
        UIEngine.Stuff.HookBack(nameof(ChatSelectFriendPage), () =>
        {
            var selectedUid = ChatSelectFriendPage.lastSelectedUid;
            if (string.IsNullOrEmpty(selectedUid))
            {
                tcs.SetResult(null);
                return;
            }
            var conversation = GameConversationManager.Stuff.GetOrCreatePrivateConversation(selectedUid);
            tcs.SetResult(conversation);
        });
        return tcs.Task;
    }
}