
using CustomLitJson;

/// <summary>
/// 简易版 roleInfo -- 基本信息
/// </summary>
public class RoleBaseInfo
{
    // 用户 id，在某些接口里叫做 uid
    public string _id;
    
    public int sid;

    // 显示用的名称
    public string name;

    // 等级
    public int lv;

    // 上次登录时间
    public long login;
    
    // 头像
    public JsonData icon;
}