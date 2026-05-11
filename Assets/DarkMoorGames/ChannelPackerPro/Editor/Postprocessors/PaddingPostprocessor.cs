using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Postprocessor(typeof(PaddingPostprocessor), "Padding", "Padding")]
    public sealed class PaddingPostprocessor : ImagePostprocessor
    {
        [SerializeField]
        PadMethod method;
        [SerializeField, Tooltip("Pad until all pixels fill up the image.")]
        bool infinite;
        [SerializeField, Tooltip("Pad by the specified padding count in pixels.")]
        int padding = 4;
        [SerializeField]
        bool clamp;

        readonly int[] bufferData = new int[1] { 1 };
        ComputeBuffer buffer;

        void OnDisable()
        {
            if (buffer != null)
                buffer.Release();
        }
        void OnDestroy()
        {
            if (buffer != null)
                buffer.Release();
        }
        protected override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Postprocessors/Padding.compute");
            }
            return shader;
        }
        public override void PostprocessImage(ImageSlot slot, RenderTexture previous)
        {
            ComputeShader shader = GetComputeShader();

            int paddingKernel = shader.FindKernel(clamp ? "PaddingClamp" : "Padding");
            int alphaFillTestKernel = shader.FindKernel("PaddingComplete");

            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            //Render the display mesh to a texture then pack the red channel to the alpha of our postprocessed texture.
            //This allows the user to pad from a generated alpha of a uv render.
            if (method == PadMethod.Uvs)
            {
                RenderTexture renderedUvsTexure = slot.UvDisplay.RenderMeshUvsToTexture(slot.Image.Width, slot.Image.Height);
                if (renderedUvsTexure != null)
                {
                    UtilityData data = ChannelPackUtility.GetData();
                    int packKernel = data.ChannelPackShader.FindKernel("PackRedToAlpha");
                    data.ChannelPackShader.SetBool(ShaderPropertyCache.InvertID, false);
                    data.ChannelPackShader.SetTexture(packKernel, ShaderPropertyCache.InputID, renderedUvsTexure);
                    data.ChannelPackShader.SetTexture(packKernel, ShaderPropertyCache.OriginalID, previous);
                    data.ChannelPackShader.SetTexture(packKernel, ShaderPropertyCache.OutputID, slot.Image.PostprocessedRenderTexture);
                    data.ChannelPackShader.Dispatch(packKernel, threadX, threadY, 1);
                    ChannelPackUtility.CopyTexture(slot.Image.PostprocessedRenderTexture, previous);
                }
            }

            if (buffer == null)
            {
                buffer = new ComputeBuffer(1, 4);
                bufferData[0] = 1;
                buffer.SetData(bufferData);
            }

            bool AlphaChannelFull()
            {
                shader.SetBuffer(alphaFillTestKernel, ShaderPropertyCache.CompleteBufferID, buffer);
                shader.SetTexture(alphaFillTestKernel, ShaderPropertyCache.OriginalID, previous);
                shader.Dispatch(alphaFillTestKernel, threadX, threadY, 1);

                //GetData() is slow in this case, need to find a better way. AsyncGPUReadback etc.
                buffer.GetData(bufferData, 0, 0, 1);
                if (bufferData[0] == 1)
                    return true;
                else
                {
                    bufferData[0] = 1;
                    buffer.SetData(bufferData);
                    return false;
                }
            }
            void InfinitePadding()
            {
                int maxIterations = Mathf.Max(slot.Image.Width, slot.Image.Height);
                int testCount = 0;
                int testStep = 4;

                for (int i = 0; i < maxIterations; i++)
                {
                    if (testCount == testStep)
                    {
                        if (AlphaChannelFull())
                            break;
                        testCount = 0;
                    }

                    shader.SetTexture(paddingKernel, ShaderPropertyCache.OriginalID, previous);
                    shader.SetTexture(paddingKernel, ShaderPropertyCache.OutputID, slot.Image.PostprocessedRenderTexture);
                    shader.Dispatch(paddingKernel, threadX, threadY, 1);

                    ChannelPackUtility.CopyTexture(slot.Image.PostprocessedRenderTexture, previous);
                    testCount++;
                }
            }
            void Padding()
            {
                int maxIterations = Mathf.Max(slot.Image.Width, slot.Image.Height);
                int padCount = padding > maxIterations ? maxIterations : padding;
                int testCount = 0;
                int testStep = 4;

                for (int i = 0; i < padCount; i++)
                {
                    if (testCount == testStep)
                    {
                        if (AlphaChannelFull())
                            break;
                        testCount = 0;
                    }
                    shader.SetTexture(paddingKernel, ShaderPropertyCache.OriginalID, previous);
                    shader.SetTexture(paddingKernel, ShaderPropertyCache.OutputID, slot.Image.PostprocessedRenderTexture);
                    shader.Dispatch(paddingKernel, threadX, threadY, 1);

                    ChannelPackUtility.CopyTexture(slot.Image.PostprocessedRenderTexture, previous);
                    testCount++;
                }
            }

            if (infinite)
            {
                InfinitePadding();
            }
            else
            {
                Padding();
            }
        }
    }
}
