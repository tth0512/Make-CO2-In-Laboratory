using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Generator(typeof(VoronoiGenerator), "Generate/Noise/Voronoi", "Voronoi")]
    public sealed class VoronoiGenerator : ImageGenerator
    {
        [SerializeField]
        VoronoiMethod method;
        [SerializeField]
        float scale = 10f;
        [SerializeField]
        float offsetX;
        [SerializeField]
        float offsetY;

        public override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Generators/Voronoi.compute");
            }
            return shader;
        }
        public override void GenerateImage(InputSlot slot)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel(method == VoronoiMethod.Euclidien ? "VoronoiEuclidien" : "VoronoiManhattan");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetFloat(ShaderPropertyCache.ScaleID, scale);
            shader.SetFloat(ShaderPropertyCache.OffsetXID, offsetX);
            shader.SetFloat(ShaderPropertyCache.OffsetYID, offsetY);

            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.SourceRenderTexture);
            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
