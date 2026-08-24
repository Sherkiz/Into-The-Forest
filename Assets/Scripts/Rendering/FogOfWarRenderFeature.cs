using UnityEngine.Experimental.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using ITF.Entity;

namespace ITF.Rendering
{
    public class FogOfWarRenderingFeature : ScriptableRendererFeature
    {
        [SerializeField] private ComputeShader computeShader;
        private FogOfWarPass fowPass;
        public static FogOfWarRenderingFeature Instance;
        private class FogOfWarPass : ScriptableRenderPass
        {
            private ComputeShader computeShader;
            private int kernel;
            private GraphicsBuffer unitsBuffer;
            private Vector2[] unitsPositions { get => WorkerManager.GetWorkersPositions(); }
            private int unitsCount { get { 
                    if (unitsPositions != null) return unitsPositions.Length;
                    return 0;
                } 
            }

            private RTHandle fowHandle;
            private Vector2Int textureSize = new Vector2Int(640, 360);

            public RTHandle FogOfWar => fowHandle;
            public void Initialize(ComputeShader computeShader)
            {
                this.computeShader = computeShader;
                kernel = computeShader.FindKernel("CSMain");
                if (fowHandle == null || fowHandle.rt.width != textureSize.x || fowHandle.rt.height != textureSize.y)
                {
                    fowHandle?.Release();

                    RenderTextureDescriptor renderTextureDescriptor = new RenderTextureDescriptor(textureSize.x, textureSize.y, GraphicsFormat.R32_SFloat, 0)
                    {
                        enableRandomWrite = true,
                        msaaSamples = 1,
                        sRGB = false,
                        useMipMap = false
                    };
                    fowHandle = RTHandles.Alloc(renderTextureDescriptor, name: "_FOWRT");
                }
            }
            public void UpdateUnitsPositions()
            {
                if (unitsCount == 0) return;
                unitsBuffer ??= new GraphicsBuffer(GraphicsBuffer.Target.Structured, unitsCount, sizeof(float) * 2);
                if (unitsBuffer.count != unitsCount)
                {
                    unitsBuffer?.Release();
                    unitsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, unitsCount, sizeof(float) * 2);
                }
                unitsBuffer.SetData(unitsPositions);
            }
            private class PassData
            {
                public ComputeShader computeShader;
                public int kernel;
                public TextureHandle outputHandle;
                public BufferHandle unitsHandle;
                public Vector2[] unitsPositions;
                public int unitsCount;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UpdateUnitsPositions();
                TextureHandle textHandle = renderGraph.ImportTexture(fowHandle);
                BufferHandle unitsHandle = renderGraph.ImportBuffer(unitsBuffer);

                using IComputeRenderGraphBuilder builder = renderGraph.AddComputePass("FogOfWarPass", out PassData passData);
                passData.computeShader = computeShader;
                passData.kernel = kernel;
                passData.outputHandle = textHandle;
                passData.unitsHandle = unitsHandle;
                passData.unitsPositions = unitsPositions;
                passData.unitsCount = unitsCount;

                builder.UseTexture(textHandle, AccessFlags.Write);
                builder.UseBuffer(unitsHandle, AccessFlags.Read);

                builder.SetRenderFunc(
                    (PassData passData, ComputeGraphContext cgctxt) =>
                    {
                        cgctxt.cmd.SetComputeIntParam(passData.computeShader, "unitsCount", passData.unitsCount);
                        cgctxt.cmd.SetComputeBufferParam(passData.computeShader, passData.kernel, "unitsPositions", passData.unitsHandle);
                        cgctxt.cmd.SetComputeTextureParam(passData.computeShader, passData.kernel, "fowTexture", passData.outputHandle);

                        cgctxt.cmd.DispatchCompute(passData.computeShader, passData.kernel, Mathf.CeilToInt(textureSize.x / 8f), Mathf.CeilToInt(textureSize.y / 8f), 1);
                    });
            }

            public void CleanUp()
            {
                fowHandle?.Release();
                fowHandle = null;

                unitsBuffer?.Release();
                unitsBuffer = null;
            }
        }
        public RTHandle GetFogOfWarTexture() => fowPass?.FogOfWar;
        public override void Create()
        {
            fowPass = new FogOfWarPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRendering,
            };
            Instance = this;
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (computeShader == null || WorkerManager.GetWorkersPositions() == null) return;
            fowPass.Initialize(computeShader);
            renderer.EnqueuePass(fowPass);
        }
        protected override void Dispose(bool disposing)
        {
            fowPass?.CleanUp();
        }
    }
}
