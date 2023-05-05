namespace DecalSystem {
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor( typeof( Decal ) )]
    public class DecalEditor : Editor {

        private Material[] materials;

        private Decal Target {
            get { return (Decal) target; }
        }


        void OnEnable() {
            materials = GetMaterials();
        }


        public override void OnInspectorGUI() {
            Target.Material = GUIUtils.DrawAssetChooser( "Material", Target.Material, materials );

            if (Target.Material && Target.Material.mainTexture) {
                var sprites = GetSprites( Target.Material.mainTexture );
                Target.Sprite = GUIUtils.DrawAssetChooser( "Sprite", Target.Sprite, sprites );
                Target.Sprite = GUIUtils.DrawSpriteList( Target.Sprite, sprites );
            }


            EditorGUILayout.Separator();
            Target.LayerMask = GUIUtils.LayerMaskField( "Layer Mask", Target.LayerMask );
            Target.MaxAngle = EditorGUILayout.Slider( "Max Angle", Target.MaxAngle, 0, 180 );
            Target.Offset = EditorGUILayout.Slider( "Offset", Target.Offset, 0.005f, 0.05f );


            EditorGUILayout.Separator();
            if (GUILayout.Button( "Build" )) Target.BuildAndSetDirty();


            //EditorGUILayout.Separator();
            //EditorGUILayout.Separator();
            //EditorGUILayout.HelpBox( "Left Ctrl + Left Mouse Button - Put decal on surface", MessageType.Info );


            if (GUI.changed) {
                Target.OnValidate();
                Target.BuildAndSetDirty();
                GUI.changed = false;
            }
        }


        void OnSceneGUI() {
            if (Event.current.control) {
                // disable default behavior for mouse press
                HandleUtility.AddDefaultControl( GUIUtility.GetControlID( FocusType.Passive ) );
            }

            if (Event.current.control && Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                // press ctrl && mouse down to set up decal transform
                var ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
                RaycastHit hit;
                if (Physics.Raycast( ray, out hit, 50 )) SetTransform( Target.transform, hit.point, -hit.normal );
            }
        }



        // Helpers
        private static Material[] GetMaterials() {
            return AssetDatabase.FindAssets( "Decal t:Material" ).Select( AssetDatabase.GUIDToAssetPath ).Select( i => AssetDatabase.LoadAssetAtPath<Material>( i ) ).ToArray();
        }

        private static Sprite[] GetSprites(Texture texture) {
            var path = AssetDatabase.GetAssetPath( texture );
            return AssetDatabase.LoadAllAssetsAtPath( path ).OfType<Sprite>().ToArray();
        }

        private static void SetTransform(Transform transform, Vector3 position, Vector3 normal) {
            transform.position = position;
            transform.forward = normal;
        }


    }
}