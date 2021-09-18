using System.Threading.Tasks;
using System;
using UnityEngine;

public static class UIEngineExtension
{
    public async static void LuaForward(string pageName)
    {
        await UIEngine.Stuff.ForwardAsync(pageName);
    }

    public static async Task<T> ForwardAsync<T> (this UIEngine uiengine, object param = null) where T : Page 
    {
        var name = typeof (T).Name;
        var page = await uiengine.ForwardAsync (name, param);
        var pageT = (T)page;
        if(pageT == null)
        {
            var type = typeof(T);
            var typeName = type.Name;
            throw new Exception($"[{nameof(UIEngineExtension)}] can't cast page to type {typeName}");
        }
        return pageT;
    }

    public static async void Forward<T> (this UIEngine uiengine, object param = null) where T : Page
    {
        await ForwardAsync<T>(uiengine, param);
    }

    public static async Task<T> BackToAsync<T>(this UIEngine uiengine, object param = null) where T : Page
    {
        var name = typeof (T).Name;
        var iPage = await uiengine.BackToAsync(name, param);
        return (T)iPage;
    }

    public static async Task<T> ReplaceAsync<T> (this UIEngine uiengine, object param = null) where T : Page
    {
        var name = typeof (T).Name;
        var page = await uiengine.ReplaceAsync(name, param);
        return (T)page;
    }

    public static async Task RemoveFromStackAsync<T>(this UIEngine uiengine) where T : Page
    {
        var name = typeof (T).Name;
        await uiengine.RemoveFromStackAsync(name);
    }

    public static T FindPage<T> (this UIEngine uiengine) where T : Page
    {
        var name = typeof (T).Name;
        return (T)uiengine.FindPage(name);
    }

    public static T FindFloating<T> (this UIEngine uiengine) where T : Floating
    {
        var name = typeof (T).Name;
        return (T)uiengine.FindFloating(name);
    }

    public static async Task<T> ForwardOrBackToAsync<T> (this UIEngine uiengine, object param = null) where T : Page
    {
        var name = typeof (T).Name;
        var ret = await uiengine.ForwardOrBackToAsync(name, param);
        return (T)ret;
    }

    public static async void ForwardOrBackTo<T> (this UIEngine uiengine, object param = null) where T : Page
    {
        await ForwardOrBackToAsync<T>(uiengine, param);
    }

    public static T ShowFloatingIfNotShown<T>(this UIEngine uiengine, Page contianerPage = null, UILayer layer = UILayer.FloatingLayer) where T : Floating
    {
        var name = typeof(T).Name;
        var floating = uiengine.ShowFloatingIfNotShown(name, contianerPage, layer);
        return (T)floating;
    }

    public static T ShowFloating<T> (this UIEngine uiengine, Page contianerPage = null, UILayer layer = UILayer.FloatingLayer, object param = null) where T : Floating
    {
        var name = typeof (T).Name;
        var floating = uiengine.ShowFloating(name, contianerPage, layer, param);
        return (T)floating;
    }

    public static void RemoveFloating<T> (this UIEngine uiengine) where T : Floating
    {
        var name = typeof (T).Name;
        uiengine.RemoveFloating (name);
    }

    public static bool IsTop<T>(this UIEngine uiengine)
    {
        var name = typeof(T).Name;
        return uiengine.IsTop(name);
    }
}