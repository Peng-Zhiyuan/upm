using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ZoomBlurRenderFeature : ScriptableRendererFeature {
    private ZoomBlurPass m_Pass;
    private Material m_Material = null;

    [SerializeField] private Shader m_Shader = null;


    /// <summary>
    /// 创建 Feature 序列化数据
    /// </summary>
    public override void Create() {
        // 创建渲染通道
        if (m_Pass == null) {
            m_Pass = new ZoomBlurPass(RenderPassEvent.BeforeRenderingPostProcessing);
        }
        int hash = this.m_Pass.GetHashCode();
        this.CheckMaterial();
    }
    /// <inheritdoc/>
    protected override void Dispose(bool disposing) {
        CoreUtils.Destroy(m_Shader);
        CoreUtils.Destroy(m_Material);
    }
    private bool CheckMaterial() {
        if (m_Material != null) {
            return true;
        }

        if (m_Shader == null) {
            m_Shader = Shader.Find("LDQ/Post/ZoomBlur");
            if (m_Shader == null) {
                return false;
            }
        }

        m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
        m_Pass.material = m_Material;
        return m_Material != null;
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (!CheckMaterial()) {
            Debug.LogError("ZoomBlurRenderFeature，材质或者Shader异常");
        }
        //数据初始化
        m_Pass.Setup(renderer.cameraColorTarget);
        //通过 renderer.EnqueuePass 将 Pass 添加到 Render 中。
        renderer.EnqueuePass(m_Pass);
    }

    internal class ZoomBlurPass : ScriptableRenderPass {
        static readonly string k_RenderTag = "Render ZoomBlur Effects";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int TempTargetId = Shader.PropertyToID("_TempTargetZoomBlur");
        static readonly int FocusPowerId = Shader.PropertyToID("_FocusPower");
        static readonly int FocusDetailId = Shader.PropertyToID("_FocusDetail");
        static readonly int FocusScreenPositionId = Shader.PropertyToID("_FocusScreenPosition");
        static readonly int outMaskSmooth = Shader.PropertyToID("_outMaskSmooth");
        static readonly int outMaskLength = Shader.PropertyToID("_outMaskLength");
        static readonly int inMaskSmooth = Shader.PropertyToID("_inMaskSmooth");
        static readonly int inMaskLength = Shader.PropertyToID("_inMaskLength");
        static readonly int ReferenceResolutionXId = Shader.PropertyToID("_ReferenceResolutionX");
        // static readonly int OnlyMaskId = Shader.PropertyToID("_isMask");
        static readonly int maskIntensity = Shader.PropertyToID("_maskIntensity");
        // static readonly int onlyMaskOn = Shader.PropertyToID("_ISMASK_ON");
        static readonly bool onlyMask = Shader.IsKeywordEnabled("_ISMASK_ON");

        ZoomBlur zoomBlur;
        // 这个渲染通道的材质
        internal Material material;
        RenderTargetIdentifier currentTarget;
        // float downsample;

        //ZoomBlurPass类的构造函数
        public ZoomBlurPass(RenderPassEvent evt) {
            renderPassEvent = evt;
            // 不可以在通道内构造 Shader 实例
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            int hash = this.GetHashCode();
            //环境准备，是否创建材质
            if (this.material == null) {
                Debug.LogError("材质没有创建成功");
            }

            //后效是否生效
            if (!renderingData.cameraData.postProcessEnabled) {
                return;
            }

            var stack = VolumeManager.instance.stack;
            zoomBlur = stack.GetComponent<ZoomBlur>();

            if (zoomBlur == null) { return; }
            if (!zoomBlur.IsActive()) { return; }

            var cmd = CommandBufferPool.Get(k_RenderTag);

            Render(cmd, ref renderingData);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        //接着写一个接口，将currentTarget传进去
        public bool Setup(in RenderTargetIdentifier currentTarget) {
            this.currentTarget = currentTarget;
            return this.material != null;
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData) {
            if (zoomBlur == null) {
                return;
            }

            int hash = this.GetHashCode();
            ref var cameraData = ref renderingData.cameraData;
            var source = currentTarget;
            int destination = TempTargetId;

            var w = (int)(cameraData.camera.scaledPixelWidth / zoomBlur.downsample.value);
            var h = (int)(cameraData.camera.scaledPixelHeight / zoomBlur.downsample.value);

            this.material.SetFloat(FocusPowerId, zoomBlur.focusPower.value);

            //设置
            this.material.SetFloat(FocusPowerId, zoomBlur.focusPower.value);
            this.material.SetInt(FocusDetailId, zoomBlur.focusDetail.value);
            this.material.SetVector(FocusScreenPositionId, zoomBlur.focusScreenPosition.value);
            this.material.SetInt(ReferenceResolutionXId, zoomBlur.referenceResolutionX.value);
            this.material.SetFloat(outMaskSmooth, zoomBlur.outMaskSmooth.value);
            this.material.SetFloat(outMaskLength, zoomBlur.outMaskLength.value);
            this.material.SetFloat(inMaskSmooth, zoomBlur.inMaskSmooth.value);
            this.material.SetFloat(inMaskLength, zoomBlur.inMaskLength.value);
            // this.material.bool(onlyMask, zoomBlur.onlyMask.value);
            this.material.SetFloat(maskIntensity, zoomBlur.maskIntensity.value);
            if (zoomBlur.onlyMask.value) {
                Shader.EnableKeyword("_ISMASK_ON");
            } else {
                Shader.DisableKeyword("_ISMASK_ON");
            }
            // this.material.Setint(downsample, zoomBlur.downsample.value);

            //shader的第一个Pass
            int shaderPass = 0;

            cmd.SetGlobalTexture(MainTexId, source);
            //在清理Render Target钱，如果存在后处理栈就需要申请一张临时的render texture
            //我们使用camera buffer的CommandBuffer.GetTemporaryRT方法来申请这样一张texture
            //传入着色器属性ID以及与相机像素尺寸想匹配的纹理宽高
            //FliterMode,RenderTextureFormat(可以对应修改)
            cmd.GetTemporaryRT(destination, w, h, 0, FilterMode.Point, RenderTextureFormat.DefaultHDR);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, this.material, shaderPass);
        }
    }
}