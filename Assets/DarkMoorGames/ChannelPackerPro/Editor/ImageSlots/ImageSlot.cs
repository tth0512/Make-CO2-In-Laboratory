using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    public abstract class ImageSlot : ScriptableObject
    {
        int previewKernel = 0;
        PreviewChannel channelPreview = PreviewChannel.Red | PreviewChannel.Green | PreviewChannel.Blue | PreviewChannel.Alpha;
        FilterMode previewFilterMode = FilterMode.Bilinear;

        protected Image image;
        UvDisplay uvDisplay;

        [SerializeField]
        ImagePostprocessor[] imagePostprocessors;

        public UvDisplay UvDisplay
        {
            get
            {
                return uvDisplay;
            }
        }
        public Image Image
        {
            get
            {
                return image;
            }
        }
        public ImagePostprocessor[] ImagePostprocessors
        {
            get
            {
                return imagePostprocessors;
            }
        }
        public int PreviewKernel
        {
            get
            {
                return previewKernel;
            }
        }
        public PreviewChannel ChannelPreview
        {
            get
            {
                return channelPreview;
            }
            set
            {
                channelPreview = value;

                UtilityData data = ChannelPackUtility.GetData();
                switch (channelPreview)
                {
                    case PreviewChannel.Red | PreviewChannel.Green | PreviewChannel.Blue | PreviewChannel.Alpha:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewRGBA");
                        break;
                    case PreviewChannel.Red | PreviewChannel.Green | PreviewChannel.Blue:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewRGB");
                        break;
                    case PreviewChannel.Red:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewRed");
                        break;
                    case PreviewChannel.Green:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewGreen");
                        break;
                    case PreviewChannel.Blue:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewBlue");
                        break;
                    case PreviewChannel.Alpha:
                    case PreviewChannel.Alpha | PreviewChannel.Grey:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewAlpha");
                        break;
                    case PreviewChannel.Red | PreviewChannel.Grey:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewRedGrey");
                        break;
                    case PreviewChannel.Green | PreviewChannel.Grey:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewGreenGrey");
                        break;
                    case PreviewChannel.Blue | PreviewChannel.Grey:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewBlueGrey");
                        break;
                    case PreviewChannel.Red | PreviewChannel.Green:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewRedGreen");
                        break;
                    case PreviewChannel.Red | PreviewChannel.Blue:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewRedBlue");
                        break;
                    case PreviewChannel.Red | PreviewChannel.Alpha:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewRedAlpha");
                        break;
                    case PreviewChannel.Green | PreviewChannel.Blue:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewGreenBlue");
                        break;
                    case PreviewChannel.Green | PreviewChannel.Alpha:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewGreenAlpha");
                        break;
                    case PreviewChannel.Blue | PreviewChannel.Alpha:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewBlueAlpha");
                        break;
                    case PreviewChannel.Red | PreviewChannel.Green | PreviewChannel.Alpha:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewRedGreenAlpha");
                        break;
                    case PreviewChannel.Red | PreviewChannel.Blue | PreviewChannel.Alpha:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewRedBlueAlpha");
                        break;
                    case PreviewChannel.Green | PreviewChannel.Blue | PreviewChannel.Alpha:
                        previewKernel = data.ChannelPreviewShader.FindKernel("PreviewGreenBlueAlpha");
                        break;
                }
                if (image)
                    ChannelPackUtility.UpdateImagePreview(this);
            }
        }

        public static T CreateSlot<T>() where T : ImageSlot
        {
            T slot = CreateInstance<T>();
            slot.hideFlags = HideFlags.HideAndDontSave;
            slot.uvDisplay = UvDisplay.Create();
            slot.OnCreate();
            return slot;
        }
        protected virtual void OnCreate()
        {

        }
        protected virtual void OnDestroy()
        {
            DestroyImmediate(image);
            image = null;

            DestroyImmediate(uvDisplay);
            uvDisplay = null;

            DestroyPostprocessors();
        }
        public virtual void DestroyImage(bool destroyPostprocessors)
        {
            DestroyImmediate(image);
            image = null;

            if (destroyPostprocessors)
            {
                DestroyPostprocessors();
            }
        }
        public void CreateEmptyImage(int width, int height)
        {
            DestroyImage(false);
            Image image = Image.CreateImage(width, height);
            this.image = image;
            this.image.PreviewRenderTexture.filterMode = previewFilterMode;
        }
        public void CreateImageFromAsset(Texture2D asset)
        {
            DestroyImage(false);
            Image image = Image.CreateImage(asset);
            this.image = image;
            this.image.PreviewRenderTexture.filterMode = previewFilterMode;
        }
        public RenderTexture GetSourceTexture()
        {
            if (!image)
                return null;

            if (AnyPostprocessorEnabled())
                return image.PostprocessedRenderTexture;
            return image.SourceRenderTexture;
        }
        public bool AnyPostprocessorEnabled()
        {
            if (imagePostprocessors == null)
                return false;
            for (int i = 0; i < imagePostprocessors.Length; i++)
            {
                if (imagePostprocessors[i].Enabled)
                    return true;
            }
            return false;
        }
        public void TogglePreviewFilterMode()
        {
            if (previewFilterMode == FilterMode.Bilinear)
                previewFilterMode = FilterMode.Point;
            else
                previewFilterMode = FilterMode.Bilinear;

            if (image)
            {
                image.PreviewRenderTexture.filterMode = previewFilterMode;
            }
        }
        public string GetPreviewFilterModeName()
        {
            switch (previewFilterMode)
            {
                case FilterMode.Bilinear:
                    return "Bilinear";
                case FilterMode.Point:
                    return "Point";
                default:
                    return string.Empty;
            }
        }
        void DestroyPostprocessors()
        {
            if (imagePostprocessors != null)
            {
                for (int i = 0; i < imagePostprocessors.Length; i++)
                    DestroyImmediate(imagePostprocessors[i]);
                imagePostprocessors = null;
            }
        }
    }
}
