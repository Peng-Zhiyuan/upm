using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SpineRegulate
{
    /// <summary>
    /// 这里用来调整spine父节点的位置
    /// </summary>
    public class SpineRegulateParentWindow : OdinEditorWindow
    {
        [MenuItem("Tools/新屋赛/Spine/调整工具-界面位置修正(已废弃,请使用预览工具)")]
        private static void OpenWindow()
        {
            var curWindow = GetWindow<SpineRegulateParentWindow>();
            // curWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(1500, 800);
            curWindow.titleContent = new GUIContent
            (
                "Spine调整工具-界面位置修正(UI)",
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
            this.SearchPreviewUIPages();
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

        private void SetSkeleton(SkeletonGraphic skeletonGraphic, string animationName, bool isLoop)
        {
            skeletonGraphic.startingLoop = isLoop;
            skeletonGraphic.startingAnimation = animationName;
            skeletonGraphic.Initialize(true);
        }

        void ClearUIRoot()
        {
            this._isPlay = false;

            var uiRoot = GameObject.Find("UIRoot");
            SpineRegulateUtil.ClearAllChildren(uiRoot);
        }

        #region ---序列化---

        [Title("选择指定修改界面", "所有包含组件:<color=darkblue>UISpineUnit</color>的预制体均可参与设")]
        [HideLabel]
        [ValueDropdown("_previewUIPageList")]
        [OnValueChanged("OnPreviewPageChanged")]
        public string previewPageName = "";

        [FormerlySerializedAs("templateType")] [Title("全身像 & 半身像")] [HideLabel] [ShowIf("PreviewIsPageExist")] [OnValueChanged("RefreshTemplateDataList")]
        public ESpineTemplateType previewTemplateType;

        [Title("选择预览Spine配置数据")]
        [HideLabel]
        [ValueDropdown("_templateDataList")]
        [OnValueChanged("OnPreviewSpineChanged")]
        [ShowIf("PreviewIsShowSpine")]
        public string previewSpineName = "";

        [FormerlySerializedAs("offsetX")]
        [TitleGroup("设置UI Transform参数", VisibleIf = "PreviewIsPageExist")]
        [Title("UI-->X偏移:")]
        [HideLabel]
        [ShowIf("PreviewIsSpineExist")]
        [OnValueChanged("OnPreviewOffsetChanged")]
        [ProgressBar(-5000, 5000, ColorMember = "PreviewGetProgressBarColor", DrawValueLabel = false)]
        public float previewOffsetX = 0;

        [FormerlySerializedAs("previewoffOffsetY")]
        [FormerlySerializedAs("offsetY")]
        [Title("UI-->Y偏移:")]
        [HideLabel]
        [ShowIf("PreviewIsSpineExist")]
        [OnValueChanged("OnPreviewOffsetChanged")]
        [ProgressBar(-5000, 5000, ColorMember = "PreviewGetProgressBarColor", DrawValueLabel = false)]
        public float previewOffsetY = 0;

        [FormerlySerializedAs("scale")]
        [Title("UI-->缩放:")]
        [HideLabel]
        [ShowIf("PreviewIsSpineExist")]
        [OnValueChanged("OnPreviewScaleChanged")]
        [ProgressBar(0, 4, ColorMember = "PreviewGetScaleProgressBarColor", DrawValueLabel = false)]
        public float previewScale = 1;

        [TitleGroup("保存 & 重置", "保存:数据存储路径->Assets/res/$Data/SpineUITransData,重置:所有设置恢复", VisibleIf = "PreviewIsSpineExist")]
        [HorizontalGroup("保存 & 重置/ButtonGroup")]
        [Button("重置", ButtonSizes.Large), GUIColor(1, 0, 0)]
        private void OnPreviewReset()
        {
            this.previewPageName = "";
            this.previewSpineName = "";
            this.previewOffsetX = 0;
            this.previewOffsetY = 0;
            this.previewScale = 1;
            this.ClearUIRoot();
        }

        [HorizontalGroup("保存 & 重置/ButtonGroup")]
        [Button("保存", ButtonSizes.Large), GUIColor(0, 1, 0)]
        private void OnPreviewSave()
        {
            this.OnPreviewSaveData();
        }

        [UsedImplicitly]
        private void OnPreviewOffsetChanged()
        {
            if (this._uiPage == null) return;
            var tf = this._uiPage.GetComponentInChildren<UISpineUnit>();

            tf.rectTransform().localPosition = new Vector3(this.previewOffsetX, this.previewOffsetY, 0);
        }

        [UsedImplicitly]
        private void OnPreviewScaleChanged()
        {
            if (this._uiPage == null) return;
            var tf = this._uiPage.GetComponentInChildren<UISpineUnit>();

            tf.rectTransform().localScale = new Vector3(this.previewScale, this.previewScale, this.previewScale);
        }

        private Color PreviewGetScaleProgressBarColor(int value)
        {
            return Color.Lerp(new Color(1, 0.92f, 0.016f), new Color(0.5f, 1, 0), Mathf.Pow(value / 1f, 2));
        }

        private Color PreviewGetProgressBarColor(int value)
        {
            return Color.Lerp(new Color(1, 0.92f, 0.016f), new Color(0.5f, 1, 0), Mathf.Pow(value / 5000f, 2));
        }

        private GameObject _uiPage;

        [UsedImplicitly]
        private void OnPreviewPageChanged()
        {
            // 先移除所有
            this.ClearUIRoot();

            if (string.IsNullOrEmpty(this.previewPageName)) return;
            var pageRealName = this.PreviewParsePageName();

            var page = AssetDatabase.LoadAssetAtPath<GameObject>(this.PreviewGetAssetPathByPrefabName(pageRealName));
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

        private void OnPreviewSpineChanged()
        {
            var spineRealName = this.PreviewParseSpineName();
            var folderName = "";
            var fullPath = "";
            if (this.previewTemplateType == ESpineTemplateType.Model)
            {
                folderName = SpineRegulateDefined.SPINE_TEMPLATE_MODEL;
                fullPath =
                    $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER}/{folderName}/{spineRealName}.asset";
            }
            else if (this.previewTemplateType == ESpineTemplateType.HalfModel)
            {
                folderName = SpineRegulateDefined.SPINE_TEMPLATE_HALF_MODEL;
                fullPath =
                    $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER}/{folderName}/{spineRealName}.asset";
            }
            else if (this.previewTemplateType == ESpineTemplateType.FeatureCamera)
            {
                folderName = SpineRegulateDefined.SPINE_TEMPLATE_FEATURE_CAMERA;
                fullPath =
                    $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER}/{folderName}/{spineRealName}.asset";
            }

            var spineData = AssetDatabase.LoadAssetAtPath<SpineRegulateTemplateScriptObject>(fullPath);
            this.PreviewRefreshTemplateTrans(spineData);
            this.OnPreviewLoadData();
        }

        private void PreviewRefreshTemplateTrans(SpineRegulateTemplateScriptObject spineData)
        {
            var spineGraphic = this._uiPage.GetComponentInChildren<SkeletonGraphic>();
            var asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(
                $"{SpineRegulateDefined.SPINE_PREVIEW_PARENT_FOLDER}/{spineData.spineFullName}");
            spineGraphic.skeletonDataAsset = asset;
            spineGraphic.Initialize(true);
            if (spineData == null)
            {
                spineGraphic.rectTransform.localPosition = Vector3.zero;
                spineGraphic.rectTransform.localScale = Vector3.one;
                return;
            }

            spineGraphic.rectTransform.localPosition = new Vector3(spineData.offsetX, spineData.offsetY, 0);
            spineGraphic.rectTransform.localScale = Vector3.one * spineData.scale;

            var mask = this._uiPage.GetComponentInChildren<RectMask2D>();
            if (spineData.templateType == ESpineTemplateType.HalfModel && spineData.openMask)
            {
                mask.rectTransform.sizeDelta = spineData.maskSize;
                mask.softness = spineData.softness;
            }
            else
            {
                mask.rectTransform.sizeDelta = new Vector2(1080, 1920);
                mask.softness = Vector2Int.zero;
            }
        }

        private void OnPreviewChanged()
        {
            this.OnPreviewOffsetChanged();
            this.OnPreviewScaleChanged();
        }

        private string PreviewParsePageName()
        {
            var splits = this.previewPageName.Split(new char[] {'-'});
            return splits.Last().Replace(">>", "");
        }

        private string PreviewParseSpineName()
        {
            var splits = this.previewSpineName.Split(new char[] {'-'});
            return splits.Last().Replace(">>", "");
        }
        
        private bool PreviewIsPageExist()
        {
            return !string.IsNullOrWhiteSpace(this.previewPageName);
        }

        private bool PreviewIsSpineExist()
        {
            return !string.IsNullOrWhiteSpace(this.previewSpineName);
        }

        private bool PreviewIsShowSpine()
        {
            return this.previewTemplateType != ESpineTemplateType.None;
        }

        #endregion

        #region ---找到对应配置全身像或者半身像的资源文件---

        private List<string> _templateDataList = new List<string>();
        private Dictionary<string, string> _templateDisplayDataDic = new Dictionary<string, string>();

        private void RefreshTemplateDataList()
        {
            if (!Directory.Exists(SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER)) return;

            var path = "";
            if (this.previewTemplateType == ESpineTemplateType.Model)
            {
                path =
                    $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER}/{SpineRegulateDefined.SPINE_TEMPLATE_MODEL}";
            }
            else if (this.previewTemplateType == ESpineTemplateType.HalfModel)
            {
                path =
                    $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER}/{SpineRegulateDefined.SPINE_TEMPLATE_HALF_MODEL}";
            }
            else if (this.previewTemplateType == ESpineTemplateType.FeatureCamera)
            {
                path =
                    $"{SpineRegulateDefined.SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER}/{SpineRegulateDefined.SPINE_TEMPLATE_FEATURE_CAMERA}";
            }

            DirectoryInfo direction = new DirectoryInfo(path);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            var pageNames = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                if (!file.Name.EndsWith(".asset") || file.Name.EndsWith(".meta")) continue;
                var assetPath = this.PreviewParseFileFullName2PrefabPath(file.FullName);
                var fileName = file.Name.Replace(".asset", "");
                pageNames.Add(fileName);
                this._templateDisplayDataDic.Add(fileName, assetPath);
            }

            for (int i = 0; i < pageNames.Count; i++)
            {
                this._templateDataList.Add($"{i + 1}->>{pageNames[i]}");
            }
        }

        #endregion

        #region ---找到对应的包含UISpineUnit脚本的预制体---

        private UISpineUnit _previewUISpineUnit;
        private List<string> _previewUIPageList = new List<string>();
        private Dictionary<string, string> _previewUIPageAssetPathList = new Dictionary<string, string>();

        void SearchPreviewUIPages()
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

                var assetPath = this.PreviewParseFileFullName2PrefabPath(file.FullName);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (go == null) continue;
                if (go.GetComponentInChildren<UISpineUnit>() == null) continue;
                if (go.name.Equals("UISpineUnit")) continue;

                pageNames.Add(go.name);
                this._previewUIPageAssetPathList.Add(go.name, assetPath);
            }

            for (int i = 0; i < pageNames.Count; i++)
            {
                this._previewUIPageList.Add($"{i + 1}->>{pageNames[i]}");
            }
        }

        // 解析文件名->预制体路径名
        private string PreviewParseFileFullName2PrefabPath(string fullName)
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
        private string PreviewParsePrefabName2EditorPreviewName(string prefabName)
        {
            return "";
        }

        private string PreviewGetAssetPathByPrefabName(string prefabName)
        {
            return DictionaryUtil.TryGet(this._previewUIPageAssetPathList, prefabName, default);
        }

        #endregion

        #region ---数据保存 & 读取相关---

        private void OnPreviewSaveData()
        {
            var pageRealName = this.PreviewParsePageName();
            var obj = CreateInstance<SpineRegulateUIPosScriptObject>();
            obj.pageName = pageRealName;
            obj.offsetX = this.previewOffsetX;
            obj.offsetY = this.previewOffsetY;
            obj.scale = this.previewScale;
            string dest = $"{SpineRegulateDefined.SPINE_REGULATE_DATA_UIPOS_PARENT_FOLDER.TrimEnd('/')}/{pageRealName}";

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
                AssetDatabase.Refresh();
            }

            var fullPath = $"{dest}/Trans_{pageRealName}.asset";
            AssetDatabase.CreateAsset(obj, fullPath);
            var guid = AssetDatabase.GUIDFromAssetPath(fullPath);
            AssetDatabase.SetMainObject(obj, fullPath);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(guid);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            this.OnPreviewReset();
        }

        private void OnPreviewLoadData()
        {
            var pageRealName = this.PreviewParsePageName();
            // 判断是否存在数据
            var fullPath =
                $"{SpineRegulateDefined.SPINE_REGULATE_DATA_UIPOS_PARENT_FOLDER}/{pageRealName}/Trans_{pageRealName}.asset";
            var pageData = AssetDatabase.LoadAssetAtPath<SpineRegulateUIPosScriptObject>(fullPath);
            if (pageData != null)
            {
                this.previewOffsetX = pageData.offsetX;
                this.previewOffsetY = pageData.offsetY;
                this.previewScale = pageData.scale;
                this.OnPreviewChanged();
                return;
            }

            this.previewOffsetX = 0;
            this.previewOffsetY = 0;
            this.previewScale = 1;
            this.OnPreviewChanged();
        }

        #endregion

      
    }
}