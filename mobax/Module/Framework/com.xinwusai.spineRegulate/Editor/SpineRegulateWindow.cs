using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SpineRegulate
{
    public class SpineRegulateWindow : OdinEditorWindow
    {
        [MenuItem("Tools/新屋赛/Spine/调整工具-模板数据")]
        private static void OpenWindow()
        {
            var curWindow = GetWindow<SpineRegulateWindow>();
            // curWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(1500, 800);
            curWindow.titleContent = new GUIContent
            (
                "Spine调整工具-模板数据(UI)",
                LoadUtility.LoadTitleImage()
            );
            curWindow.Focus();

            // open window
            EditorSceneManager.OpenScene(SpineRegulateDefined.SPINE_REGULATE_PREVIEW_SCENE, OpenSceneMode.Single);

            // reset window
            GameObject sceneRoot = GameObject.Find("UIRoot");
            SpineRegulateUtil.ClearAllChildren(sceneRoot);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.SearchUIPages();
        }

        private bool _isPlay = false;

        private void Update()
        {
            if (!Application.isPlaying)
            {
                this._isPlay = false;
                return;
            }

            // 获取到Spine的节点 然后设置一下Spine的动画
            var tf = this._uiPage.GetComponentInChildren<UISpineUnit>();
            var spine = tf.GetComponentInChildren<SkeletonGraphic>();
            if (spine == null) return;
            if (!this._isPlay)
            {
                this._isPlay = true;
                this.SetSkeleton(spine, "idle", true);
            }
        }

        void ClearUIRoot()
        {
            this._isPlay = false;

            var uiRoot = GameObject.Find("UIRoot");
            SpineRegulateUtil.ClearAllChildren(uiRoot);
        }

        #region ---序列化Window界面---

        [Title("选择生成模式", "指定数据:每个界面每个角色单独拥有一套spine配置,模板数据:N界面公用某套spine配置")]
        [HideLabel]
        [OnValueChanged("OnTypeChanged")]
        public ESpineRegulateType type;

        [Title("全身像 & 半身像")] [HideLabel] [ShowIf("IsTemplateModel")] [OnValueChanged("OnReload")]
        public ESpineTemplateType templateType;

        [Title("选择指定修改界面", "所有包含组件:UISpineUnit的预制体均可参与设")]
        [HideLabel]
        [ValueDropdown("_uiPageList")]
        [OnValueChanged("OnPageChanged")]
        [ShowIf("IsAppointModel")]
        public string pageName = "";

        [Title("选择spine的文件名", "如果资源名修改，那么需要重新制作配套数据")]
        [HideLabel]
        [OnValueChanged("OnSpineChanged")]
        [ShowIf("IsPageExist")]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/Arts/Spine", Extensions = "asset")]
        public string spineName = "";

        [TitleGroup("开启遮罩配置", "目前遮罩统一为边缘虚化版本,一般推荐只改虚化偏移")] [HideLabel] [ShowIf("CanRegulateMask")]
        public bool openMask = false;

        [Title("遮罩-->宽度:")]
        [HideLabel]
        [ShowIf("openMask")]
        [OnValueChanged("OnMaskChanged")]
        [ProgressBar(0, 1080, ColorMember = "GetProgressBarColor", DrawValueLabel = false)]
        public float maskWidth = 1080;

        [Title("遮罩-->高度:")]
        [HideLabel]
        [ShowIf("openMask")]
        [OnValueChanged("OnMaskChanged")]
        [ProgressBar(0, 1920, ColorMember = "GetProgressBarColor", DrawValueLabel = false)]
        public float maskHeight = 1920;

        [Title("遮罩-->虚化偏移X:")]
        [HideLabel]
        [ShowIf("openMask")]
        [OnValueChanged("OnSoftnessChanged")]
        [ProgressBar(0, 100, ColorMember = "GetSoftnessProgressBarColor", DrawValueLabel = false)]
        public int softnessX = 0;

        [Title("遮罩-->虚化偏移Y:")]
        [HideLabel]
        [ShowIf("openMask")]
        [OnValueChanged("OnSoftnessChanged")]
        [ProgressBar(0, 100, ColorMember = "GetSoftnessProgressBarColor", DrawValueLabel = false)]
        public int softnessY = 0;

        [TitleGroup("设置UI Transform参数", VisibleIf = "IsPageExist")]
        [Title("UI-->X偏移:")]
        [HideLabel]
        [ShowIf("IsSpineExist")]
        [OnValueChanged("OnOffsetChanged")]
        [ProgressBar(-5000, 5000, ColorMember = "GetProgressBarColor", DrawValueLabel = false)]
        public float offsetX = 0;

        [Title("UI-->Y偏移:")]
        [HideLabel]
        [ShowIf("IsSpineExist")]
        [OnValueChanged("OnOffsetChanged")]
        [ProgressBar(-5000, 5000, ColorMember = "GetProgressBarColor", DrawValueLabel = false)]
        public float offsetY = 0;

        [Title("UI-->缩放:")]
        [HideLabel]
        [ShowIf("IsSpineExist")]
        [OnValueChanged("OnScaleChanged")]
        [ProgressBar(0, 4, ColorMember = "GetScaleProgressBarColor", DrawValueLabel = false)]
        public float scale = 1;

        [TitleGroup("保存 & 重置", "保存:数据存储路径->Assets/res/$Data/SpineUITransData,重置:所有设置恢复", VisibleIf = "IsSpineExist")]
        [HorizontalGroup("保存 & 重置/ButtonGroup")]
        [Button("重置", ButtonSizes.Large), GUIColor(1, 0, 0)]
        private void OnReset()
        {
            this.pageName = "";
            this.spineName = "";
            this.offsetX = 0;
            this.offsetY = 0;
            this.scale = 1;
            this.openMask = false;
            this.maskWidth = 1080;
            this.maskHeight = 1920;
            this.softnessX = 0;
            this.softnessY = 0;
            this.type = ESpineRegulateType.None;
            this.templateType = ESpineTemplateType.None;
            this.ClearUIRoot();
        }

        [HorizontalGroup("保存 & 重置/ButtonGroup")]
        [Button("保存", ButtonSizes.Large), GUIColor(0, 1, 0)]
        private void OnSave()
        {
            if (this.type == ESpineRegulateType.Appoint)
            {
                this.OnSaveByAppointType();
            }
            else if (this.type == ESpineRegulateType.Template)
            {
                this.OnSaveByTemplateType();
            }
        }

        [UsedImplicitly]
        private void OnOffsetChanged()
        {
            if (this._uiPage == null) return;
            var tf = this._uiPage.GetComponentInChildren<SkeletonGraphic>();

            tf.rectTransform().localPosition = new Vector3(this.offsetX, this.offsetY, 0);
        }

        [UsedImplicitly]
        private void OnScaleChanged()
        {
            if (this._uiPage == null) return;
            var tf = this._uiPage.GetComponentInChildren<SkeletonGraphic>();

            tf.rectTransform().localScale = new Vector3(this.scale, this.scale, this.scale);
        }

        [UsedImplicitly]
        private void OnMaskChanged()
        {
            if (this._uiPage == null) return;
            var tf = this._uiPage.GetComponentInChildren<UISpineUnit>();
            var mask = tf.GetComponentInChildren<RectMask2D>();
            mask.rectTransform.sizeDelta = new Vector2(this.maskWidth, this.maskHeight);
        }

        [UsedImplicitly]
        private void OnSoftnessChanged()
        {
            if (this._uiPage == null) return;
            var tf = this._uiPage.GetComponentInChildren<UISpineUnit>();
            var mask = tf.GetComponentInChildren<RectMask2D>();

            mask.softness = new Vector2Int(this.softnessX, this.softnessY);
        }

        private Color GetSoftnessProgressBarColor(int value)
        {
            return Color.Lerp(new Color(1, 0.92f, 0.016f), new Color(0.5f, 1, 0), Mathf.Pow(value / 100f, 2));
        }

        private Color GetScaleProgressBarColor(int value)
        {
            return Color.Lerp(new Color(1, 0.92f, 0.016f), new Color(0.5f, 1, 0), Mathf.Pow(value / 1f, 2));
        }

        private Color GetProgressBarColor(int value)
        {
            return Color.Lerp(new Color(1, 0.92f, 0.016f), new Color(0.5f, 1, 0), Mathf.Pow(value / 5000f, 2));
        }

        private GameObject _uiPage;

        [UsedImplicitly]
        private void OnTypeChanged()
        {
            if (this.type == ESpineRegulateType.None)
            {
                this.OnReset();
            }

            if (this.type == ESpineRegulateType.Template)
            {
                var template = AssetDatabase.LoadAssetAtPath<GameObject>(SpineRegulateDefined.SPINE_TEMPLATE_PREFAB);
                var uiRoot = GameObject.Find("UIRoot");
                this._uiPage = SpineRegulateUtil.InstantiatePrefab(uiRoot, template);
            }
        }

        [UsedImplicitly]
        private void OnPageChanged()
        {
            // 先移除所有
            this.ClearUIRoot();

            if (string.IsNullOrEmpty(this.pageName)) return;
            var pageRealName = this.ParsePageName();

            var page = AssetDatabase.LoadAssetAtPath<GameObject>(this.GetAssetPathByPrefabName(pageRealName));
            if (page == null)
            {
                EditorUtility.DisplayDialog
                (
                    "CODE ERROR",
                    $"需要初始化Page-->{pageRealName}，但并未找到指定预制体，代码异常！！！",
                    "Ok"
                );
                return;
            }

            var uiRoot = GameObject.Find("UIRoot");
            this._uiPage = SpineRegulateUtil.InstantiatePrefab(uiRoot, page);
        }

        [UsedImplicitly]
        private void OnSpineChanged()
        {
            if (this._uiPage == null) return;
            var tf = this._uiPage.GetComponentInChildren<SkeletonGraphic>();

            var asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(
                $"{SpineRegulateDefined.SPINE_PREVIEW_PARENT_FOLDER}/{this.spineName}");
            tf.skeletonDataAsset = asset;
            tf.Initialize(true);
            this.OnReload();
        }

        private void OnReload()
        {
            if (this.type == ESpineRegulateType.Appoint)
            {
                this.OnLoadByAppointType();
            }
            else if (this.type == ESpineRegulateType.Template)
            {
                this.OnLoadByTemplateType();
            }
        }

        private void OnChanged()
        {
            this.OnOffsetChanged();
            this.OnScaleChanged();
            this.OnMaskChanged();
            this.OnSoftnessChanged();
        }

        private bool IsPageExist()
        {
            if (this.IsTemplateModel())
            {
                return true;
            }

            if (this.IsAppointModel())
            {
                return !string.IsNullOrEmpty(this.pageName);
            }

            return false;
        }

        private bool CanRegulateMask()
        {
            if (this.IsAppointModel())
            {
                return this.IsSpineExist();
            }

            if (this.IsTemplateModel())
            {
                if (this.templateType == ESpineTemplateType.HalfModel)
                {
                    return this.IsSpineExist();
                }

                return false;
            }

            return false;
        }

        private bool IsSpineExist()
        {
            return !string.IsNullOrEmpty(this.spineName);
        }

        private bool IsTemplateModel()
        {
            return this.type == ESpineRegulateType.Template;
        }

        private bool IsAppointModel()
        {
            return this.type == ESpineRegulateType.Appoint;
        }

        private string ParsePageName()
        {
            var splits = this.pageName.Split(new char[] {'-'});
            return splits.Last().Replace(">>", "");
        }

        private string ParseSpineName(string nameStr)
        {
            var splits = nameStr.Split(new char[] {'/'});
            return splits.Last().Replace(".asset", "");
        }

        private void SetSkeleton(SkeletonGraphic skeletonGraphic, string animationName, bool isLoop)
        {
            skeletonGraphic.startingLoop = isLoop;
            skeletonGraphic.startingAnimation = animationName;
            skeletonGraphic.Initialize(true);
        }

        #endregion

        #region ---找到对应的包含UISpineUnit脚本的预制体---

        private UISpineUnit _uiSpineUnit;
        private List<string> _uiPageList = new List<string>();
        private Dictionary<string, string> _uiPageAssetPathList = new Dictionary<string, string>();

        void SearchUIPages()
        {
            //获取指定路径下面的所有资源文件
            if (!Directory.Exists(SpineRegulateDefined.SPINE_REGULATE_CHECK_FOLDER)) return;
            DirectoryInfo direction = new DirectoryInfo(SpineRegulateDefined.SPINE_REGULATE_CHECK_FOLDER);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            var pageNames = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                if (!file.Name.EndsWith(".prefab") || file.Name.EndsWith(".meta")) continue;

                var assetPath = this.ParseFileFullName2PrefabPath(file.FullName);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (go == null) continue;
                var spineUnit = go.GetComponentInChildren<UISpineUnit>(true);
                if (spineUnit== null) continue;
                if (go.name.Equals("UISpineUnit")) continue;

                pageNames.Add(go.name);
                this._uiPageAssetPathList.Add(go.name, assetPath);
            }

            for (int i = 0; i < pageNames.Count; i++)
            {
                this._uiPageList.Add($"{i + 1}->>{pageNames[i]}");
            }
        }

        // 解析文件名->预制体路径名
        private string ParseFileFullName2PrefabPath(string fullName)
        {
            var splits = fullName.Split(new char[] {'\\'}).ToList();
            var index = splits.FindIndex(val => val.Equals("Assets"));
            var temp = new List<string>();
            for (int i = index; i < splits.Count; i++)
            {
                temp.Add(splits[i]);
            }

            return string.Join("/", temp);
        }

        // 解析预制体路径名->编辑器预览名
        private string ParsePrefabName2EditorPreviewName(string prefabName)
        {
            return "";
        }

        private string GetAssetPathByPrefabName(string prefabName)
        {
            return DictionaryUtil.TryGet(this._uiPageAssetPathList, prefabName, default);
        }

        #endregion

        #region ---数据存储 & 读取相关---

        // 指定模式保存
        private void OnSaveByAppointType()
        {
            var pageRealName = this.ParsePageName();
            var spineRealName = this.ParseSpineName(this.spineName);
            var obj = CreateInstance<SpineRegulateAppointScriptObject>();
            obj.pageName = pageRealName;
            obj.spineName = spineRealName;
            obj.spineFullName = this.spineName;
            obj.offsetX = this.offsetX;
            obj.offsetY = this.offsetY;
            obj.scale = this.scale;
            obj.openMask = this.openMask;
            obj.softness = new Vector2Int(this.softnessX, this.softnessY);
            obj.maskSize = new Vector2(this.maskWidth, this.maskHeight);
            string dest = $"{SpineRegulateDefined.SPINE_REGULATE_DATA_PARENT_FOLDER.TrimEnd('/')}/{pageRealName}";

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
                AssetDatabase.Refresh();
            }

            var fullPath =
                $"{SpineRegulateDefined.SPINE_REGULATE_DATA_PARENT_FOLDER}/{pageRealName}/{pageRealName}_{spineRealName}.asset";
            AssetDatabase.CreateAsset(obj, fullPath);
            var guid = AssetDatabase.GUIDFromAssetPath(fullPath);
            AssetDatabase.SetMainObject(obj, fullPath);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(guid);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            this.OnReset();
        }

        private void OnSaveByTemplateType()
        {
            var spineRealName = this.ParseSpineName(this.spineName);
            var obj = CreateInstance<SpineRegulateTemplateScriptObject>();
            obj.spineName = spineRealName;
            obj.spineFullName = this.spineName;
            obj.offsetX = this.offsetX;
            obj.offsetY = this.offsetY;
            obj.scale = this.scale;
            obj.templateType = this.templateType;
            obj.openMask = this.openMask;
            obj.softness = new Vector2Int(this.softnessX, this.softnessY);
            obj.maskSize = new Vector2(this.maskWidth, this.maskHeight);

            var folderName = "";
            if (this.templateType == ESpineTemplateType.Model)
            {
                folderName = SpineRegulateDefined.SPINE_TEMPLATE_MODEL;
            }
            else if (this.templateType == ESpineTemplateType.HalfModel)
            {
                folderName = SpineRegulateDefined.SPINE_TEMPLATE_HALF_MODEL;
            }
            else if (this.templateType == ESpineTemplateType.FeatureCamera)
            {
                folderName = SpineRegulateDefined.SPINE_TEMPLATE_FEATURE_CAMERA;
            }

            string dest =
                $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER.TrimEnd('/')}/{folderName}";

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
                AssetDatabase.Refresh();
            }

            var fullPath = "";
            if (this.templateType == ESpineTemplateType.Model)
            {
                fullPath = $"{dest}/Model_{spineRealName}.asset";
            }
            else if (this.templateType == ESpineTemplateType.HalfModel)
            {
                fullPath = $"{dest}/HalfModel_{spineRealName}.asset";
            }
            else if (this.templateType == ESpineTemplateType.FeatureCamera)
            {
                fullPath = $"{dest}/FeatureCamera_{spineRealName}.asset";
            }

            AssetDatabase.CreateAsset(obj, fullPath);
            var guid = AssetDatabase.GUIDFromAssetPath(fullPath);
            AssetDatabase.SetMainObject(obj, fullPath);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(guid);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            this.OnReset();
        }

        private void OnLoadByAppointType()
        {
            var pageRealName = this.ParsePageName();
            // 判断是否存在数据
            var spineRealName = this.ParseSpineName(this.spineName);
            var fullPath =
                $"{SpineRegulateDefined.SPINE_REGULATE_DATA_PARENT_FOLDER}/{pageRealName}/{pageRealName}_{spineRealName}.asset";
            var pageData = AssetDatabase.LoadAssetAtPath<SpineRegulateAppointScriptObject>(fullPath);
            if (pageData != null)
            {
                this.offsetX = pageData.offsetX;
                this.offsetY = pageData.offsetY;
                this.scale = pageData.scale;
                this.openMask = pageData.openMask;
                this.maskWidth = pageData.maskSize.x;
                this.maskHeight = pageData.maskSize.y;
                this.softnessY = (int) pageData.softness.y;
                this.OnChanged();
                return;
            }

            this.offsetX = 0;
            this.offsetY = 0;
            this.scale = 1;
            this.openMask = false;
            this.maskWidth = 1080;
            this.maskHeight = 1920;
            this.softnessX = 0;
            this.softnessY = 0;
            this.OnChanged();
        }

        private void OnLoadByTemplateType()
        {
            // 判断是否存在数据
            var spineRealName = this.ParseSpineName(this.spineName);
            var folderName = "";
            var fullPath = "";
            if (this.templateType == ESpineTemplateType.Model)
            {
                folderName = SpineRegulateDefined.SPINE_TEMPLATE_MODEL;
                fullPath =
                    $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER}/{folderName}/Model_{spineRealName}.asset";
            }
            else if (this.templateType == ESpineTemplateType.HalfModel)
            {
                folderName = SpineRegulateDefined.SPINE_TEMPLATE_HALF_MODEL;
                fullPath =
                    $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER}/{folderName}/HalfModel_{spineRealName}.asset";
            }
            else if (this.templateType == ESpineTemplateType.FeatureCamera)
            {
                folderName = SpineRegulateDefined.SPINE_TEMPLATE_FEATURE_CAMERA;
                fullPath =
                    $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER}/{folderName}/FeatureCamera_{spineRealName}.asset";
            }

            var pageData = AssetDatabase.LoadAssetAtPath<SpineRegulateTemplateScriptObject>(fullPath);
            if (pageData != null)
            {
                this.offsetX = pageData.offsetX;
                this.offsetY = pageData.offsetY;
                this.scale = pageData.scale;
                this.openMask = pageData.openMask;
                this.maskWidth = pageData.maskSize.x;
                this.maskHeight = pageData.maskSize.y;
                this.softnessY = (int) pageData.softness.y;
                this.OnChanged();
                return;
            }

            this.offsetX = 0;
            this.offsetY = 0;
            this.scale = 1;
            this.openMask = false;
            this.maskWidth = 1080;
            this.maskHeight = 1920;
            this.softnessX = 0;
            this.softnessY = 0;
            this.OnChanged();
        }

        #endregion
    }
}