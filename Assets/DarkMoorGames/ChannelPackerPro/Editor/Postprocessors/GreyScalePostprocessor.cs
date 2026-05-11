using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Postprocessor(typeof(GreyScalePostprocessor), "Grey Scale", "Grey Scale")]
    public class GreyScalePostprocessor : ImagePostprocessor
    {
        [SerializeField]
        float amount = 1f;

        protected override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Postprocessors/GreyScale.compute");
            }
            return shader;
        }
        public override void PostprocessImage(ImageSlot slot, RenderTexture previous)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("GreyScale");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetFloat(ShaderPropertyCache.ValueID, amount);

            shader.SetTexture(kernel, ShaderPropertyCache.OriginalID, previous);
            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.PostprocessedRenderTexture);

            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
