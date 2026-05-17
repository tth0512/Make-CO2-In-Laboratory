using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Postprocessor(typeof(BlurPostprocessor), "Blur", "Blur")]
    public sealed class BlurPostprocessor : ImagePostprocessor
    {
        [SerializeField]
        int iterations = 1;
        [SerializeField, Tooltip("If enabled, neighbor samples do not wrap around the edges of the texture, disable for seamless textures.")]
        bool clamp;

        protected override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Postprocessors/Blur.compute");
            }
            return shader;
        }
        public override void PostprocessImage(ImageSlot slot, RenderTexture previous)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel(GetKernelName());
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            for (int i = 0; i < iterations; i++)
            {
                shader.SetTexture(kernel, ShaderPropertyCache.OriginalID, previous);
                shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.PostprocessedRenderTexture);
                shader.Dispatch(kernel, threadX, threadY, 1);

                ChannelPackUtility.CopyTexture(slot.Image.PostprocessedRenderTexture, previous);
            }
        }
        string GetKernelName()
        {
            return clamp ? "BlurClamp" : "Blur";
        }
    }
}
