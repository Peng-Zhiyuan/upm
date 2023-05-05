namespace DecalSystem {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    static class GUIUtils {


        public static LayerMask LayerMaskField(string label, LayerMask mask) {
            var names = Enumerable.Range( 0, 32 ).Select( i => LayerMask.LayerToName( i ) )/*.Where( i => !string.IsNullOrEmpty( i ) )*/.ToArray(); // TODO: fix bug
            return EditorGUILayout.MaskField( label, mask.value, names );
        }

        public static T DrawAssetChooser<T>(string label, T item, T[] items) where T : Object {
            var index = ArrayUtility.IndexOf( items, item );
            var names = items.Select( i => i.name ).ToArray();

            using (new GUILayout.HorizontalScope()) {
                EditorGUILayout.PrefixLabel( label );
                item = (T) EditorGUILayout.ObjectField( item, typeof( T ), false );
                GUILayout.Space( 5 );
                index = EditorGUILayout.Popup( index, names );
            }

            return items.ElementAtOrDefault( index );
        }

        public static Sprite DrawSpriteList(Sprite sprite, Sprite[] list) {
            foreach (var item in DrawGrid( list )) {
                var selected = DrawSprite( item.Value, item.Key, item.Key == sprite );
                if (selected) sprite = item.Key;
            }
            return sprite;
        }

        private static bool DrawSprite(Rect rect, Sprite sprite, bool isSelected) {
            var texture = sprite.texture;
            var uvRect = ToRect01( sprite.rect, texture );

            if (isSelected) EditorGUI.DrawRect( rect, Color.blue );
            GUI.DrawTextureWithTexCoords( rect, texture, uvRect );
            return GUI.Button( rect, GUIContent.none, GUI.skin.label );
        }



        // Helpers
        private static Rect ToRect01(Rect rect, Texture2D texture) {
            rect.x /= texture.width;
            rect.y /= texture.height;
            rect.width /= texture.width;
            rect.height /= texture.height;
            return rect;
        }

        private static IEnumerable<KeyValuePair<T, Rect>> DrawGrid<T>(T[] list) {
            var xCount = 5;
            var yCount = Mathf.CeilToInt( (float) list.Length / xCount );
            var i = 0;
            foreach (var rect in DrawGrid( xCount, yCount )) {
                if (i < list.Length) yield return new KeyValuePair<T, Rect>( list[ i ], rect );
                i++;
            }
        }

        private static IEnumerable<Rect> DrawGrid(int xCount, int yCount) {
            var id = GUIUtility.GetControlID( "Grid".GetHashCode(), FocusType.Keyboard );

            using (new GUILayout.VerticalScope( GUI.skin.box )) {
                for (var y = 0; y < yCount; y++) {

                    using (new GUILayout.HorizontalScope()) {
                        for (var x = 0; x < xCount; x++) {
                            var rect = GUILayoutUtility.GetAspectRect( 1 );

                            if (Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition )) {
                                GUIUtility.hotControl = GUIUtility.keyboardControl = id;
                            }

                            yield return rect;
                        }
                    }

                }
            }
        }


    }
}