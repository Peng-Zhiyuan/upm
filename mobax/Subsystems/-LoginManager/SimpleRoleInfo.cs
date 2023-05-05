using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginRoleInfo
{
    public string _id;
    

    public int lv;

    public string name;

    public int sid;

    public int support;

    // 禁言
    public long shutup;

    // 封号
    public int disable;
}

public class CreateRoleResponse
{
    public LoginRoleInfo role;
    //public GameServerCookie cookie;
}
