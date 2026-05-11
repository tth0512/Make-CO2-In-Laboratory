namespace DarkMoorGames.ChannelPackerPro
{
    public sealed class InputSlot : ImageSlot
    {
        ImageGenerator imageGenerator;

        public ImageGenerator ImageGenerator
        {
            get
            {
                return imageGenerator;
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            DestroyImmediate(imageGenerator);
            imageGenerator = null;
        }
        public override void DestroyImage(bool destroyPostprocessors)
        {
            base.DestroyImage(destroyPostprocessors);

            DestroyImmediate(imageGenerator);
            imageGenerator = null;
        }
        public void TryRebuildGeneratorAndGenerateImage(GeneratorAttribute attribute)
        {
            if (!imageGenerator || imageGenerator.GetType() != attribute.Type)
            {
                DestroyImmediate(imageGenerator);
                imageGenerator = ImageGenerator.CreateImageGenerator(attribute);
                image.name = imageGenerator.name;
            }
            imageGenerator.GenerateImage(this);
        }
    }
}
