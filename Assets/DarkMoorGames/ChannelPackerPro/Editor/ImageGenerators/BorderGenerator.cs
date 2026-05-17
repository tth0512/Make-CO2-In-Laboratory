using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Generator(typeof(BorderGenerator), "Generate/Misc/Border", "Border")]
    public sealed class BorderGenerator : ImageGenerator
    {
        [SerializeField]
        Color color1 = new Color(1f, 1f, 1f, 1f);
        [SerializeField]
        Color color2 = new Color(0f, 0f, 0f, 1f);
        [SerializeField, Tooltip("Thickness in pixels")]
        int thickness = 4;

        public override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Generators/Border.compute");
            }
            return shader;
        }
        public override void GenerateImage(InputSlot slot)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("Border");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetVector(ShaderPropertyCache.Color1ID, color1);
            shader.SetVector(ShaderPropertyCache.Color2ID, color2);
            shader.SetInt(ShaderPropertyCache.ThicknessID, thickness);

            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.SourceRenderTexture);
            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
