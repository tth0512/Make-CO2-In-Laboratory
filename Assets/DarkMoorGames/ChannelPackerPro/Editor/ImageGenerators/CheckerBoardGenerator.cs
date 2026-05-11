using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Generator(typeof(CheckerBoardGenerator), "Generate/Patterns/Checker Board", "Checker Board")]
    public sealed class CheckerBoardGenerator : ImageGenerator
    {
        [SerializeField]
        int size = 2;

        [SerializeField]
        Color color1 = new Color(1f, 1f, 1f, 1f);
        [SerializeField]
        Color color2 = new Color(0f, 0f, 0f, 1f);

        public override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Generators/CheckerBoard.compute");
            }
            return shader;
        }
        public override void GenerateImage(InputSlot slot)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel("CheckerBoard");
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetInt(ShaderPropertyCache.IntValueID, size);
            shader.SetVector(ShaderPropertyCache.Color1ID, color1);
            shader.SetVector(ShaderPropertyCache.Color2ID, color2);

            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.SourceRenderTexture);
            shader.Dispatch(kernel, threadX, threadY, 1);
        }
    }
}
