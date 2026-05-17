using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Generator(typeof(CircleGenerator), "Generate/Shapes/Circle", "Circle")]
    public class CircleGenerator : ImageGenerator
    {
        [SerializeField]
        int grid = 1;
        [SerializeField]
        float scale = 0.5f;
        [SerializeField]
        float hardness = 0.5f;
        [SerializeField]
        float offsetX = 0f;
        [SerializeField]
        float offsetY = 0f;
        [SerializeField]
        Color color = new Color(1f, 1f, 1f, 1f);

        public override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Generators/Circle.compute");
            }
            return shader;
        }
        public override void GenerateImage(InputSlot slot)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("Circle");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetFloat(ShaderPropertyCache.OffsetXID, offsetX);
            shader.SetFloat(ShaderPropertyCache.OffsetYID, offsetY);
            shader.SetFloat(ShaderPropertyCache.HardnessID, hardness);
            shader.SetFloat(ShaderPropertyCache.ScaleID, scale);

            shader.SetInt(ShaderPropertyCache.IntValueID, grid);
            shader.SetVector(ShaderPropertyCache.ColorID, color);

            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.SourceRenderTexture);
            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
