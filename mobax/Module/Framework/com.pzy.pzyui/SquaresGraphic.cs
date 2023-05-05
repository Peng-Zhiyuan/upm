using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    public readonly struct ColoredSquare
    {
        public int X { get; }
        public int Y { get; }
        public int Len { get; }
        public Color32 Color { get; }

        public ColoredSquare(int x, int y, int len, Color32 color)
        {
            X = x;
            Y = y;
            Len = len;
            Color = color;
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public class SquaresGraphic : MaskableGraphic, ICanvasRaycastFilter
    {
        private ColoredSquare[] _squares;
        
        public void AddSquares(params ColoredSquare[] squares)
        {
            _squares = squares;
            SetVerticesDirty();
        }
        
        public bool IsRaycastLocationValid(Vector2 sp, Camera cam)
        {
            if (null == _squares) return false;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, cam, out Vector2 pos);
            // 只要在任意一个square中，就生效
            foreach (var square in _squares)
            {
                if (pos.x > square.X && pos.x < square.X + square.Len
                &&  pos.y > square.Y && pos.y < square.Y + square.Len)
                {
                    return true;
                }
            }

            return false;
        }
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            if (null == _squares || _squares.Length <= 0) return;
            
            vh.Clear(); // 清除Graphic默认提供的正方形
            foreach (var coloredSquare in _squares)
            {
                _AddRect(vh, coloredSquare);
            }
        }

        private void _AddRect(VertexHelper vh, ColoredSquare square)
        {
            var vertices = new UIVertex[4];
            for (var i = 0; i < 4; i++)
            {
                vertices[i].color = square.Color;
            }

            // 正方形
            vertices[0].position = new Vector3(square.X, square.Y);
            vertices[1].position = new Vector3(square.X, square.Y + square.Len);
            vertices[2].position = new Vector3(square.X + square.Len, square.Y + square.Len);
            vertices[3].position = new Vector3(square.X + square.Len, square.Y);
            vh.AddUIVertexQuad(vertices);
        }
    }
}