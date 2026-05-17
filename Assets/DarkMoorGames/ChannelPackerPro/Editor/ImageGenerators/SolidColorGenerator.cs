using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Generator(typeof(SolidColorGenerator), "Generate/Misc/Solid Color", "Solid Color")]
    public sealed class SolidColorGenerator : ImageGenerator
    {
        [SerializeField]
        Color color = new Color(1f, 1f, 1f, 1f);

        public override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Generators/SolidColor.compute");
            }
            return shader;
        }
        public override void GenerateImage(InputSlot slot)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("SolidColor");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetVector(ShaderPropertyCache.ColorID, color);

            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.SourceRenderTexture);
            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}