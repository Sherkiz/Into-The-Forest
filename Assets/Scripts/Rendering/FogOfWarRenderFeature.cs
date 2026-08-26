using UnityEngine.Experimental.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using ITF.Entity;
using System.Runtime.InteropServices;
using ITF.World;
using Unity.Collections;

namespace ITF.Rendering
{
    public class FogOfWarRenderingFeature : ScriptableRendererFeature
    {
        [SerializeField] private ComputeShader computeShader;
        [SerializeField] private Vector2Int mapResolution = new(520, 36);
        private FogOfWarPass fowPass;
        public static FogOfWarRenderingFeature Instance;

        private class FogOfWarPass : ScriptableRenderPass
        {
            public Vector2Int mapResolution;
            private float cameraSize = 5;
            const float cellSize = 0.64f;
            float PixelRatioWorldToScreen => 2 * cameraSize / Screen.height;
            Vector2 cameraPosition;
            Vector2 lastMapOffset;
            private ComputeShader computeShader;
            private int kernel;
            private GraphicsBuffer unitsBuffer;
            private GraphicsBuffer viewFieldsBuffer;
            private GraphicsBuffer mapDataBuffer;
            private Vector2[] unitsPositions { get => WorkerManager.GetWorkersPositions(); }
            private int unitsCount { get { 
                    if (unitsPositions != null) return unitsPositions.Length;
                    return 0;
                } 
            }

            private RTHandle fowHandle1;
            private RTHandle fowHandle2;
            private Vector2Int textureSize = new Vector2Int(640, 360);

            public RTHandle FogOfWar => Time.frameCount % 2 == 1 ? fowHandle1 : fowHandle2;
            public void Initialize(ComputeShader computeShader, Camera camera)
            {
                this.computeShader = computeShader;
                kernel = computeShader.FindKernel("CSMain");
                if (fowHandle1 == null || fowHandle1.rt.width != textureSize.x || fowHandle1.rt.height != textureSize.y)
                {
                    fowHandle1?.Release();
                    fowHandle2?.Release();

                    RenderTextureDescriptor renderTextureDescriptor = new RenderTextureDescriptor(textureSize.x, textureSize.y, GraphicsFormat.R16_SFloat, 0)
                    {
                        enableRandomWrite = true,
                        msaaSamples = 1,
                        sRGB = false,
                        useMipMap = false
                    };
                    fowHandle1 = RTHandles.Alloc(renderTextureDescriptor, name: "_FOWRT1");
                    fowHandle2 = RTHandles.Alloc(renderTextureDescriptor, name: "_FOWRT2");
                }

                cameraSize = camera.orthographicSize;
                cameraPosition = camera.transform.position;
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

            void UpdateBuffers(out int viewFieldCount)
            {
                ViewField[] viewFields = GetViewFields(unitsPositions);
                viewFieldCount = viewFields.Length;
                viewFieldsBuffer ??= new GraphicsBuffer(GraphicsBuffer.Target.Structured, viewFields.Length, sizeof(float) * 4);
                if(viewFieldsBuffer.count < viewFields.Length)
                {
                    viewFieldsBuffer?.Release();
                    viewFieldsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, viewFields.Length, sizeof(float) * 4);
                }
                viewFieldsBuffer.SetData(viewFields);
            }

            void UpdateMapBuffer()
            {
                int count = mapResolution.x * mapResolution.y;
                uint[] mapData = new uint[count];
                for (int y = 0; y < mapResolution.y; y++)
                {
                    for(int x = 0; x < mapResolution.x; x++)
                    {
                        mapData[x + y * mapResolution.x] = (byte)(WorldManager.Map.IsPassable(new(x, y)) ? 0 : 1);
                    }
                }
                mapDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, 4);
                mapDataBuffer.SetData(mapData);
            }

            private class PassData
            {
                public ComputeShader computeShader;
                public int kernel;
                public BufferHandle mapDataHandle;
                public TextureHandle lastResultHandle;
                public TextureHandle outputHandle;
                public BufferHandle viewFieldsHandle;
                public int viewFieldCount;
                public float pixelRatioWorldToScreen;
                public Vector2 lastMapOffset;
                public Vector2 mapOffset;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UpdateBuffers(out int viewFieldCount);
                if (mapDataBuffer == null) UpdateMapBuffer();

                RTHandle fowHandle, lastFowHandle;
                if(Time.frameCount % 2 == 1)
                {
                    fowHandle = fowHandle1;
                    lastFowHandle = fowHandle2;
                }
                else
                {
                    fowHandle = fowHandle2;
                    lastFowHandle = fowHandle1;
                }
                TextureHandle lastResultHandle = renderGraph.ImportTexture(lastFowHandle);
                TextureHandle textHandle = renderGraph.ImportTexture(fowHandle);
                BufferHandle viewFieldHandle = renderGraph.ImportBuffer(viewFieldsBuffer);
                BufferHandle mapDataHandle = renderGraph.ImportBuffer(mapDataBuffer);

                using IComputeRenderGraphBuilder builder = renderGraph.AddComputePass("FogOfWarPass", out PassData passData);
                passData.computeShader = computeShader;
                passData.kernel = kernel;
                passData.mapDataHandle = mapDataHandle;
                passData.lastResultHandle = lastResultHandle;
                passData.outputHandle = textHandle;
                passData.viewFieldsHandle = viewFieldHandle;
                passData.viewFieldCount = viewFieldCount;
                passData.pixelRatioWorldToScreen = PixelRatioWorldToScreen * 3;
                passData.lastMapOffset = lastMapOffset;
                passData.mapOffset = cameraPosition - new Vector2(Screen.width, Screen.height) / 2 * passData.pixelRatioWorldToScreen / 3;
                lastMapOffset = passData.mapOffset;

                builder.UseTexture(lastResultHandle, AccessFlags.Read);
                builder.UseTexture(textHandle, AccessFlags.Write);
                builder.UseBuffer(viewFieldHandle, AccessFlags.Read);

                builder.SetRenderFunc(
                    (PassData passData, ComputeGraphContext cgctxt) =>
                    {
                        cgctxt.cmd.SetComputeIntParams(passData.computeShader, "MapResolution", new int[] { mapResolution.x, mapResolution.y });
                        cgctxt.cmd.SetComputeIntParams(passData.computeShader, "TotalThreads", new int[] {textureSize.x, textureSize.y });
                        cgctxt.cmd.SetComputeFloatParam(passData.computeShader, "CellSize", cellSize);
                        cgctxt.cmd.SetComputeVectorParam(passData.computeShader, "MapOffset", passData.mapOffset);
                        cgctxt.cmd.SetComputeVectorParam(passData.computeShader, "LastMapOffset", passData.lastMapOffset);
                        cgctxt.cmd.SetComputeFloatParam(passData.computeShader, "PixelRatioWorldToScreen", passData.pixelRatioWorldToScreen);
                        cgctxt.cmd.SetComputeBufferParam(passData.computeShader, passData.kernel, "MapData", passData.mapDataHandle);
                        cgctxt.cmd.SetComputeIntParam(passData.computeShader, "ViewFieldCount", passData.viewFieldCount);
                        cgctxt.cmd.SetComputeBufferParam(passData.computeShader, passData.kernel, "ViewFields", passData.viewFieldsHandle);
                        cgctxt.cmd.SetComputeTextureParam(passData.computeShader, passData.kernel, "LastResult", passData.lastResultHandle);
                        cgctxt.cmd.SetComputeTextureParam(passData.computeShader, passData.kernel, "Result", passData.outputHandle);

                        cgctxt.cmd.DispatchCompute(passData.computeShader, passData.kernel, Mathf.CeilToInt(textureSize.x / 8f), Mathf.CeilToInt(textureSize.y / 8f), 1);
                    });
            }

            public void CleanUp()
            {
                fowHandle1?.Release();
                fowHandle1 = null;
                fowHandle2?.Release();
                fowHandle2 = null;

                unitsBuffer?.Release();
                unitsBuffer = null;
                viewFieldsBuffer?.Release();
                mapDataBuffer?.Release();
            }

            ViewField[] GetViewFields(Vector2[] positions)
            {
                if(positions == null) return new ViewField[0];
                ViewField[] viewFields = new ViewField[positions.Length];
                for(int i = 0; i < positions.Length; i++)
                {
                    viewFields[i] = new ViewField(positions[i], 1f, 2f);
                }
                return viewFields;
            }
        }

        public RTHandle GetFogOfWarTexture() => fowPass?.FogOfWar;
        public override void Create()
        {
            fowPass = new FogOfWarPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRendering,
                mapResolution = mapResolution,
            };
            Instance = this;
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game || renderingData.cameraData.camera != Camera.main) return;
            if (WorldManager.Instance == null || !WorldManager.IsMapBuilt) return;
            if (computeShader == null || WorkerManager.GetWorkersPositions() == null) return;
            fowPass.Initialize(computeShader, renderingData.cameraData.camera);
            renderer.EnqueuePass(fowPass);
        }
        protected override void Dispose(bool disposing)
        {
            fowPass?.CleanUp();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct ViewField
    {
        Vector2 pos;
        float clearRadius;
        float fadeRadius;

        public ViewField(Vector2 pos,  float clearRadius, float fadeRadius)
        {
            this.pos = pos;
            this.clearRadius = clearRadius;
            this.fadeRadius = fadeRadius;
        }
    }
}
