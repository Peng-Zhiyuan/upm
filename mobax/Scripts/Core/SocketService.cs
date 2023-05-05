using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Text;
using System;

public class SocketService : Service
{
    public override void OnCreate()
    {
        SocketManager.authPath = "login";
        SocketManager.onAuthProcessAsync = OnAuthAsync;
    }

    async Task OnAuthAsync()
    {
        var token = LoginManager.Stuff.session.cookie.val;
        var data = Encoding.Default.GetBytes(token);
        await RemoteMessage.Stuff.CallAsync("login", null, data);
    }
}
