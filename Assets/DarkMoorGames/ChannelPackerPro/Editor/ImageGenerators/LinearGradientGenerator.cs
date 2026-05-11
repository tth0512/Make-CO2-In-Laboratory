using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Generator(typeof(LinearGradientGenerator), "Generate/Gradients/Linear Gradient", "Linear Gradient")]
    public sealed class LinearGradientGenerator : ImageGenerator
    {
        [SerializeField]
        float rotation;
        [SerializeField]
        float hardness;
        [SerializeField]
        Color start = new Color(1f, 1f, 1f, 1f);
        [SerializeField]
        Color end = new Color(0f, 0f, 0f, 1f);

        public override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Generators/Gradient.compute");
            }
            return shader;
        }
        public override void GenerateImage(InputSlot slot)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("LinearGradient");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetVector(ShaderPropertyCache.Color1ID, start);
            shader.SetVector(ShaderPropertyCache.Color2ID, end);
            shader.SetFloat(ShaderPropertyCache.HardnessID, hardness);
            shader.SetFloat(ShaderPropertyCache.RotationID, rotation);

            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.SourceRenderTexture);
            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
