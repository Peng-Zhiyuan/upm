using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Markup 
{
        public static MarkupAdapter Red = Color(new Color(1f, 0f, 0f, 1f));
        public static MarkupAdapter Green = Color(new Color(0f, 1f, 0f, 1f));
        public static MarkupAdapter Blue = Color(new Color(0f, 0f, 1f, 1f));
        public static MarkupAdapter White = Color(new Color(1f, 1f, 1f, 1f));
        public static MarkupAdapter Black = Color(new Color(0f, 0f, 0f, 1f));
        public static MarkupAdapter Yellow = Color(new Color(1f, 0.9215686f, 0.01568628f, 1f));
        public static MarkupAdapter Cyan = Color(new Color(0.0f, 1f, 1f, 1f));
        public static MarkupAdapter Magenta = Color(new Color(1f, 0.0f, 1f, 1f));
        public static MarkupAdapter Gray = Color(new Color(0.5f, 0.5f, 0.5f, 1f));
        public static MarkupAdapter Clear = Color(new Color(0.0f, 0.0f, 0.0f, 0.0f));

        public static MarkupAdapter Color(Color color)
        {
            return new MarkupAdapter("<color=" + ColorToMarkup(color) + ">", "</color>");
        }

        private static string ColorToMarkup(Color color)
        {
            var r = (int) (color.r*255);
            var g = (int) (color.g*255);
            var b = (int) (color.b*255);
            var a = (int) (color.a*255);
            return "#" + r.ToString("x2") + g.ToString("x2") + b.ToString("x2") + a.ToString("x2");
        }
        public static string Create(string str, params MarkupAdapter[] markups) 
        {
            for (var i = 0; i < markups.Length; i++)
            {
                var markup = markups[i];
                str = markup.ToString(str);
            }
            return str;
        }
        public static MarkupAdapter Blod = new MarkupAdapter("<b>", "</b>");

        public static MarkupAdapter Italic = new MarkupAdapter("<i>", "</i>");

        public static MarkupAdapter Size(int size)
        {
            size = size < 1 ? 1 : size;
            return new MarkupAdapter("<size=" + size + ">", "</size>");
        }

        public static MarkupAdapter Material(int index) 
        {
            index = index < 0 ? 0 : index;
            return new MarkupAdapter("<material=" + index + ">", "</material>");
        }

        public static MarkupAdapter Quad(int materialIndex, int size, float x, float y, float width, float height)
        {
            materialIndex = materialIndex < 0 ? 0 : materialIndex;
            return new MarkupAdapter(
                "<quad material=" + materialIndex +
                " size=" + size +
                " x=" + x +
                " y=" + y +
                " width=" + width +
                " height=" + height + ">",
                "</material>");
        }

        public static MarkupAdapter Quad(string name, int size, float x, float y, float width, float height)
        {
            size = size < 0 ? 0 : size;
            return new MarkupAdapter(
                "<quad name=" + name +
                " size=" + size +
                " x=" + x +
                " y=" + y +
                " width=" + width +
                " height=" + height + ">",
                "</quad>");
        }
    }

