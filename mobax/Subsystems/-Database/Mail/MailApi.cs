using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using CustomLitJson;

public class MailApi : MonoBehaviour
{
    /**
    * 
    * @param time 开始搜索的时间戳
    * @param fieldsDescriptor 可选，需要的字段，使用逗号分隔符，eg: "id,name"
    */
    public static async Task<NetMsg<MailInfo[]>> RequestAllAsync(long time, bool isBlock = true)
    {
        var param = new JsonData();
        param["update"] = time;
        var msg = await NetworkManager.Stuff.CallAndGetMsgAsync<MailInfo[]>(ServerType.Game, "getter/mail", param, null, isBlock);
        return msg;
    }

    /**
     * 设置已读
     * @param id 邮件id，多个用逗号分隔，为空表示一所有
     * @returns 
     */
    public static async Task ReadAsync(string id)
    {
        var param = new JsonData();
        param["id"] = id;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "mail/read", param);
    }

    /**
    * 删除邮件
    * @param id 邮件id，多个用逗号分隔，为空表示所有
    * @returns 
    */
    public static async Task RemoveAsync(string id)
    {
        var param = new JsonData();
        param["id"] = id;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "mail/remove", param);
    }

    /**
     * 领取邮件
     * @param id 邮件id，多个用逗号分隔，为空表示所有
     * @returns 
     */
    public static async Task SubmitAsync(string id, DisplayType isAutoShowReward)
    {
        var param = new JsonData();
        param["id"] = id;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "mail/submit", param, null, true, isAutoShowReward);
    }

    /**
     * 
     * @param rowId 数据表 id
     * @param title 标题
     * @param attr 附件（[分类，id，数量] 三个一组循环的格式. 分类没什么用. eg: "1,11002,100,9,15001，1"）
     * @param content 文字内容
     * @param time 时间戳
     * @returns 
     */
    public async static Task DebugSend(int rowId, string title, string attr, string content, long time)
    {
        var jd = new JsonData();
        jd["id"] = rowId;
        jd["title"] = title;
        jd["attr"] = attr;
        jd["content"] = content;
        jd["time"] = time;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "debug/mail", jd);
    }
}
