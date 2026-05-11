using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    public sealed class UtilityData : ScriptableObject
    {
        Preferences preferences;
        GUICache guiContentCache;

        ComputeShader channelPackShader;
        ComputeShader channelPreviewShader;

        RenderTexture cachedRenderTexture;
        UvRenderer uvRenderer;

        GeneratorAttribute[] generatorAttributes;
        PostprocessorAttribute[] postprocessorAttributes;

        Template[] templates;
        string[] exportFormats;
        string[] bitsPerChannelOptions;

        public string outputPrefix = string.Empty;
        public string outputSuffix = string.Empty;

        int activePackingGroupIndex;
        List<PackingGroup> packingGroups;

        public GeneratorAttribute[] GeneratorAttributes
        {
            get
            {
                return generatorAttributes;
            }
        }
        public PostprocessorAttribute[] PostprocessorAttributes
        {
            get
            {
                return postprocessorAttributes;
            }
        }
        public ComputeShader ChannelPackShader
        {
            get
            {
                return channelPackShader;
            }
        }
        public ComputeShader ChannelPreviewShader
        {
            get
            {
                return channelPreviewShader;
            }
        }
        public RenderTexture CachedRenderTexture
        {
            get
            {
                return cachedRenderTexture;
            }
            set
            {
                cachedRenderTexture = value;
            }
        }
        public UvRenderer UvRenderer
        {
            get
            {
                return uvRenderer;
            }
        }
        public GUICache GUICache
        {
            get
            {
                return guiContentCache;
            }
        }
        public Preferences Preferences
        {
            get
            {
                return preferences;
            }
        }
        public int ActivePackingGroupIndex
        {
            get
            {
                return activePackingGroupIndex;
            }
        }
        public string[] ExportFormats
        {
            get
            {
                return exportFormats;
            }
        }
        public string[] BitsPerChannelOptions
        {
            get
            {
                return bitsPerChannelOptions;
            }
        }

        public static UtilityData Create()
        {
            UtilityData data = CreateInstance<UtilityData>();
            data.hideFlags = HideFlags.HideAndDontSave;
            data.exportFormats = new string[] { ".png", ".jpg", ".tga" };
            data.bitsPerChannelOptions = new string[] { "8bpc", "16bpc" };
            data.preferences = Preferences.Create();
            data.guiContentCache = GUICache.Create();
            data.uvRenderer = UvRenderer.Create();
            data.GetRequiredShaders();
            data.GetImageGeneratorAndPostprocessorTypes();
            data.packingGroups = new List<PackingGroup>
            {
                PackingGroup.Create("New Image")
            };
            return data;
        }
        void OnEnable()
        {
            AssemblyReloadEvents.afterAssemblyReload -= AfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;
        }
        void OnDestroy()
        {
            AssemblyReloadEvents.afterAssemblyReload -= AfterAssemblyReload;

            if (guiContentCache)
            {
                DestroyImmediate(guiContentCache);
                guiContentCache = null;
            }
            if (preferences)
            {
                DestroyImmediate(preferences);
                preferences = null;
            }
            if (cachedRenderTexture)
            {
                cachedRenderTexture.Release();
                DestroyImmediate(cachedRenderTexture);
                cachedRenderTexture = null;
            }
            if (uvRenderer)
            {
                DestroyImmediate(uvRenderer);
                uvRenderer = null;
            }

            DestroyPackingGroups();

            channelPackShader = null;
            channelPreviewShader = null;
        }
        void AfterAssemblyReload()
        {
            GetImageGeneratorAndPostprocessorTypes();
        }
        void GetImageGeneratorAndPostprocessorTypes()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> imageGeneratorTypesList = new List<Type>();
            List<Type> imagePostprocessorTypesList = new List<Type>();

            List<GeneratorAttribute> generatorAttributeList = new List<GeneratorAttribute>();
            List<PostprocessorAttribute> postprocessorAttributeList = new List<PostprocessorAttribute>();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; j++)
                {
                    if (assemblyTypes[j].IsSubclassOf(typeof(ImageGenerator)))
                    {
                        imageGeneratorTypesList.Add(assemblyTypes[j]);

                        GeneratorAttribute attribute = (GeneratorAttribute)assemblyTypes[j].GetCustomAttribute(typeof(GeneratorAttribute), false);
                        if (attribute != null)
                            generatorAttributeList.Add(attribute);
                    }
                    else if (assemblyTypes[j].IsSubclassOf(typeof(ImagePostprocessor)))
                    {
                        imagePostprocessorTypesList.Add(assemblyTypes[j]);

                        PostprocessorAttribute attribute = (PostprocessorAttribute)assemblyTypes[j].GetCustomAttribute(typeof(PostprocessorAttribute), false);
                        if (attribute != null)
                            postprocessorAttributeList.Add(attribute);
                    }
                }
            }

            postprocessorAttributes = postprocessorAttributeList.ToArray();
            generatorAttributes = generatorAttributeList.ToArray();

            if (imageGeneratorTypesList.Count != generatorAttributes.Length)
            {
                for (int i = 0; i < imageGeneratorTypesList.Count; i++)
                {
                    if (imageGeneratorTypesList[i].GetCustomAttribute(typeof(GeneratorAttribute)) == null)
                    {
                        Debug.LogWarning("Missing (GeneratorAttribute) on the type (" + imageGeneratorTypesList[i].Name + ")");
                    }
                }
            }
            if (imagePostprocessorTypesList.Count != postprocessorAttributes.Length)
            {
                for (int i = 0; i < imagePostprocessorTypesList.Count; i++)
                {
                    if (imagePostprocessorTypesList[i].GetCustomAttribute(typeof(PostprocessorAttribute)) == null)
                    {
                        Debug.LogWarning("Missing (PostprocessorAttribute) on the type (" + imagePostprocessorTypesList[i].Name + ")");
                    }
                }
            }

            generatorAttributeList.Clear();
            postprocessorAttributeList.Clear();
            imageGeneratorTypesList.Clear();
            imagePostprocessorTypesList.Clear();
        }
        void GetRequiredShaders()
        {
            channelPackShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/ChannelPackShader.compute");
            channelPreviewShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/DarkMoorGames/ChannelPackerPro/Editor/Shaders/ChannelPreviewShader.compute");
        }
        void SaveTemplate()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Template", string.Empty, "asset", "Save a Template of the Packing Group names and options.");
            if (path == string.Empty)
                return;

            Template template = CreateInstance<Template>();
            template.SetTemplateData(packingGroups);

            AssetDatabase.CreateAsset(template, path);
            AssetDatabase.Refresh();
        }
        void FindTemplatesInProject()
        {
            string[] guids = AssetDatabase.FindAssets("t:DarkMoorGames.ChannelPackerPro.Template");
            templates = new Template[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                Template template = AssetDatabase.LoadAssetAtPath<Template>(AssetDatabase.GUIDToAssetPath(guids[i]));
                templates[i] = template;
            }
        }
        void DestroyPackingGroups()
        {
            for (int i = 0; i < packingGroups.Count; i++)
                DestroyImmediate(packingGroups[i]);
            packingGroups.Clear();
        }
        public void ResetToDefaultPackingGroups(bool clearAffixes)
        {
            DestroyPackingGroups();
            packingGroups.Add(PackingGroup.Create("New Image"));
            activePackingGroupIndex = 0;
            if (clearAffixes)
            {
                outputPrefix = string.Empty;
                outputSuffix = string.Empty;
            }
        }
        public void RemoveAllPackingGroupImages()
        {
            for (int i = 0; i < packingGroups.Count; i++)
                packingGroups[i].DestroyAllImages();
        }
        public GeneratorAttribute GetGeneratorAttribute(ImageGenerator generator)
        {
            if (!generator)
                return null;

            for (int i = 0; i < generatorAttributes.Length; i++)
            {
                if (generatorAttributes[i].Type == generator.GetType())
                {
                    return generatorAttributes[i];
                }
            }
            return null;
        }
        public string[] GetPackingGroupNames(PackingGroup ignore)
        {
            return ChannelPackUtility.GetObjectNames(ignore, packingGroups);
        }
        public string[] GetPackingGroupNames()
        {
            return ChannelPackUtility.GetObjectNames(packingGroups);
        }
        public PackingGroup GetSelectedPackingGroup()
        {
            return packingGroups[activePackingGroupIndex];
        }
        public void SetSelectedPackingGroup(int index)
        {
            activePackingGroupIndex = index;
        }
        public PackingGroup GetPackingGroup(int index)
        {
            if (index > packingGroups.Count - 1 || index < 0)
                return null;
            return packingGroups[index];
        }
        public int PackingGroupCount()
        {
            return packingGroups.Count;
        }
        public void AddPackingGroup(PackingGroup group, bool selectAdded)
        {
            packingGroups.Add(group);
            if (selectAdded)
                activePackingGroupIndex = packingGroups.Count - 1;
        }
        public void RemovePackingGroup(int index)
        {
            DestroyImmediate(packingGroups[index]);
            packingGroups.RemoveAt(index);

            if (index < activePackingGroupIndex)
                activePackingGroupIndex -= 1;
            activePackingGroupIndex = Mathf.Clamp(activePackingGroupIndex, 0, packingGroups.Count - 1);
        }
        public bool CanBatchExport()
        {
            int count = 0;
            for (int i = 0; i < packingGroups.Count; i++)
            {
                if (packingGroups[i].OutputSlot.Image)
                    count++;
                if (count == 2)
                    return true;
            }
            return false;
        }
        public void SetTemplate(object data)
        {
            Template template = (Template)data;
            int id = EditorUtility.DisplayDialogComplex("Set Template", "Do you want to set the template " + template.name + "?", "Keep Images", "Cancel", "Clear Images");
            if (id == 1)
                return;

            if (id == 0)
            {
                if (packingGroups.Count != template.Data.Count)
                {
                    if (packingGroups.Count > template.Data.Count)
                    {
                        while (packingGroups.Count != template.Data.Count)
                        {
                            int index = packingGroups.Count - 1;
                            DestroyImmediate(packingGroups[index]);
                            packingGroups.RemoveAt(index);
                        }
                    }
                    for (int i = 0; i < template.Data.Count; i++)
                    {
                        if (i < packingGroups.Count)
                        {
                            packingGroups[i].SetPackingGroupData(template.Data[i]);
                        }
                        else
                        {
                            packingGroups.Add(PackingGroup.Create(template.Data[i]));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < template.Data.Count; i++)
                    {
                        packingGroups[i].SetPackingGroupData(template.Data[i]);
                    }
                }
            }
            else if (id == 2)
            {
                DestroyPackingGroups();
                for (int i = 0; i < template.Data.Count; i++)
                    packingGroups.Add(PackingGroup.Create(template.Data[i]));
            }

            if (activePackingGroupIndex > packingGroups.Count - 1)
                activePackingGroupIndex = packingGroups.Count - 1;
        }
        public GenericMenu GetTemplateGenericMenu()
        {
            FindTemplatesInProject();

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Save Template..."), false, SaveTemplate);
            if (templates.Length > 0)
            {
                menu.AddSeparator("");
                for (int i = 0; i < templates.Length; i++)
                {
                    menu.AddItem(new GUIContent(templates[i].name), false, SetTemplate, templates[i]);
                }
            }
            return menu;
        }
    }
}
