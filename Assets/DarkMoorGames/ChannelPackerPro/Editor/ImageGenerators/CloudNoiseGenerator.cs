using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Generator(typeof(CloudNoiseGenerator), "Generate/Noise/Clouds", "Clouds")]
    public sealed class CloudNoiseGenerator : ImageGenerator
    {
        [SerializeField]
        float scale = 10f;
        [SerializeField]
        int octaves = 4;
        [SerializeField]
        float offsetX = 0f;
        [SerializeField]
        float offsetY = 0f;

        public override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Generators/Clouds.compute");
            }
            return shader;
        }
        public override void GenerateImage(InputSlot slot)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("Clouds");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetFloat(ShaderPropertyCache.ScaleID, scale);
            shader.SetInt(ShaderPropertyCache.IntValueID, octaves);
            shader.SetFloat(ShaderPropertyCache.OffsetXID, offsetX);
            shader.SetFloat(ShaderPropertyCache.OffsetYID, offsetY);

            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.SourceRenderTexture);
            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
