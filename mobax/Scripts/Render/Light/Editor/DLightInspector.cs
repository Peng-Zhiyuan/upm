using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.PlayerLoop;

namespace ScRender.Editor
{
    using Editor = UnityEditor.Editor;
    using Event = UnityEngine.Event;
    [CustomEditor(typeof(DLight)),CanEditMultipleObjects]
    public class DLightInspector : Editor
    {
        public enum QuickCurveType
        {
            LINE,
            BACK,
        }
        
        
        //baseTip
        private string baseTip = "请使用该面板来控制灯光属性,不要直接修改子节点的属性.";
        
        private SerializedProperty priority;
        private DLIGHT_PRIORITY priorityValue;

        private SerializedProperty light;
        private GUIContent lightGoLabel = new GUIContent("灯光对象");
        
        
        private GUIContent staticLabel = new GUIContent("静态光");
        private SerializedProperty isStatic;
        
        
        private GUIContent spriteOnlyLabel = new GUIContent("角色灯");
        private SerializedProperty spriteOnly;
        
        


        private GUIContent staticColorLabel = new GUIContent("颜色");
        private SerializedProperty staticColor;
        private GUIContent staticItensityLabel = new GUIContent("强度");
        private SerializedProperty staticIntensity;
        private GUIContent staticRangeLabel = new GUIContent("范围");
        private SerializedProperty staticRange;
        

        //======================主光
        private bool mainFoldout = false;
        private GUIContent mainLoopLabel = new GUIContent("是否循环");
        private SerializedProperty mainLoop;
        
        //mainColor
        private GUIContent mainAniLabel = new GUIContent("主光源动态");
        private SerializedProperty mainAni;
        private GUIContent mainTimeLabel = new GUIContent("时长");
        private SerializedProperty mainTime;
        
        //instensity
        private GUIContent intensityMainBeginLabel = new GUIContent("起始强度");
        private SerializedProperty intensityMainBegin;
        private GUIContent intensityMainEndLabel = new GUIContent("结束强度");
        private SerializedProperty intensityMainEnd;
        private SerializedProperty mainCurve;
        
        //color
        private GUIContent colorMainBeginLabel = new GUIContent("起始颜色");
        private SerializedProperty colorMainBegin;
        private GUIContent colorMainEndLabel = new GUIContent("结束颜色");
        private SerializedProperty colorMainEnd;
        
        
        //range
        private GUIContent rangeBeginLabel = new GUIContent("起始范围");
        private SerializedProperty rangeBegin;
        private GUIContent rangeEndLabel = new GUIContent("结束范围");
        private SerializedProperty rangeEnd;
        
        
        //======================副光
        private bool subFoldout = false;
        private GUIContent subLoopLabel = new GUIContent("是否循环");
        private SerializedProperty subLoop;
        
        private GUIContent subAniLabel = new GUIContent("副光源动态");
        private SerializedProperty subAni;
        private GUIContent subTimeLabel = new GUIContent("时长");
        private SerializedProperty subTime;
        
        //instensity
        private GUIContent intensitySubBeginLabel = new GUIContent("起始强度");
        private SerializedProperty intensitySubBegin;
        private GUIContent intensitySubEndLabel = new GUIContent("结束强度");
        private SerializedProperty intensitySubEnd;
        private SerializedProperty subCurve;
        
        //color
        private GUIContent colorSubBeginLabel = new GUIContent("起始颜色");
        private SerializedProperty colorSubBegin;
        private GUIContent colorSubEndLabel = new GUIContent("结束颜色");
        private SerializedProperty colorSubEnd;
        
        
        
        
        
        //flash
        private bool falshFoldout = false;
        private GUIContent flashLabel = new GUIContent("闪烁效果");
        private SerializedProperty flash;
        private GUIContent flashTimeLabel = new GUIContent("间歇时长","多长时间闪烁一次");
        private SerializedProperty flashTime;
        private GUIContent flashKeepLabel = new GUIContent("闪烁时长","'闪'时持续多久时间");
        private SerializedProperty flashKeep;
        private GUIContent flashOffsetLabel = new GUIContent("闪烁系数","'闪'时的亮度系数");
        private SerializedProperty flashOffset;

        private DLight owner;

        private bool isMain = false;


        private void OnEnable()
        {
            owner = (DLight) serializedObject.targetObject;
            light = serializedObject.FindProperty("light");
            
            LoadValue();
            
            
            


        }

        void LoadValue()
        {
                        var root = serializedObject.FindProperty("data");
            //owner = serializedObject.targetObject;
            
            priority = root.FindPropertyRelative("priority");
            priorityValue = (DLIGHT_PRIORITY)priority.enumValueIndex;
         

            isMain=root.FindPropertyRelative("isMain").boolValue;

            spriteOnly=root.FindPropertyRelative("spriteOnly");
            isStatic = root.FindPropertyRelative("isStatic");

            
            
            
            //=================main
            mainLoop=root.FindPropertyRelative("mainLoop");
            staticColor = root.FindPropertyRelative("staticColor");
            staticIntensity = root.FindPropertyRelative("staticIntensity");
            staticRange = root.FindPropertyRelative("staticRange");
            
            
            
            //instensity
            mainAni = root.FindPropertyRelative("mainAni");
            mainTime = root.FindPropertyRelative("mainTime");
            intensityMainBegin = root.FindPropertyRelative("intensityMainBegin");
            intensityMainEnd = root.FindPropertyRelative("intensityMainEnd");
            mainCurve=root.FindPropertyRelative("mainCurve");
       
            
            //color
            colorMainBegin = root.FindPropertyRelative("colorMainBegin");
            colorMainEnd = root.FindPropertyRelative("colorMainEnd");
            
            
            
            //range
            rangeBegin = root.FindPropertyRelative("rangeBegin");
            rangeEnd = root.FindPropertyRelative("rangeEnd");
            
            //=================sub
            subLoop=root.FindPropertyRelative("subLoop");
            
            //instensity
            subAni = root.FindPropertyRelative("subAni");
            subTime = root.FindPropertyRelative("subTime");
            intensitySubBegin = root.FindPropertyRelative("intensitySubBegin");
            intensitySubEnd = root.FindPropertyRelative("intensitySubEnd");
            subCurve=root.FindPropertyRelative("subCurve");
       
            
            //color
            colorSubBegin = root.FindPropertyRelative("colorSubBegin");
            colorSubEnd = root.FindPropertyRelative("colorSubEnd");

            
            

            
            
            //flash
            flash = root.FindPropertyRelative("flash");
            flashTime = root.FindPropertyRelative("flashTime");
            flashKeep = root.FindPropertyRelative("flashKeep");
            flashOffset=root.FindPropertyRelative("flashOffset");
        }



        IEnumerator ReviewEffect()
        {
            yield return null;
        }

        void TailTip()
        {
            if (GUILayout.Button("保存"))
            {
                owner.SaveData();
            }
            if (GUILayout.Button("读取"))
            {
                owner.LoadData();
                LoadValue();
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.Space();
            GUI.contentColor = Color.yellow;
            EditorGUILayout.LabelField(baseTip);
            GUI.contentColor = Color.white;
        }
        
        public override void OnInspectorGUI()
        {
            

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(light,lightGoLabel);
            
            
            
            spriteOnly.boolValue=EditorGUILayout.Toggle(spriteOnlyLabel, spriteOnly.boolValue);
            
            
            isStatic.boolValue=EditorGUILayout.Toggle(staticLabel, isStatic.boolValue);
            
            
            if (isStatic.boolValue)
            {
                staticColor.colorValue = EditorGUILayout.ColorField(staticColorLabel, staticColor.colorValue);
                staticIntensity.floatValue=EditorGUILayout.FloatField(staticItensityLabel,staticIntensity.floatValue);
                staticRange.floatValue=EditorGUILayout.FloatField(staticRangeLabel,staticRange.floatValue);
                TailTip();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    owner.ResetStatic();
                }
                return;
            }


      
            
            mainAni.boolValue=EditorGUILayout.Toggle(mainAniLabel, mainAni.boolValue);
            

            if (!isMain)
            {
                var  v= (DLIGHT_PRIORITY)EditorGUILayout.EnumPopup("优先级", priorityValue);
                if (v != priorityValue)
                {
                    priorityValue = v;
                    priority.enumValueIndex = (int) priorityValue;
                }
            }


            
            
            if (mainAni.boolValue)
            {
                GUI.contentColor=Color.white;
            }
            else
            {
                GUI.contentColor=Color.grey;
            }
            mainFoldout = EditorGUILayout.Foldout(mainFoldout, "主光参数");

            if (mainFoldout)
            {
                mainAni.boolValue=EditorGUILayout.Toggle(mainAniLabel, mainAni.boolValue);
                if (mainAni.boolValue)
                {

                    EditorGUILayout.BeginVertical();
                    intensityMainBegin.floatValue= EditorGUILayout.FloatField(intensityMainBeginLabel, intensityMainBegin.floatValue);
                    intensityMainEnd.floatValue= EditorGUILayout.FloatField(intensityMainEndLabel, intensityMainEnd.floatValue);
                
                    colorMainBegin.colorValue= EditorGUILayout.ColorField(colorMainBeginLabel, colorMainBegin.colorValue);
                    colorMainEnd.colorValue= EditorGUILayout.ColorField(colorMainEndLabel, colorMainEnd.colorValue);

                    if (!isMain)
                    {
                        rangeBegin.floatValue= EditorGUILayout.FloatField(rangeBeginLabel, rangeBegin.floatValue);
                        rangeEnd.floatValue= EditorGUILayout.FloatField(rangeEndLabel, rangeEnd.floatValue);
                    }

                
                    mainTime.floatValue= EditorGUILayout.FloatField(mainTimeLabel, mainTime.floatValue);
                    Rect vrect = new Rect()
                    {
                        width = 1f,
                        height = 1f,
                    };
                    mainCurve.animationCurveValue=EditorGUILayout.CurveField("变化曲线",mainCurve.animationCurveValue,Color.green,vrect);
                    mainLoop.boolValue=EditorGUILayout.Toggle(mainLoopLabel, mainLoop.boolValue);
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            if (subAni.boolValue)
            {
                GUI.contentColor=Color.white;
            }
            else
            {
                GUI.contentColor=Color.grey;
            }

            subFoldout = EditorGUILayout.Foldout(subFoldout, "辅光参数");

            if (subFoldout)
            {
                subAni.boolValue = EditorGUILayout.Toggle(subAniLabel, subAni.boolValue);
                if (subAni.boolValue)
                {

                    EditorGUILayout.BeginVertical();
                    intensitySubBegin.floatValue =
                        EditorGUILayout.FloatField(intensitySubBeginLabel, intensitySubBegin.floatValue);
                    intensitySubEnd.floatValue =
                        EditorGUILayout.FloatField(intensitySubEndLabel, intensitySubEnd.floatValue);

                    colorSubBegin.colorValue = EditorGUILayout.ColorField(colorSubBeginLabel, colorSubBegin.colorValue);
                    colorSubEnd.colorValue = EditorGUILayout.ColorField(colorSubEndLabel, colorSubEnd.colorValue);



                    subTime.floatValue = EditorGUILayout.FloatField(subTimeLabel, subTime.floatValue);
                    Rect vrect = new Rect()
                    {
                        width = 1f,
                        height = 1f,
                    };
                    subCurve.animationCurveValue =
                        EditorGUILayout.CurveField("变化曲线", subCurve.animationCurveValue, Color.green, vrect);
                    subLoop.boolValue = EditorGUILayout.Toggle(subLoopLabel, subLoop.boolValue);
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            if (flash.boolValue)
            {
                GUI.contentColor=Color.white;
            }
            else
            {
                GUI.contentColor=Color.grey;
            }

            falshFoldout = EditorGUILayout.Foldout(falshFoldout, "闪烁效果");
            if(falshFoldout)
            {
                flash.boolValue=EditorGUILayout.Toggle(flashLabel, flash.boolValue);
                if (flash.boolValue)
                {
                    flashTime.floatValue= EditorGUILayout.FloatField(flashTimeLabel, flashTime.floatValue);
                    flashKeep.floatValue= EditorGUILayout.FloatField(flashKeepLabel, flashKeep.floatValue);
                    flashOffset.floatValue=EditorGUILayout.FloatField(flashOffsetLabel, flashOffset.floatValue);
                }
            }


            GUI.contentColor=Color.white;
            

            
            GUILayout.Label("效果模板");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("爆炸"))
            {
                mainAni.boolValue = true;
                intensityMainBegin.floatValue = 0f;
                intensityMainEnd.floatValue = 2f;
                mainTime.floatValue = 0.3f;
                colorMainBegin.colorValue=Color.gray;
                colorMainEnd.colorValue=Color.white;
                mainLoop.boolValue = false;
                mainCurve.animationCurveValue = MakeCurve(QuickCurveType.LINE);
                //MakeCurve(mainCurve.animationCurveValue,QuickCurveType.LINE);
                mainFoldout = true;

                subAni.boolValue = false;
            }
            if (GUILayout.Button("光球"))
            {
                mainAni.boolValue = true;
                intensityMainBegin.floatValue = 0.5f;
                intensityMainEnd.floatValue = 0.8f;
                mainTime.floatValue = 5.0f;
                colorMainBegin.colorValue=Color.gray;
                colorMainEnd.colorValue=Color.white;
                mainLoop.boolValue = true;
                mainCurve.animationCurveValue=MakeCurve(QuickCurveType.BACK);
                mainFoldout = true;
                
                subAni.boolValue = false;
                subFoldout = false;
            }
            if (GUILayout.Button("火焰"))
            {
                mainAni.boolValue = true;
                intensityMainBegin.floatValue = 0.5f;
                intensityMainEnd.floatValue = 0.8f;
                mainTime.floatValue = 5.0f;
                colorMainBegin.colorValue=Color.gray;
                colorMainEnd.colorValue=Color.white;
                mainLoop.boolValue = true;
                mainCurve.animationCurveValue=MakeCurve(QuickCurveType.BACK);
                mainFoldout = true;
                
                
                subAni.boolValue = true;
                intensitySubBegin.floatValue = 0.0f;
                intensitySubEnd.floatValue = 0.3f;
                subTime.floatValue = 1.0f;
                colorSubBegin.colorValue=Color.gray;
                colorSubEnd.colorValue=Color.white;
                subLoop.boolValue = true;
                subCurve.animationCurveValue=MakeCurve(QuickCurveType.BACK);
                subFoldout = true;
            }
            GUILayout.EndHorizontal();
           
            
            if (GUILayout.Button("播放效果"))
            {
                owner.Reset();
            }



            
            TailTip();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                owner.ResetStatic();
            }
            
            
        }

        AnimationCurve MakeCurve( QuickCurveType _qtype)
        {
            AnimationCurve _ac = new AnimationCurve();
            Keyframe[] _keys = null;
            switch (_qtype)
            {
                case QuickCurveType.LINE:
                    _keys = new Keyframe[2];
                    _keys[0].time = 0;
                    _keys[0].value = 0;
                    _keys[1].time = 1;
                    _keys[1].value = 1;
                    break;
                case QuickCurveType.BACK:
                    _keys = new Keyframe[3];
                    _keys[0].time = 0;
                    _keys[0].value = 0;
                    _keys[1].time = 0.5f;
                    _keys[1].value = 1;
                    _keys[2].time = 1;
                    _keys[2].value = 0;
                    break;
            }
            _ac.keys = _keys;
            return _ac;
        }
    }
}