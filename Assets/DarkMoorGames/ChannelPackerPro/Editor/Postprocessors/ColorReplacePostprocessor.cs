using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Postprocessor(typeof(ColorReplacePostprocessor), "Color Replace", "Color Replace")]
    public sealed class ColorReplacePostprocessor : ImagePostprocessor
    {
        [SerializeField]
        float min = 0.0f;
        [SerializeField]
        float max = 0.5f;

        [SerializeField]
        Color from = new Color(1f, 1f, 1f, 1f);
        [SerializeField]
        Color to = new Color(0f, 0f, 0f, 1f);

        protected override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Postprocessors/ColorReplace.compute");
            }
            return shader;
        }
        public override void PostprocessImage(ImageSlot slot, RenderTexture previous)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("ColorReplace");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetVector(ShaderPropertyCache.Color1ID, from);
            shader.SetVector(ShaderPropertyCache.Color2ID, to);
            shader.SetFloat(ShaderPropertyCache.MinID, min);
            shader.SetFloat(ShaderPropertyCache.MaxID, max);

            shader.SetTexture(kernel, ShaderPropertyCache.OriginalID, previous);
            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.PostprocessedRenderTexture);

            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
