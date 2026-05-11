using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Postprocessor(typeof(ChannelFactorPostprocessor), "Channel Factor", "Channel Factor")]
    public sealed class ChannelFactorPostprocessor : ImagePostprocessor
    {
        [SerializeField]
        float redFactor = 1f;
        [SerializeField]
        float greenFactor = 1f;
        [SerializeField]
        float blueFactor = 1f;
        [SerializeField]
        float alphaFactor = 1f;

        protected override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Postprocessors/ChannelFactor.compute");
            }
            return shader;
        }
        public override void PostprocessImage(ImageSlot slot, RenderTexture previous)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("ChannelFactor");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetFloat(ShaderPropertyCache.RedFactorID, redFactor);
            shader.SetFloat(ShaderPropertyCache.GreenFactorID, greenFactor);
            shader.SetFloat(ShaderPropertyCache.BlueFactorID, blueFactor);
            shader.SetFloat(ShaderPropertyCache.AlphaFactorID, alphaFactor);

            shader.SetTexture(kernel, ShaderPropertyCache.OriginalID, previous);
            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.PostprocessedRenderTexture);

            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
