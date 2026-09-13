using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricCloudsFeature : ScriptableRendererFeature
{
    public enum CloudDebugMode
    {
        Disabled = 0,
        PassExecution_Magenta = 1,
        RayDirections = 2,
        SceneDepth = 3,
        BoundingBoxHits_Green = 4,
        Raw3DNoise = 5,
        AccumulatedCloudAlpha = 6
    }

    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        public Material cloudMaterial;
        public CloudDebugMode debugMode = CloudDebugMode.RayDirections;
    }

    public Settings settings = new Settings();
    private VolumetricCloudsPass pass;

    public override void Create()
    {
        pass = new VolumetricCloudsPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.cloudMaterial == null)
        {
            Debug.LogWarning("VolumetricCloudsFeature: No material assigned, skipping pass.");
            return;
        }

        if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderer.EnqueuePass(pass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
    }

    private class VolumetricCloudsPass : ScriptableRenderPass
    {
        private readonly Settings settings;
        private RTHandle tempHandle;
        private static readonly int DebugModeID = Shader.PropertyToID("_DebugMode");

        public VolumetricCloudsPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempHandle, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_SeeMeAgain_CloudsTemp");
            ConfigureTarget(tempHandle);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.cloudMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("Volumetric Clouds Pass");

            // Set current debug mode on material
            settings.cloudMaterial.SetFloat(DebugModeID, (float)settings.debugMode);

            RTHandle cameraColorHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

            if (cameraColorHandle != null && tempHandle != null)
            {
                Blitter.BlitCameraTexture(cmd, cameraColorHandle, tempHandle, settings.cloudMaterial, 0);
                Blitter.BlitCameraTexture(cmd, tempHandle, cameraColorHandle);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempHandle?.Release();
        }
    }
}