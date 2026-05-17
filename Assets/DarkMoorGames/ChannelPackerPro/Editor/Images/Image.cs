using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    public class Image : ScriptableObject
    {
        public RenderTexture PreviewRenderTexture { get; private set; }
        public RenderTexture SourceRenderTexture { get; private set; }
        public RenderTexture PostprocessedRenderTexture { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public static Image CreateImage(Texture2D sourceTexture)
        {
            Image image = CreateInstance<Image>();
            image.OnCreate(sourceTexture);
            return image;
        }
        public static Image CreateImage(int width, int height)
        {
            Image image = CreateInstance<Image>();
            image.OnCreate(width, height);
            return image;
        }
        void OnCreate(Texture2D sourceTexture)
        {
            int width = sourceTexture.width;
            int height = sourceTexture.height;

            name = sourceTexture.name;
            hideFlags = HideFlags.HideAndDontSave;

            Width = width;
            Height = height;

            RenderTexture previewRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D
            };
            previewRenderTexture.Create();
            previewRenderTexture.DiscardContents(false, true);

            RenderTexture sourceRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D
            };
            sourceRenderTexture.Create();
            sourceRenderTexture.DiscardContents(false, true);

            RenderTexture postprocessedRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D
            };
            postprocessedRenderTexture.Create();
            postprocessedRenderTexture.DiscardContents(false, true);

            SourceRenderTexture = sourceRenderTexture;
            PreviewRenderTexture = previewRenderTexture;
            PostprocessedRenderTexture = postprocessedRenderTexture;

            ChannelPackUtility.CopyTexture(sourceTexture, sourceRenderTexture, true);
        }
        void OnCreate(int width, int height)
        {
            hideFlags = HideFlags.HideAndDontSave;
            Width = width;
            Height = height;

            RenderTexture previewRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D
            };
            previewRenderTexture.Create();
            previewRenderTexture.DiscardContents(false, true);

            RenderTexture sourceRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D
            };
            sourceRenderTexture.Create();
            sourceRenderTexture.DiscardContents(false, true);

            RenderTexture postprocessedRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D
            };
            postprocessedRenderTexture.Create();
            postprocessedRenderTexture.DiscardContents(false, true);

            SourceRenderTexture = sourceRenderTexture;
            PreviewRenderTexture = previewRenderTexture;
            PostprocessedRenderTexture = postprocessedRenderTexture;
        }
        void OnDestroy()
        {
            if (SourceRenderTexture)
            {
                SourceRenderTexture.Release();
                DestroyImmediate(SourceRenderTexture);
            }
            if (PreviewRenderTexture)
            {
                PreviewRenderTexture.Release();
                DestroyImmediate(PreviewRenderTexture);
            }
            if (PostprocessedRenderTexture)
            {
                PostprocessedRenderTexture.Release();
                DestroyImmediate(PostprocessedRenderTexture);
            }
            SourceRenderTexture = null;
            PreviewRenderTexture = null;
            PostprocessedRenderTexture = null;
        }
    }
}
