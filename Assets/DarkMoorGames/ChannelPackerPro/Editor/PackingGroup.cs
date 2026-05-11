using System.IO;
using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    public sealed class PackingGroup : ScriptableObject
    {
        public int extensionIndex;

        int targetWidth;
        int targetHeight;

        InputSlot[] inputSlots;
        OutputSlot outputSlot;

        string[] sourceInputNames;

        PackOption packOption1;
        PackOption packOption2;
        PackOption packOption3;
        PackOption packOption4;

        public int TargetWidth
        {
            get
            {
                return targetWidth;
            }
            set
            {
                targetWidth = value;
            }
        }
        public int TargetHeight
        {
            get
            {
                return targetHeight;
            }
            set
            {
                targetHeight = value;
            }
        }

        public OutputSlot OutputSlot
        {
            get
            {
                return outputSlot;
            }
            set
            {
                outputSlot = value;
            }
        }
        public string[] SourceInputNames
        {
            get
            {
                return sourceInputNames;
            }
        }
        public InputSlot[] InputSlots
        {
            get
            {
                return inputSlots;
            }
        }

        public PackOption PackOption1
        {
            get
            {
                return packOption1;
            }
        }
        public PackOption PackOption2
        {
            get
            {
                return packOption2;
            }
        }
        public PackOption PackOption3
        {
            get
            {
                return packOption3;
            }
        }
        public PackOption PackOption4
        {
            get
            {
                return packOption4;
            }
        }
        public string OutputName
        {
            get
            {
                UtilityData data = ChannelPackUtility.GetData();
                return data.outputPrefix + name + data.outputSuffix;
            }
        }
        public string OutputRichTextName
        {
            get
            {
                UtilityData data = ChannelPackUtility.GetData();
                return string.Format("<color={2}>{0}</color>{4}<color={3}>{1}</color>", data.outputPrefix, data.outputSuffix, data.Preferences.prefixTextColorHtml, data.Preferences.suffixTextColorHtml, name);
            }
        }

        public static PackingGroup Create(string name)
        {
            PackingGroup group = CreateInstance<PackingGroup>();
            group.hideFlags = HideFlags.HideAndDontSave;
            group.name = name;

            group.inputSlots = new InputSlot[5]
            {
                ImageSlot.CreateSlot<InputSlot>(),
                ImageSlot.CreateSlot<InputSlot>(),
                ImageSlot.CreateSlot<InputSlot>(),
                ImageSlot.CreateSlot<InputSlot>(),
                ImageSlot.CreateSlot<InputSlot>()
            };
            group.outputSlot = ImageSlot.CreateSlot<OutputSlot>();

            group.packOption1 = PackOption.CreateOption<PackOptionToRed>(Channel.Red, 1);
            group.packOption2 = PackOption.CreateOption<PackOptionToGreen>(Channel.Green, 2);
            group.packOption3 = PackOption.CreateOption<PackOptionToBlue>(Channel.Blue, 3);
            group.packOption4 = PackOption.CreateOption<PackOptionToAlpha>(Channel.Alpha, 4);

            group.sourceInputNames = new string[5] { "Value", "Image (1)", "Image (2)", "Image (3)", "Image (4)" };
            group.UpdateSourceInputNames();
            return group;
        }
        public static PackingGroup Create(PackingGroupData data)
        {
            PackingGroup group = CreateInstance<PackingGroup>();
            group.hideFlags = HideFlags.HideAndDontSave;
            group.name = data.Name;

            group.inputSlots = new InputSlot[5]
            {
                ImageSlot.CreateSlot<InputSlot>(),
                ImageSlot.CreateSlot<InputSlot>(),
                ImageSlot.CreateSlot<InputSlot>(),
                ImageSlot.CreateSlot<InputSlot>(),
                ImageSlot.CreateSlot<InputSlot>()
            };
            group.outputSlot = ImageSlot.CreateSlot<OutputSlot>();

            group.packOption1 = PackOption.CreateOption<PackOptionToRed>(data.PackOption1);
            group.packOption2 = PackOption.CreateOption<PackOptionToGreen>(data.PackOption2);
            group.packOption3 = PackOption.CreateOption<PackOptionToBlue>(data.PackOption3);
            group.packOption4 = PackOption.CreateOption<PackOptionToAlpha>(data.PackOption4);

            group.sourceInputNames = new string[5] { "Value", "Image (1)", "Image (2)", "Image (3)", "Image (4)" };
            group.UpdateSourceInputNames();
            return group;
        }
        public void SetPackingGroupData(PackingGroupData data)
        {
            name = data.Name;

            packOption1.SetPackOptionData(data.PackOption1);
            packOption2.SetPackOptionData(data.PackOption2);
            packOption3.SetPackOptionData(data.PackOption3);
            packOption4.SetPackOptionData(data.PackOption4);

            UpdateSourceInputNames();

            PackInputToOutputAll(true);
        }
        void OnDestroy()
        {
            for (int i = 1; i < inputSlots.Length; i++)
            {
                DestroyImmediate(inputSlots[i]);
                inputSlots[i] = null;
            }
            DestroyImmediate(outputSlot);

            DestroyImmediate(packOption1);
            DestroyImmediate(packOption2);
            DestroyImmediate(packOption3);
            DestroyImmediate(packOption4);
        }
        bool HasLoadedInputImage()
        {
            for (int i = 1; i < 5; i++)
            {
                if (inputSlots[i].Image)
                    return true;
            }
            return false;
        }
        int GetLoadedInputImageCount()
        {
            int count = 0;
            for (int i = 1; i < 5; i++)
            {
                if (inputSlots[i].Image)
                    count++;
            }
            return count;
        }
        void UpdateSourceInputNames()
        {
            for (int i = 1; i < 5; i++)
            {
                if (inputSlots[i].Image)
                    sourceInputNames[i] = inputSlots[i].Image.name + " (" + i + ")";
                else
                    sourceInputNames[i] = "Image (" + i + ")";
            }
        }
        public void DestroyInputImage(int index, bool destroyPostprocessors)
        {
            inputSlots[index].DestroyImage(destroyPostprocessors);
            UpdateSourceInputNames();

            if (!HasLoadedInputImage())
            {
                targetWidth = 0;
                targetHeight = 0;

                outputSlot.DestroyImage(true);
            }
            else
            {
                PackInputToOutputAll();
            }
        }
        public void DestroyAllImages()
        {
            for (int i = 1; i < 5; i++)
            {
                inputSlots[i].DestroyImage(true);
            }
            outputSlot.DestroyImage(true);

            UpdateSourceInputNames();

            targetWidth = 0;
            targetHeight = 0;
        }
        public void UpdateSourceInputName(int index)
        {
            // 0 is always value
            if (index == 0)
                return;

            if (inputSlots[index].Image)
                sourceInputNames[index] = inputSlots[index].Image.name + " (" + index + ")";
            else
                sourceInputNames[index] = "Image (" + index + ")";
        }
        public void PackInputToOutput(PackOption option)
        {
            if (outputSlot.Image)
            {
                UtilityData data = ChannelPackUtility.GetData();
                if (data.Preferences.autoUpdate)
                {
                    ChannelPackUtility.PackInputToOutput(option, outputSlot, inputSlots[option.inputImageIndex]);
                    ChannelPackUtility.PostprocessImage(outputSlot);
                    ChannelPackUtility.UpdateImagePreview(outputSlot);
                }
            }
        }
        public void PackInputToOutputAll(bool forceUpdate = false)
        {
            if (outputSlot.Image)
            {
                UtilityData data = ChannelPackUtility.GetData();
                if (data.Preferences.autoUpdate || forceUpdate)
                {
                    ChannelPackUtility.PackInputToOutput(packOption1, outputSlot, inputSlots[packOption1.inputImageIndex]);
                    ChannelPackUtility.PackInputToOutput(packOption2, outputSlot, inputSlots[packOption2.inputImageIndex]);
                    ChannelPackUtility.PackInputToOutput(packOption3, outputSlot, inputSlots[packOption3.inputImageIndex]);
                    ChannelPackUtility.PackInputToOutput(packOption4, outputSlot, inputSlots[packOption4.inputImageIndex]);

                    ChannelPackUtility.PostprocessImage(outputSlot);
                    ChannelPackUtility.UpdateImagePreview(outputSlot);
                }
            }
        }
        public void FindAndTryPostprocessImage(ImageSlot slot)
        {
            if (slot == outputSlot)
            {
                if (ChannelPackUtility.GetData().Preferences.autoUpdate)
                {
                    ChannelPackUtility.PostprocessImage(slot);
                    ChannelPackUtility.UpdateImagePreview(slot);
                }
            }
            else
            {
                for (int i = 1; i < 5; i++)
                {
                    if (inputSlots[i] == slot)
                    {
                        ChannelPackUtility.PostprocessImage(slot);
                        ChannelPackUtility.UpdateImagePreview(slot);
                        TryPackInputToOutput(inputSlots[i]);
                        break;
                    }
                }
            }
        }
        public bool LoadImageFromPath(string path, int slotIndex)
        {
            if (path == string.Empty)
                return false;

            ChannelPackUtility.GetData().Preferences.lastLoadSaveDirectory = Path.GetDirectoryName(path);

            if (!path.StartsWith("Assets"))
            {
                string relative = FileUtil.GetProjectRelativePath(path);
                if (relative != string.Empty)
                {
                    path = relative;
                }
            }

            void SetPlatformSettings(TextureImporter importer, TextureImporterPlatformSettings settings)
            {
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                settings.overridden = true;
                settings.format = TextureImporterFormat.RGBA32;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                settings.crunchedCompression = false;

                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
            bool ValidImporter(AssetImporter importer, bool autoFix)
            {
                if (importer == null)
                    return false;
                if (importer.GetType() != typeof(TextureImporter))
                    return false;

                bool isValid = true;

                try
                {
                    AssetDatabase.StartAssetEditing();
                    TextureImporter textureImporter = (TextureImporter)importer;

                    TextureImporterPlatformSettings platformSettings = textureImporter.GetPlatformTextureSettings(ChannelPackUtility.GetActivePlatformName());

                    if (textureImporter.textureShape != TextureImporterShape.Texture2D)
                        isValid = false;
                    if (textureImporter.textureType != TextureImporterType.Default)
                        isValid = false;
                    if (textureImporter.alphaIsTransparency)
                        isValid = false;
                    if (platformSettings.format != TextureImporterFormat.RGBA32)
                        isValid = false;

                    if (!isValid)
                    {
                        if (!autoFix)
                        {
                            if (EditorUtility.DisplayDialog("Fix Import Settings", "Please set :\nTexture Type to Default\nTexture Shape to 2D\nAlpha Is Transparency to false\nFormat to RGBA32", "Auto Fix", "Cancel"))
                            {
                                SetPlatformSettings(textureImporter, platformSettings);
                                isValid = true;
                            }
                        }
                        else
                        {
                            SetPlatformSettings(textureImporter, platformSettings);
                            isValid = true;
                        }
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
                return isValid;
            }
            bool LoadFromPath(string assetPath)
            {
                Texture2D asset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (targetWidth != 0 && targetHeight != 0)
                {
                    if (GetLoadedInputImageCount() == 1 && inputSlots[slotIndex].Image)
                    {
                        outputSlot.DestroyImage(false);
                    }
                    else if (asset.width != targetWidth || asset.height != targetHeight)
                    {
                        EditorUtility.DisplayDialog("Failed To Load Image", "Input Resolution: " + asset.width + " * " + asset.height + "\nOutput Resolution: " + targetWidth + " * " + targetHeight + "\n\nThe Input and Output Resolution need to be equal to load, You could try resizing the texture you want to load from its import settings.", "Ok");
                        return false;
                    }
                }

                targetWidth = asset.width;
                targetHeight = asset.height;

                inputSlots[slotIndex].CreateImageFromAsset(asset);

                ChannelPackUtility.PostprocessImage(inputSlots[slotIndex]);
                ChannelPackUtility.UpdateImagePreview(inputSlots[slotIndex]);

                UpdateSourceInputName(slotIndex);

                if (outputSlot.Image == null)
                {
                    outputSlot.CreateEmptyImage(targetWidth, targetHeight);
                    PackInputToOutputAll(true);
                }
                else
                {
                    TryPackInputToOutput(inputSlots[slotIndex]);
                }
                return true;
            }

            if (ValidImporter(AssetImporter.GetAtPath(path), false))
            {
                return LoadFromPath(path);
            }
            else
            {
                if (!path.StartsWith("Assets"))
                {
                    FileInfo info = new FileInfo(path);
                    if (ChannelPackUtility.SupportedLoadFormat(info.Extension))
                    {
                        if (EditorUtility.DisplayDialog("Import Asset", "Do you want to import " + info.Name + " to the assets folder then copy to the image slot?", "Import and Load", "Cancel"))
                        {
                            string importedAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + info.Name);
                            FileUtil.CopyFileOrDirectory(path, importedAssetPath);
                            AssetDatabase.ImportAsset(importedAssetPath);
                            AssetImporter importer = AssetImporter.GetAtPath(importedAssetPath);

                            if (ValidImporter(importer, true))
                            {
                                bool loaded = LoadFromPath(importedAssetPath);
                                Object importedObject = AssetDatabase.LoadAssetAtPath<Object>(importedAssetPath);
                                Selection.activeObject = importedObject;
                                EditorGUIUtility.PingObject(importedObject);
                                return loaded;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public void UpdateGeneratedImage(int inputIndex, GeneratorAttribute attribute)
        {
            Image image = inputSlots[inputIndex].Image;
            if (image != null)
            {
                inputSlots[inputIndex].TryRebuildGeneratorAndGenerateImage(attribute);

                ChannelPackUtility.PostprocessImage(inputSlots[inputIndex]);
                ChannelPackUtility.UpdateImagePreview(inputSlots[inputIndex]);

                TryPackInputToOutput(inputSlots[inputIndex]);
            }
            else
            {
                inputSlots[inputIndex].CreateEmptyImage(targetWidth, targetHeight);
                inputSlots[inputIndex].TryRebuildGeneratorAndGenerateImage(attribute);

                ChannelPackUtility.PostprocessImage(inputSlots[inputIndex]);
                ChannelPackUtility.UpdateImagePreview(inputSlots[inputIndex]);

                if (outputSlot.Image == null)
                {
                    outputSlot.CreateEmptyImage(targetWidth, targetHeight);
                    PackInputToOutputAll(true);
                }
                else
                {
                    TryPackInputToOutput(inputSlots[inputIndex]);
                }
            }
            UpdateSourceInputName(inputIndex);
        }
        void TryPackInputToOutput(InputSlot inputSlot, bool forceUpdate = false)
        {
            if (outputSlot.Image)
            {
                UtilityData data = ChannelPackUtility.GetData();
                if (data.Preferences.autoUpdate || forceUpdate)
                {
                    Image option1Image = inputSlots[packOption1.inputImageIndex].Image;
                    Image option2Image = inputSlots[packOption2.inputImageIndex].Image;
                    Image option3Image = inputSlots[packOption3.inputImageIndex].Image;
                    Image option4Image = inputSlots[packOption4.inputImageIndex].Image;
                    bool changed = false;

                    if (inputSlot.Image == option1Image)
                    {
                        ChannelPackUtility.PackInputToOutput(packOption1, outputSlot, inputSlot);
                        changed = true;
                    }
                    if (inputSlot.Image == option2Image)
                    {
                        ChannelPackUtility.PackInputToOutput(packOption2, outputSlot, inputSlot);
                        changed = true;
                    }
                    if (inputSlot.Image == option3Image)
                    {
                        ChannelPackUtility.PackInputToOutput(packOption3, outputSlot, inputSlot);
                        changed = true;
                    }
                    if (inputSlot.Image == option4Image)
                    {
                        ChannelPackUtility.PackInputToOutput(packOption4, outputSlot, inputSlot);
                        changed = true;
                    }
                    if (changed)
                    {
                        ChannelPackUtility.PostprocessImage(outputSlot);
                        ChannelPackUtility.UpdateImagePreview(outputSlot);
                    }
                }
            }
        }
    }
}