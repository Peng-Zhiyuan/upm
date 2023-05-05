using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class MarkupExtension
{
    public static string ToMarkup(this string str, params MarkupAdapter[] markups)
    {
        return Markup.Create(str, markups);
    }

    public static string ToMarkup(this string str, Color color)
    {
        return Markup.Create(str, Markup.Color(color));
    }

    public static string ToMarkup(this object obj, params MarkupAdapter[] markups) {
        return Markup.Create(obj.ToString(), markups);
    }

    public static string ToMarkup(this object obj, Color color) {
        return Markup.Create(obj.ToString(), Markup.Color(color));
    }
}