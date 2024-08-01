using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private Settings _settings;
        private Material _material;

        private RenderTargetIdentifier _source;

        private RenderTargetHandle _tempTexture;

        public CustomRenderPass(Settings settings, Material material) : base()
        {
            _settings = settings;
            _material = material;
            _tempTexture.Init("_BallLocation"); //todo add
        }

        public void SetSource(RenderTargetIdentifier source)
        {
            _source = source;
        }
        
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("CustomRenderPassFeature");

            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(_tempTexture.id, cameraTextureDesc, FilterMode.Bilinear);

            _material.SetFloat(Radius, _settings.ballRadius);
            _material.SetVector(BallLocation, _settings.ballLoc);
            
            Blit(cmd, _source, _tempTexture.Identifier(), _material, 0);
            Blit(cmd, _tempTexture.Identifier(), _source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_tempTexture.id);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public int materialPassIndex = -1;
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public float ballRadius = 0.5f;
        public Vector2 ballLoc = Vector2.one/2;
    }

    public static Settings settings = new Settings();
    
    CustomRenderPass _rendPass;
    private static readonly int Radius = Shader.PropertyToID("_Radius");
    private static readonly int BallLocation = Shader.PropertyToID("_BallLocation");

    /// <inheritdoc/>
    public override void Create()
    {
        var material = new Material(Shader.Find("Hidden/InverseCircleS"));// todo change on material
        _rendPass = new CustomRenderPass(settings, material);

        // Configures where the render pass should be injected.
        _rendPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _rendPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(_rendPass);
    }
}


