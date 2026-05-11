using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Postprocessor(typeof(ColorToAlphaPostprocessor), "Color To Alpha", "Color To Alpha")]
    public class ColorToAlphaPostprocessor : ImagePostprocessor
    {
        [SerializeField]
        float min = 0f;
        [SerializeField]
        float max = 0.5f;
        [SerializeField]
        Color color = new Color(1f, 1f, 1f, 1f);

        protected override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Postprocessors/ColorToAlpha.compute");
            }
            return shader;
        }
        public override void PostprocessImage(ImageSlot slot, RenderTexture previous)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("ColorToAlpha");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetFloat(ShaderPropertyCache.MinID, min);
            shader.SetFloat(ShaderPropertyCache.MaxID, max);
            shader.SetVector(ShaderPropertyCache.ColorID, color);

            shader.SetTexture(kernel, ShaderPropertyCache.OriginalID, previous);
            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.PostprocessedRenderTexture);

            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
