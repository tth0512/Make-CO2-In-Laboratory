using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DarkMoorGames.ChannelPackerPro
{
    public static class ChannelPackUtility
    {
        public const string VERSION = "1.5.3";
        public const int MAX_TEXTURE_SIZE = 8192;

        static UtilityData data;

        static RenderTexture GetCachedRenderTexture(Image image)
        {
            UtilityData data = GetData();

            if (data.CachedRenderTexture == null)
            {
                data.CachedRenderTexture = new RenderTexture(image.Width, image.Height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    antiAliasing = 1,
                    enableRandomWrite = true,
                    useMipMap = false,
                    autoGenerateMips = false,
                    dimension = TextureDimension.Tex2D
                };
                data.CachedRenderTexture.Create();
                data.CachedRenderTexture.DiscardContents(false, true);
                return data.CachedRenderTexture;
            }
            if (data.CachedRenderTexture.width != image.Width || data.CachedRenderTexture.height != image.Height)
            {
                Object.DestroyImmediate(data.CachedRenderTexture);

                data.CachedRenderTexture = new RenderTexture(image.Width, image.Height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    antiAliasing = 1,
                    enableRandomWrite = true,
                    useMipMap = false,
                    autoGenerateMips = false,
                    dimension = TextureDimension.Tex2D
                };
                data.CachedRenderTexture.Create();
                data.CachedRenderTexture.DiscardContents(false, true);
                return data.CachedRenderTexture;
            }
            return data.CachedRenderTexture;
        }
        static bool CanUsePackOptionModifers(PackOption option, bool hasImage)
        {
            if (option.inputImageIndex == 0)
                return true;
            if (option.inputImageIndex != 0 && hasImage)
                return true;
            return false;
        }
        static UtilityData FindUtillityData()
        {
            UtilityData[] all = Resources.FindObjectsOfTypeAll<UtilityData>();
            UtilityData found = null;
            if (all.Length > 0)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    if (!EditorUtility.IsPersistent(all[i]))
                    {
                        found = all[i];
                        break;
                    }
                }
            }
            return found;
        }
        public static UtilityData GetData()
        {
            if (data == null)
            {
                data = FindUtillityData();
                if (data == null)
                    data = UtilityData.Create();
            }
            return data;
        }
        public static void Init()
        {
            GetData();
        }
        public static void Cleanup()
        {
            Object.DestroyImmediate(FindUtillityData());
            data = null;

            EditorUtility.UnloadUnusedAssetsImmediate();
        }
        public static void UpdateImagePreview(ImageSlot slot)
        {
            if (!slot.Image)
                return;

            UtilityData data = GetData();
            int threadX = Mathf.CeilToInt(slot.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(slot.Image.Height / 8f);

            data.ChannelPreviewShader.SetBool(ShaderPropertyCache.LinearSpaceID, PlayerSettings.colorSpace == ColorSpace.Linear);
            data.ChannelPreviewShader.SetTexture(slot.PreviewKernel, ShaderPropertyCache.InputID, slot.GetSourceTexture());
            data.ChannelPreviewShader.SetTexture(slot.PreviewKernel, ShaderPropertyCache.OutputID, slot.Image.PreviewRenderTexture);
            data.ChannelPreviewShader.Dispatch(slot.PreviewKernel, threadX, threadY, 1);
        }
        public static string[] GetObjectNames<T>(List<T> objects) where T : Object
        {
            string[] names = new string[objects.Count];
            for (int i = 0; i < names.Length; i++)
                names[i] = objects[i].name;
            return names;
        }
        public static string[] GetObjectNames<T>(T ingore, List<T> objects) where T : Object
        {
            List<string> nameList = new List<string>(objects.Count);

            int count = objects.Count;
            for (int i = 0; i < count; i++)
            {
                if (ingore != objects[i])
                    nameList.Add(objects[i].name);
            }
            string[] names = nameList.ToArray();
            nameList.Clear();
            return names;
        }
        public static void PackInputToOutput(PackOption option, OutputSlot output, InputSlot input)
        {
            UtilityData data = GetData();

            bool hasInputImage = input.Image;

            RenderTexture tmp = GetCachedRenderTexture(output.Image);

            CopyTexture(output.Image.SourceRenderTexture, tmp);

            int threadX = Mathf.CeilToInt(output.Image.Width / 8f);
            int threadY = Mathf.CeilToInt(output.Image.Height / 8f);

            int kernel = data.ChannelPackShader.FindKernel(option.GetPackKernel(hasInputImage));
            bool useModifer = CanUsePackOptionModifers(option, hasInputImage);

            data.ChannelPackShader.SetFloat(ShaderPropertyCache.ValueID, useModifer ? option.value : 0f);
            data.ChannelPackShader.SetBool(ShaderPropertyCache.InvertID, useModifer && option.invertChannel);

            if (hasInputImage)
                data.ChannelPackShader.SetTexture(kernel, ShaderPropertyCache.InputID, input.GetSourceTexture());

            data.ChannelPackShader.SetTexture(kernel, ShaderPropertyCache.OriginalID, tmp);
            data.ChannelPackShader.SetTexture(kernel, ShaderPropertyCache.OutputID, output.Image.SourceRenderTexture);
            data.ChannelPackShader.Dispatch(kernel, threadX, threadY, 1);
        }
        public static void PostprocessImage(ImageSlot slot)
        {
            if (!slot.Image)
                return;
            if (!slot.AnyPostprocessorEnabled())
                return;

            RenderTexture tmp = GetCachedRenderTexture(slot.Image);
            bool beganPostprocessing = false;

            for (int i = 0; i < slot.ImagePostprocessors.Length; i++)
            {
                ImagePostprocessor postprocessor = slot.ImagePostprocessors[i];
                if (!postprocessor.Enabled)
                    continue;

                if (!beganPostprocessing)
                {
                    CopyTexture(slot.Image.SourceRenderTexture, tmp);
                    beganPostprocessing = true;
                }

                postprocessor.PostprocessImage(slot, tmp);

                if (i != slot.ImagePostprocessors.Length - 1)
                    CopyTexture(slot.Image.PostprocessedRenderTexture, tmp);
            }
        }
        public static byte[] GenerateOutput(OutputSlot slot, string extension)
        {
            Texture2D texture = new Texture2D(slot.Image.Width, slot.Image.Height, TextureFormat.RGBA32, false, false);

            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(slot.GetSourceTexture(), 0);
            request.WaitForCompletion();
            if (!request.hasError)
            {
                texture.SetPixelData(request.GetData<byte>(), 0);
                texture.Apply(false);
            }
            else
            {
                RenderTexture current = RenderTexture.active;
                RenderTexture.active = slot.GetSourceTexture();
                texture.ReadPixels(new Rect(0, 0, slot.Image.Width, slot.Image.Height), 0, 0);
                texture.Apply(false);
                RenderTexture.active = current;
            }
            byte[] bytes = GetTextureEncoded(texture, extension);
            Object.DestroyImmediate(texture);
            return bytes;
        }
        public static bool SupportedExportFormat(string extension)
        {
            switch (extension)
            {
                case ".png":
                case ".jpeg":
                case ".jpg":
                case ".JPG":
                case ".tga":
                    return true;
            }
            return false;
        }
        public static bool SupportedLoadFormat(string extension)
        {
            switch (extension)
            {
                case ".png":
                case ".jpeg":
                case ".jpg":
                case ".JPG":
                case ".tga":
                case ".tif":
                case ".psd":
                    return true;
            }
            return false;
        }
        public static string GetSupportedExportFormatsLog()
        {
            return ".png, .jpg or .tga";
        }
        public static string GetSupportedLoadFormatsFilter()
        {
            return "png,jpg,tga,tif,psd";
        }
        public static string GetChannelFirstLetter(Channel channel)
        {
            string letter = "";
            switch (channel)
            {
                case Channel.Red:
                    letter = "R";
                    break;
                case Channel.Green:
                    letter = "G";
                    break;
                case Channel.Blue:
                    letter = "B";
                    break;
                case Channel.Alpha:
                    letter = "A";
                    break;
            }
            return letter;
        }
        public static Color GetChannelColor(Channel channel)
        {
            Color color = new Color(1f, 1f, 1f, 1f);
            switch (channel)
            {
                case Channel.Red:
                    color = new Color(1f, 0f, 0f, 1f);
                    break;
                case Channel.Green:
                    color = new Color(0f, 1f, 0f, 1f);
                    break;
                case Channel.Blue:
                    color = new Color(0f, 0.15f, 1f, 1f);
                    break;
                case Channel.Alpha:
                    color = new Color(1f, 1f, 1f, 1f);
                    break;
            }
            return color;
        }
        public static void CopyTexture(Texture source, Texture destination, bool useMaxTextureLimit = false)
        {
            if (useMaxTextureLimit)
            {
                int limit = QualitySettings.globalTextureMipmapLimit;
                QualitySettings.globalTextureMipmapLimit = 0;
                Graphics.CopyTexture(source, 0, 0, 0, 0, source.width, source.height, destination, 0, 0, 0, 0);
                QualitySettings.globalTextureMipmapLimit = limit;
            }
            else
            {
                Graphics.CopyTexture(source, 0, 0, 0, 0, source.width, source.height, destination, 0, 0, 0, 0);
            }
        }
        public static string GetActivePlatformName()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneLinux64:
                    return "Standalone";
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iPhone";
                case BuildTarget.tvOS:
                    return "tvOS";
                case BuildTarget.PS4:
                    return "PS4";
                case BuildTarget.PS5:
                    return "PS5";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.XboxOne:
                    return "XboxOne";
                case BuildTarget.Switch:
                    return "Nintendo Switch";
                case BuildTarget.WSAPlayer:
                    return "Windows Store Apps";
                default:
                    return "Default";
            }
        }
        static byte[] GetTextureEncoded(Texture2D texture, string extension)
        {
            switch (extension)
            {
                case ".png":
                    return texture.EncodeToPNG();
                case ".jpeg":
                case ".jpg":
                case ".JPG":
                    return texture.EncodeToJPG(GetData().Preferences.jpgQuality);
                case ".tga":
                    return texture.EncodeToTGA();
            }
            return null;
        }
    }
}
