using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Runtime.CompilerServices;

public static class UIEngineExtension
{
    public static async Task<T> ForwardAsync<T>(this UIEngine uiengine, object param = null, [CallerFilePath] string callerFilePath = null) where T : Page
    {
        var name = typeof(T).Name;
        var page = await uiengine.ForwardAsync(name, param, false, callerFilePath);
        var pageT = page as T;
        if (pageT == null)
        {
            var type = typeof(T);
            var typeName = type.Name;
            throw new Exception($"[{nameof(UIEngineExtension)}] can't cast page to type {typeName}");
        }

        return pageT;
    }

    public static async void Forward<T>(this UIEngine uiengine, object param = null, [CallerFilePath] string callerFilePath = null) where T : Page
    {
        await ForwardAsync<T>(uiengine, param, callerFilePath);
    }

    public static async Task<T> BackToAsync<T>(this UIEngine uiengine, object param = null, [CallerFilePath] string callerFilePath = null) where T : Page
    {
        var name = typeof(T).Name;
        var page = await uiengine.BackToAsync(name, param, callerFilePath);
        return (T) page;
    }

    public static async void BackTo<T>(this UIEngine uiengine, object param = null, [CallerFilePath] string callerFilePath = null) where T : Page
    {
        var name = typeof(T).Name;
        await uiengine.BackToAsync(name, param, callerFilePath);
    }


    public static async Task<T> ReplaceAsync<T>(this UIEngine uiengine, object param = null, [CallerFilePath] string callerFilePath = null) where T : Page
    {
        var name = typeof(T).Name;
        var page = await uiengine.ReplaceAsync(name, param, false, callerFilePath);
        return (T) page;
    }

    public static async Task RemoveFromStack<T>(this UIEngine uiengine, [CallerFilePath] string callerFilePath = null)
        where T : Page
    {
        var name = typeof(T).Name;
        await uiengine.RemoveFromStackAsync(name, callerFilePath);
    }

    public static T FindPage<T>(this UIEngine uiengine) where T : Page
    {
        var name = typeof(T).Name;
        return (T) uiengine.FindPage(name);
    }

    public static T FindFloating<T>(this UIEngine uiengine) where T : Floating
    {
        var name = typeof(T).Name;
        return (T) uiengine.FindFloating(name);
    }

    public static async Task<T> ForwardOrBackToAsync<T>(this UIEngine uiengine, object param = null,
        [CallerFilePath] string callerFilePath = null) where T : Page
    {
        var name = typeof(T).Name;
        var ret = await uiengine.ForwardOrBackToAsync(name, param, callerFilePath);
        return (T) ret;
    }

    public static async void ForwardOrBackTo(this UIEngine uiengine, string pageName, object param = null,
        [CallerFilePath] string callerFilePath = null)
    {
        await uiengine.ForwardOrBackToAsync(pageName, param, callerFilePath);
    }

    public static async void ForwardOrBackTo<T>(this UIEngine uiengine, object param = null,
        [CallerFilePath] string callerFilePath = null) where T : Page
    {
        await ForwardOrBackToAsync<T>(uiengine, param, callerFilePath);
    }


    public static async void ShowFloating<T>(this UIEngine uiengine, object param = null,
        UILayer layer = UILayer.UseDefault) where T : Floating
    {
        await ShowFloatingAsync<T>(uiengine, null, layer, param);
    }

    public static async Task<T> ShowFloatingAsync<T>(this UIEngine uiengine, Page contianerPage = null,
        UILayer layer = UILayer.UseDefault, object param = null, string wwise = "") where T : Floating
    {
        var name = typeof(T).Name;
        var floating = await uiengine.ShowFloatingAsync(name, contianerPage, layer, param, wwise);
        return (T) floating;
    }

    public static T ShowFloatingImediatly<T>(this UIEngine uiengine, Page contianerPage = null,
        UILayer layer = UILayer.UseDefault, object param = null, string wwise = "") where T : Floating
    {
        var name = typeof(T).Name;
        var floating = uiengine.ShowFloatingImediatly(name, contianerPage, layer, param, wwise);
        return (T) floating;
    }

    /// <summary>
    /// “移除”一词指是从 UIEngine 系统中移除
    /// 游戏对象会被放入池中，而不是被销毁
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uiengine"></param>
    public static async void RemoveFloating<T>(this UIEngine uiengine) where T : Floating
    {
        var name = typeof(T).Name;
        await uiengine.RemoveFloatingAsync(name);
    }

    public static async Task RemoveFloatingAsync<T>(this UIEngine uiengine) where T : Floating
    {
        var name = typeof(T).Name;
        await uiengine.RemoveFloatingAsync(name);
    }

    public static void RemoveFloatingImediately<T>(this UIEngine uiengine) where T : Floating
    {
        var name = typeof(T).Name;
        uiengine.RemoveFloatingImediately(name);
    }

    public static bool IsTop<T>(this UIEngine uiengine)
    {
        var name = typeof(T).Name;
        return uiengine.IsTop(name);
    }
}