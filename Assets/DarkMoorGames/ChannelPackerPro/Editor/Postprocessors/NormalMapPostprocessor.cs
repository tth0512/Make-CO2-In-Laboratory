using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [Postprocessor(typeof(NormalMapPostprocessor), "Normal Map", "Normal Map")]
    public sealed class NormalMapPostprocessor : ImagePostprocessor
    {
        [SerializeField]
        float strength = 5f;
        [SerializeField, Tooltip("If enabled, neighbor samples do not wrap around the edges of the texture, disable for seamless textures.")]
        bool clamp;

        protected override ComputeShader GetComputeShader()
        {
            if (shader == null)
            {
                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/Postprocessors/NormalMap.compute");
            }
            return shader;
        }
        public override void PostprocessImage(ImageSlot slot, RenderTexture previous)
        {
            ComputeShader shader = GetComputeShader();

            int kernel = shader.FindKernel(GetKernelName());
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            shader.SetInt(ShaderPropertyCache.ImageWidthID, slot.Image.Width);
            shader.SetInt(ShaderPropertyCache.ImageHeightID, slot.Image.Height);

            shader.SetFloat(ShaderPropertyCache.StrengthID, strength);

            shader.SetTexture(kernel, ShaderPropertyCache.OriginalID, previous);
            shader.SetTexture(kernel, ShaderPropertyCache.OutputID, slot.Image.PostprocessedRenderTexture);

            shader.Dispatch(kernel, threadX, threadY, 1);
        }
        string GetKernelName()
        {
            return clamp ? "NormalMapClamp" : "NormalMap";
        }
    }
}
