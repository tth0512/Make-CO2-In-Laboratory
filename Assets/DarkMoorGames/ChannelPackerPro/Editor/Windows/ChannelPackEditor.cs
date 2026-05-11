using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    struct GroupExtension
    {
        PackingGroup group;
        int extensionIndex;

        public void ApplyGroupExtensionIndex()
        {
            if (group)
                group.extensionIndex = extensionIndex;
        }

        public GroupExtension(PackingGroup group, int extensionIndex)
        {
            this.group = group;
            this.extensionIndex = extensionIndex;
        }
    }
    struct CreateImageContext
    {
        PackingGroup group;
        int inputIndex;
        GeneratorAttribute attribute;

        public PackingGroup Group
        {
            get
            {
                return group;
            }
        }
        public int InputIndex
        {
            get
            {
                return inputIndex;
            }
        }
        public GeneratorAttribute Attribute
        {
            get
            {
                return attribute;
            }
        }

        public CreateImageContext(PackingGroup group, int inputIndex, GeneratorAttribute attribute)
        {
            this.group = group;
            this.inputIndex = inputIndex;
            this.attribute = attribute;
        }
    }
    struct GroupSlotContext
    {
        PackingGroup group;
        ImageSlot slot;

        public PackingGroup Group
        {
            get
            {
                return group;
            }
        }
        public ImageSlot Slot
        {
            get
            {
                return slot;
            }
        }

        public GroupSlotContext(PackingGroup group, ImageSlot slot)
        {
            this.group = group;
            this.slot = slot;
        }
    }
    struct ImageSlotUvDisplayContext
    {
        PackingGroup group;
        ImageSlot slot;
        int uvSetIndex;

        public ImageSlotUvDisplayContext(PackingGroup group, ImageSlot slot, int uvSetIndex)
        {
            this.group = group;
            this.slot = slot;
            this.uvSetIndex = uvSetIndex;
        }
        public void SetSlotUvDisplayIndex()
        {
            slot.UvDisplay.uvSetIndex = uvSetIndex;
            group.FindAndTryPostprocessImage(slot);
        }
    }

    public sealed class ChannelPackEditor : EditorWindow
    {
        const string WINDOW_NAME = "Channel Packer Pro - " + ChannelPackUtility.VERSION;

        int viewMode;

        Rect saveButtonRect;
        Rect formatButtonRect;
        Rect inputLoadButtonRect;
        Rect preferencesButtonRect;
        Rect uvButtonRect;

        Rect textRect;

        string editNameText;
        bool editingName;
        Template openWithTemplate;

        bool unityEditorQuitting;

        [MenuItem("Window/DarkMoorGames/Channel Packer Pro #p")]
        static void MenuItemOpen()
        {
            Open(true, null);
        }
        public static void Open(bool tryShowRatingWindow, Template template)
        {
            ChannelPackEditor window = GetWindow<ChannelPackEditor>(true);
            window.titleContent = new GUIContent(WINDOW_NAME);
            window.maxSize = new Vector2(1100f, 745f);
            window.minSize = window.maxSize;
            window.openWithTemplate = template;
            window.Show();

            if (tryShowRatingWindow)
                StoreRatingWindow.TryShow(window);
        }
        static void OpenCreate()
        {
            ChannelPackEditor window = CreateInstance<ChannelPackEditor>();
            window.titleContent = new GUIContent(WINDOW_NAME);
            window.maxSize = new Vector2(1100f, 745f);
            window.minSize = window.maxSize;
            window.openWithTemplate = null;
            window.ShowUtility();
        }
        void Awake()
        {
            ChannelPackUtility.Init();
        }
        void OnDestroy()
        {
            if (unityEditorQuitting)
            {
                ChannelPackUtility.Cleanup();
                return;
            }

            int option = EditorUtility.DisplayDialogComplex("Close Channel Packer Pro", "Are you sure you want to close the editor.", "Keep Data", "Cancel", "Clear Data");
            if (option == 2)
            {
                ChannelPackUtility.Cleanup();
            }
            else if (option == 1)
            {
                OpenCreate();
            }
        }
        void OnEnable()
        {
            Undo.undoRedoPerformed -= Repaint;
            Undo.undoRedoPerformed += Repaint;

            EditorApplication.wantsToQuit -= OnUnityEditorQuitting;
            EditorApplication.wantsToQuit += OnUnityEditorQuitting;
        }
        bool OnUnityEditorQuitting()
        {
            unityEditorQuitting = true;
            return true;
        }
        void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
            EditorApplication.wantsToQuit -= OnUnityEditorQuitting;
        }
        void OnLostFocus()
        {
            if (editingName)
            {
                ApplyEditingName(ChannelPackUtility.GetData().GetSelectedPackingGroup());
            }
        }
        void OnGUI()
        {
            PackingGroup activeGroup = ChannelPackUtility.GetData().GetSelectedPackingGroup();
            switch (viewMode)
            {
                case 0:
                    DrawPackingGroupToolbar();

                    DrawInputPreview(new Rect(5f, 25f, 310f, 355f), activeGroup, 1);
                    DrawInputPreview(new Rect(320f, 25f, 310f, 355f), activeGroup, 2);

                    DrawInputPreview(new Rect(5f, 385f, 310f, 355f), activeGroup, 3);
                    DrawInputPreview(new Rect(320f, 385f, 310f, 355f), activeGroup, 4);

                    DrawOutputPreview(activeGroup);
                    DrawPackingGroup(activeGroup);
                    break;
                case 1:
                    DrawInputLargePreview(activeGroup, 1);
                    break;
                case 2:
                    DrawInputLargePreview(activeGroup, 2);
                    break;
                case 3:
                    DrawInputLargePreview(activeGroup, 3);
                    break;
                case 4:
                    DrawInputLargePreview(activeGroup, 4);
                    break;
                case 5:
                    DrawOutputLargePreview(activeGroup);
                    break;
            }
            if (openWithTemplate)
            {
                EditorApplication.delayCall -= DelayOpenWithTemplateCall;
                EditorApplication.delayCall += DelayOpenWithTemplateCall;
            }
        }
        void DrawPackingGroupToolbar()
        {
            UtilityData data = ChannelPackUtility.GetData();
            int groupCount = data.PackingGroupCount();

            GUILayout.BeginArea(new Rect(0, 0, position.width, 20f), EditorStyles.toolbar);

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < groupCount; i++)
            {
                bool selected = data.ActivePackingGroupIndex == i;
                EditorGUI.BeginChangeCheck();
                GUILayout.Toggle(selected, data.GetPackingGroup(i).OutputRichTextName, data.GUICache.ToolbarButtonRichText, GUILayout.Width(135f));
                if (EditorGUI.EndChangeCheck())
                {
                    data.SetSelectedPackingGroup(i);
                }

                EditorGUI.BeginDisabledGroup(groupCount == 1);
                if (GUILayout.Button(data.GUICache.RemovePackingGroupContent, EditorStyles.toolbarButton, GUILayout.Width(25f)))
                {
                    if (EditorUtility.DisplayDialog("Remove Packing Group", "Are you sure you want to remove " + data.GetPackingGroup(i).OutputName + "?", "Yes", "No"))
                    {
                        data.RemovePackingGroup(i);
                        Repaint();
                        GUIUtility.ExitGUI();
                        break;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            if (groupCount != 6)
            {
                if (GUILayout.Button(data.GUICache.AddNewPackingGroupContent, EditorStyles.toolbarButton, GUILayout.Width(160f)))
                {
                    data.AddPackingGroup(PackingGroup.Create(ObjectNames.GetUniqueName(data.GetPackingGroupNames(), "New Image")), true);
                    Repaint();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            data.Preferences.autoUpdate = GUILayout.Toggle(data.Preferences.autoUpdate, new GUIContent("Auto", "Allow Automatic updating of the output image."), EditorStyles.toolbarButton, GUILayout.Width(40f));
            if (EditorGUI.EndChangeCheck())
            {
                if (data.Preferences.autoUpdate)
                {
                    for (int i = 0; i < data.PackingGroupCount(); i++)
                        data.GetPackingGroup(i).PackInputToOutputAll(true);
                }
            }
            EditorGUI.BeginDisabledGroup(data.Preferences.autoUpdate);
            if (GUILayout.Button(new GUIContent("Update", "Manually update the output image."), EditorStyles.toolbarButton, GUILayout.Width(55f)))
            {
                data.GetSelectedPackingGroup().PackInputToOutputAll(true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorGUILayout.DropdownButton(data.GUICache.SettingsDropdownContent, FocusType.Passive, EditorStyles.toolbarDropDown, GUILayout.MaxWidth(35f)))
            {
                SettingsPopupContent content = new SettingsPopupContent(this);
                Vector2 size = content.GetWindowSize();
                preferencesButtonRect.x -= size.x - preferencesButtonRect.width;
                PopupWindow.Show(preferencesButtonRect, content);
            }
            if (Event.current.type == EventType.Repaint)
                preferencesButtonRect = GUILayoutUtility.GetLastRect();

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        bool DrawPackOption(PackOption option, string[] inputNames)
        {
            EditorGUILayout.BeginHorizontal();

            Color color = GUI.backgroundColor;
            Color tint = ChannelPackUtility.GetChannelColor(option.OutputChannel);
            GUI.backgroundColor = tint;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.backgroundColor = color;

            EditorGUI.BeginChangeCheck();
            option.inputImageIndex = EditorGUILayout.Popup(new GUIContent("Source", "The source type to pack into the output image.\n\nIf value is selected this will pack all the pixels by the value into the output channel - (" + option.OutputChannel + ").\n\nIf a source image is selected this will pack the selected channel of that image - (" + option.inputChannel + ") to (" + option.OutputChannel + ")."), option.inputImageIndex, inputNames, GUILayout.MaxWidth(200f));
            if (option.inputImageIndex == 0)
            {
                EditorGUILayout.Space();
                option.value = EditorGUILayout.Slider(option.value, 0f, 1f, GUILayout.MaxWidth(110f));
                EditorGUILayout.Space();
                option.invertChannel = EditorGUILayout.Toggle(new GUIContent("Invert", "Invert the input value."), option.invertChannel, GUILayout.MinWidth(80f));
            }
            else
            {
                EditorGUILayout.Space();
                option.inputChannel = (Channel)EditorGUILayout.EnumPopup(new GUIContent("Channel", "The selected input channel from the source image."), option.inputChannel, GUILayout.MaxWidth(110f));
                EditorGUILayout.Space();
                option.invertChannel = EditorGUILayout.Toggle(new GUIContent("Invert", "Invert the selected input channel - (" + option.inputChannel + ")."), option.invertChannel, GUILayout.MinWidth(80f));
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = tint;
            GUILayout.Box(new GUIContent(ChannelPackUtility.GetChannelFirstLetter(option.OutputChannel), "The output channel (" + option.OutputChannel + ")."), ChannelPackUtility.GetData().GUICache.BoxCenteredLabel, GUILayout.Width(24f), GUILayout.Height(24f));
            GUI.backgroundColor = color;

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                return true;
            }
            return false;
        }
        void DrawPackingGroup(PackingGroup group)
        {
            UtilityData data = ChannelPackUtility.GetData();

            void DrawPackingGroupPackOptions()
            {
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 55f;

                if (DrawPackOption(group.PackOption1, group.SourceInputNames))
                {
                    group.PackInputToOutput(group.PackOption1);
                }
                EditorGUILayout.Space();
                if (DrawPackOption(group.PackOption2, group.SourceInputNames))
                {
                    group.PackInputToOutput(group.PackOption2);
                }
                EditorGUILayout.Space();
                if (DrawPackOption(group.PackOption3, group.SourceInputNames))
                {
                    group.PackInputToOutput(group.PackOption3);
                }
                EditorGUILayout.Space();
                if (DrawPackOption(group.PackOption4, group.SourceInputNames))
                {
                    group.PackInputToOutput(group.PackOption4);
                }
                EditorGUIUtility.labelWidth = labelWidth;
            }
            void DrawPackingGroupExportOptions()
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 55f;

                EditorGUILayout.PrefixLabel(new GUIContent("Output", "The Output Image name."), EditorStyles.textField);
                GUI.SetNextControlName("EditingObjectName");
                if (!editingName)
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        if (textRect.Contains(Event.current.mousePosition))
                        {
                            editingName = true;
                            editNameText = group.name;
                            Event.current.Use();

                            EditorGUI.FocusTextInControl("EditingObjectName");
                        }
                    }
                    EditorGUILayout.TextField(group.OutputRichTextName, data.GUICache.RichTextField, GUILayout.MaxWidth(335f));
                    if (Event.current.type == EventType.Repaint)
                        textRect = GUILayoutUtility.GetLastRect();

                    if (GUI.GetNameOfFocusedControl() == "EditingObjectName")
                        EditorGUI.FocusTextInControl("");
                }
                else
                {
                    editNameText = EditorGUILayout.TextField(editNameText, GUILayout.MaxWidth(335f));
                    if (GUI.GetNameOfFocusedControl() == "EditingObjectName")
                    {
                        if (Event.current.keyCode == KeyCode.Return)
                            ApplyEditingName(group);
                    }
                    else
                    {
                        ApplyEditingName(group);
                    }
                }
                EditorGUIUtility.labelWidth = labelWidth;

                GUILayout.FlexibleSpace();

                if (EditorGUILayout.DropdownButton(new GUIContent(data.ExportFormats[group.extensionIndex]), FocusType.Passive, GUILayout.MinWidth(45f)))
                {
                    if (editingName)
                    {
                        ApplyEditingName(group);
                        GUIUtility.ExitGUI();
                    }
                    GenericMenu menu = GetExportFormatsMenu(group);
                    menu.DropDown(formatButtonRect);
                }
                if (Event.current.type == EventType.Repaint)
                    formatButtonRect = GUILayoutUtility.GetLastRect();

                if (EditorGUILayout.DropdownButton(data.GUICache.ExportButtonContent, FocusType.Passive, GUILayout.MaxWidth(75f)))
                {
                    if (editingName)
                    {
                        ApplyEditingName(group);
                        GUIUtility.ExitGUI();
                    }
                    GenericMenu menu = GetExportButtonMenu(group);
                    menu.DropDown(saveButtonRect);
                }
                if (Event.current.type == EventType.Repaint)
                    saveButtonRect = GUILayoutUtility.GetLastRect();
                EditorGUILayout.EndHorizontal();
            }

            Rect optionsRect = new Rect(635f, 530f, 460f, 210f);
            GUILayout.BeginArea(optionsRect, EditorStyles.helpBox);
            EditorGUILayout.LabelField("Output", data.GUICache.BoldCenteredLabel);
            GUILayout.FlexibleSpace();
            DrawPackingGroupPackOptions();
            GUILayout.FlexibleSpace();
            DrawPackingGroupExportOptions();
            GUILayout.EndArea();
        }
        void ApplyGroupExtensionIndex(object groupFormatObject)
        {
            GroupExtension format = (GroupExtension)groupFormatObject;
            format.ApplyGroupExtensionIndex();
        }
        void ApplyEditingName(PackingGroup group)
        {
            string trimmed = editNameText.Trim();
            if (trimmed != string.Empty)
            {
                group.name = ObjectNames.GetUniqueName(ChannelPackUtility.GetData().GetPackingGroupNames(group), trimmed);
            }
            EditorGUI.FocusTextInControl("");
            EditorGUIUtility.editingTextField = false;
            editingName = false;
            Repaint();
        }
        void DrawPreviewSettings(ImageSlot slot)
        {
            GUICache gUICache = ChannelPackUtility.GetData().GUICache;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(slot.GetPreviewFilterModeName(), "The preview image filter mode."), GUILayout.Width(60f)))
            {
                slot.TogglePreviewFilterMode();
            }

            void AssignPreviewChannel(PreviewChannel channel)
            {
                if (Event.current.shift)
                {
                    slot.ChannelPreview &= ~PreviewChannel.Grey;
                    slot.ChannelPreview ^= channel;
                }
                else if (Event.current.control)
                    slot.ChannelPreview = PreviewChannel.Red | PreviewChannel.Green | PreviewChannel.Blue | PreviewChannel.Alpha;
                else if (Event.current.alt)
                    slot.ChannelPreview = channel | PreviewChannel.Grey;
                else
                    slot.ChannelPreview = channel;
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle((slot.ChannelPreview & PreviewChannel.Red) != 0, gUICache.PreviewChannelRedContent, "ButtonLeft", GUILayout.Width(25f));
            if (EditorGUI.EndChangeCheck())
            {
                AssignPreviewChannel(PreviewChannel.Red);
            }
            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle((slot.ChannelPreview & PreviewChannel.Green) != 0, gUICache.PreviewChannelGreenContent, "ButtonMid", GUILayout.Width(25f));
            if (EditorGUI.EndChangeCheck())
            {
                AssignPreviewChannel(PreviewChannel.Green);
            }
            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle((slot.ChannelPreview & PreviewChannel.Blue) != 0, gUICache.PreviewChannelBlueContent, "ButtonMid", GUILayout.Width(25f));
            if (EditorGUI.EndChangeCheck())
            {
                AssignPreviewChannel(PreviewChannel.Blue);
            }
            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle((slot.ChannelPreview & PreviewChannel.Alpha) != 0, gUICache.PreviewChannelAlphaContent, "ButtonRight", GUILayout.Width(25f));
            if (EditorGUI.EndChangeCheck())
            {
                AssignPreviewChannel(PreviewChannel.Alpha);
            }

            slot.UvDisplay.show = GUILayout.Toggle(slot.UvDisplay.show, gUICache.ShowUvsButtonContent, "Button", GUILayout.Width(30f));
            EditorGUILayout.EndHorizontal();
        }
        void DrawInputPreview(Rect offset, PackingGroup group, int inputIndex)
        {
            UtilityData data = ChannelPackUtility.GetData();
            bool hasInputImage = group.InputSlots[inputIndex].Image;

            GUILayout.BeginArea(offset, EditorStyles.helpBox);
            EditorGUI.LabelField(new Rect(30f, 5f, offset.width - 60f, 16f), hasInputImage ? new GUIContent(group.InputSlots[inputIndex].Image.name, "Image Slot - " + inputIndex) : new GUIContent("No Image", "Image Slot - " + inputIndex), data.GUICache.BoldCenteredLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(data.GUICache.ZoomInToLargeViewContent, GUILayout.MaxWidth(20f)))
            {
                viewMode = inputIndex;
            }
            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(!hasInputImage);
            if (GUILayout.Button(data.GUICache.RemoveInputImageContent, GUILayout.MaxWidth(20f)))
            {
                int optionID = EditorUtility.DisplayDialogComplex("Remove Image", "Do you want to remove the input slot image.", "Remove Postprocessors", "Cancel", "Keep Postprocessors");
                if (optionID == 0)
                    group.DestroyInputImage(inputIndex, true);
                else if (optionID == 2)
                    group.DestroyInputImage(inputIndex, false);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            DrawInputEvent(new Rect(5f, 25f, 300f, 300f), group, inputIndex);

            GUILayout.BeginArea(new Rect(5f, 330f, 300f, 30f));
            EditorGUILayout.BeginHorizontal();

            DrawPreviewSettings(group.InputSlots[inputIndex]);

            if (EditorGUILayout.DropdownButton(data.GUICache.LoadImageContent, FocusType.Passive, GUILayout.MaxWidth(55f)))
            {
                GenericMenu menu = GetInputGenericMenu(group, inputIndex);
                menu.DropDown(inputLoadButtonRect);
            }
            if (Event.current.type == EventType.Repaint)
                inputLoadButtonRect = GUILayoutUtility.GetLastRect();

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUILayout.EndArea();
        }
        void DrawOutputPreview(PackingGroup group)
        {
            UtilityData data = ChannelPackUtility.GetData();

            GUILayout.BeginArea(new Rect(635f, 25f, 460f, 500f), EditorStyles.helpBox);
            EditorGUI.LabelField(new Rect(0f, 5f, 450f, 16f), group.OutputRichTextName, data.GUICache.RichBoldCenteredLabel);

            DrawOutputEvent(new Rect(5f, 25f, 450f, 450f), group);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(data.GUICache.ZoomInToLargeViewContent, GUILayout.MaxWidth(20f), GUILayout.MaxHeight(20f)))
            {
                viewMode = 5;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Resolution - " + group.TargetWidth + " * " + group.TargetHeight, data.GUICache.BoldRightLabel);

            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            DrawPreviewSettings(group.OutputSlot);
            if (EditorGUILayout.DropdownButton(new GUIContent("Image"), FocusType.Passive, GUILayout.MaxWidth(55f)))
            {
                GenericMenu menu = GetOutputGenericMenu(group);
                menu.DropDown(uvButtonRect);
            }
            if (Event.current.type == EventType.Repaint)
                uvButtonRect = GUILayoutUtility.GetLastRect();

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        void DrawInputLargePreview(PackingGroup group, int inputIndex)
        {
            UtilityData data = ChannelPackUtility.GetData();

            Image image = group.InputSlots[inputIndex].Image;

            GUILayout.BeginArea(new Rect(5f, 5f, position.width - 10f, position.height - 10f), EditorStyles.helpBox);
            EditorGUI.LabelField(new Rect(80f, 5f, position.width - 160, 16f), image ? new GUIContent(image.name, "Image Slot - " + inputIndex) : new GUIContent("No Image", "Image Slot - " + inputIndex), data.GUICache.BoldCenteredLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(data.GUICache.ZoomOutOfLargeViewContent, GUILayout.MaxWidth(20f), GUILayout.MaxHeight(20f)))
            {
                viewMode = 0;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Resolution - " + group.TargetWidth + " * " + group.TargetHeight, data.GUICache.BoldRightLabel);
            EditorGUILayout.EndHorizontal();

            DrawInputEvent(new Rect(5f, 25f, position.width - 20f, position.height - 60f), group, inputIndex);

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            DrawPreviewSettings(group.InputSlots[inputIndex]);

            GUILayout.FlexibleSpace();
            if (EditorGUILayout.DropdownButton(data.GUICache.LoadImageContent, FocusType.Passive, GUILayout.MaxWidth(55f)))
            {
                GenericMenu menu = GetInputGenericMenu(group, inputIndex);
                menu.DropDown(inputLoadButtonRect);
            }
            if (Event.current.type == EventType.Repaint)
                inputLoadButtonRect = GUILayoutUtility.GetLastRect();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        void DrawOutputLargePreview(PackingGroup group)
        {
            UtilityData data = ChannelPackUtility.GetData();

            GUILayout.BeginArea(new Rect(5f, 5f, position.width - 10f, position.height - 10f), EditorStyles.helpBox);
            EditorGUI.LabelField(new Rect(0f, 5f, position.width - 10f, 16f), group.OutputRichTextName, data.GUICache.RichBoldCenteredLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(data.GUICache.ZoomOutOfLargeViewContent, GUILayout.MaxWidth(20f), GUILayout.MaxHeight(20f)))
            {
                viewMode = 0;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Resolution - " + group.TargetWidth + " * " + group.TargetHeight, data.GUICache.BoldRightLabel);
            EditorGUILayout.EndHorizontal();

            DrawOutputEvent(new Rect(5f, 25f, position.width - 20f, position.height - 60f), group);

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            DrawPreviewSettings(group.OutputSlot);

            GUILayout.FlexibleSpace();
            if (EditorGUILayout.DropdownButton(new GUIContent("Image"), FocusType.Passive, GUILayout.MaxWidth(55f)))
            {
                GenericMenu menu = GetOutputGenericMenu(group);
                menu.DropDown(uvButtonRect);
            }
            if (Event.current.type == EventType.Repaint)
                uvButtonRect = GUILayoutUtility.GetLastRect();

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        void DrawOutputEvent(Rect rect, PackingGroup group)
        {
            GUI.BeginGroup(rect);
            switch (Event.current.type)
            {
                case EventType.Repaint:
                    EditorGUI.DrawRect(new Rect(0f, 0f, rect.width, rect.height), new Color(0f, 0f, 0f, 1f));
                    if (group.OutputSlot.Image)
                    {
                        EditorGUI.DrawTextureTransparent(new Rect(0f, 0f, rect.width, rect.height), group.OutputSlot.Image.PreviewRenderTexture, ScaleMode.ScaleToFit);
                    }
                    group.OutputSlot.UvDisplay.TryDrawWireframeUvs(group.TargetWidth, group.TargetHeight, new Rect(0f, 0f, rect.width, rect.height));
                    break;
                case EventType.ContextClick:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        GenericMenu menu = GetOutputGenericMenu(group);
                        menu.ShowAsContext();
                    }
                    break;
            }
            GUI.EndGroup();
        }
        void DrawInputEvent(Rect rect, PackingGroup group, int inputIndex)
        {
            GUI.BeginGroup(rect);
            switch (Event.current.type)
            {
                case EventType.Repaint:
                    EditorGUI.DrawRect(new Rect(0f, 0f, rect.width, rect.height), new Color(0f, 0f, 0f, 1f));
                    if (group.InputSlots[inputIndex].Image)
                    {
                        EditorGUI.DrawTextureTransparent(new Rect(0f, 0f, rect.width, rect.height), group.InputSlots[inputIndex].Image.PreviewRenderTexture, ScaleMode.ScaleToFit);
                    }
                    group.InputSlots[inputIndex].UvDisplay.TryDrawWireframeUvs(group.TargetWidth, group.TargetHeight, new Rect(0f, 0f, rect.width, rect.height));
                    break;
                case EventType.DragUpdated:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    }
                    break;
                case EventType.DragPerform:
                    string[] paths = DragAndDrop.paths;
                    if (paths != null && paths.Length > 0)
                    {
                        if (group.LoadImageFromPath(paths[0], inputIndex))
                        {
                            DragAndDrop.AcceptDrag();
                        }
                    }
                    break;
                case EventType.ContextClick:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        GenericMenu menu = GetInputGenericMenu(group, inputIndex);
                        menu.ShowAsContext();
                    }
                    break;
            }
            GUI.EndGroup();
        }
        void AddInputSlotMenuItems(GenericMenu menu, PackingGroup group, int inputIndex)
        {
            UtilityData data = ChannelPackUtility.GetData();
            InputSlot slot = group.InputSlots[inputIndex];

            menu.AddItem(new GUIContent("Load..."), false, LoadImage, new CreateImageContext(group, inputIndex, null));
            menu.AddSeparator(string.Empty);

            if (slot.ImageGenerator)
                menu.AddItem(new GUIContent("Edit..."), false, ShowImageGeneratorWindow, new CreateImageContext(group, inputIndex, data.GetGeneratorAttribute(slot.ImageGenerator)));
            else
                menu.AddDisabledItem(new GUIContent("Edit..."));

            for (int i = 0; i < data.GeneratorAttributes.Length; i++)
            {
                bool selected = false;
                if (slot.ImageGenerator)
                    selected = data.GeneratorAttributes[i].Type == slot.ImageGenerator.GetType();
                menu.AddItem(data.GeneratorAttributes[i].MenuContext, selected, ShowImageGeneratorWindow, new CreateImageContext(group, inputIndex, data.GeneratorAttributes[i]));
            }
        }
        void AddImageSlotMenuItems(GenericMenu menu, PackingGroup group, ImageSlot slot)
        {
            UvDisplay display = slot.UvDisplay;

            menu.AddItem(new GUIContent("Uvs/Mesh..."), false, ShowMeshSelectWindow, new GroupSlotContext(group, slot));
            menu.AddSeparator("Uvs/");
            menu.AddItem(new GUIContent("Uvs/UV0"), display.uvSetIndex == 0, SetUvSetIndex, new ImageSlotUvDisplayContext(group, slot, 0));
            menu.AddItem(new GUIContent("Uvs/UV1"), display.uvSetIndex == 1, SetUvSetIndex, new ImageSlotUvDisplayContext(group, slot, 1));
            menu.AddItem(new GUIContent("Uvs/UV2"), display.uvSetIndex == 2, SetUvSetIndex, new ImageSlotUvDisplayContext(group, slot, 2));
            menu.AddItem(new GUIContent("Uvs/UV3"), display.uvSetIndex == 3, SetUvSetIndex, new ImageSlotUvDisplayContext(group, slot, 3));
            menu.AddItem(new GUIContent("Uvs/UV4"), display.uvSetIndex == 4, SetUvSetIndex, new ImageSlotUvDisplayContext(group, slot, 4));
            menu.AddItem(new GUIContent("Uvs/UV5"), display.uvSetIndex == 5, SetUvSetIndex, new ImageSlotUvDisplayContext(group, slot, 5));
            menu.AddItem(new GUIContent("Uvs/UV6"), display.uvSetIndex == 6, SetUvSetIndex, new ImageSlotUvDisplayContext(group, slot, 6));
            menu.AddItem(new GUIContent("Uvs/UV7"), display.uvSetIndex == 7, SetUvSetIndex, new ImageSlotUvDisplayContext(group, slot, 7));
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Postprocess..."), false, ShowImagePostprocessorWindow, new GroupSlotContext(group, slot));
        }
        GenericMenu GetInputGenericMenu(PackingGroup group, int inputIndex)
        {
            GenericMenu menu = new GenericMenu();
            AddInputSlotMenuItems(menu, group, inputIndex);
            menu.AddSeparator(string.Empty);
            AddImageSlotMenuItems(menu, group, group.InputSlots[inputIndex]);
            return menu;
        }
        GenericMenu GetOutputGenericMenu(PackingGroup group)
        {
            GenericMenu menu = new GenericMenu();
            AddImageSlotMenuItems(menu, group, group.OutputSlot);
            return menu;
        }
        GenericMenu GetExportFormatsMenu(PackingGroup group)
        {
            UtilityData data = ChannelPackUtility.GetData();
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < data.ExportFormats.Length; i++)
            {
                bool on = group.extensionIndex == i;
                menu.AddItem(new GUIContent(data.ExportFormats[i]), on, ApplyGroupExtensionIndex, new GroupExtension(group, i));
            }
            return menu;
        }
        GenericMenu GetExportButtonMenu(PackingGroup group)
        {
            GenericMenu menu = new GenericMenu();
            UtilityData data = ChannelPackUtility.GetData();
            if (group.OutputSlot.Image)
                menu.AddItem(new GUIContent(@"Export """ + group.OutputName + @""""), false, ExportCurrent);
            else
                menu.AddDisabledItem(new GUIContent(@"Export """ + group.OutputName + @""""), false);

            if (data.CanBatchExport())
                menu.AddItem(new GUIContent("Export Batch"), false, ExportBatch);
            else
                menu.AddDisabledItem(new GUIContent("Export Batch"), false);
            return menu;
        }
        void ExportCurrent()
        {
            UtilityData data = ChannelPackUtility.GetData();
            PackingGroup group = data.GetSelectedPackingGroup();

            string path = EditorUtility.SaveFilePanel("Export " + group.OutputName, data.Preferences.lastLoadSaveDirectory, group.OutputName, data.ExportFormats[group.extensionIndex].Remove(0, 1));
            if (path == string.Empty)
                return;
            data.Preferences.lastLoadSaveDirectory = Path.GetDirectoryName(path);

            string extension = Path.GetExtension(path);
            if (ChannelPackUtility.SupportedExportFormat(extension))
            {
                if (UpdateOutputBeforeExportDialog("Update " + group.OutputName, "Auto update is off, Would you like to update the output image before exporting."))
                    group.PackInputToOutputAll(true);

                byte[] bytes = ChannelPackUtility.GenerateOutput(group.OutputSlot, extension);
                if (bytes != null)
                {
                    File.WriteAllBytes(path, bytes);
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Failed To Export Image", "The file extension " + extension + " is not supported, please use " + ChannelPackUtility.GetSupportedExportFormatsLog(), "Ok");
            }
        }
        void ExportBatch()
        {
            UtilityData data = ChannelPackUtility.GetData();
            List<PackingGroup> exportableGroups = new List<PackingGroup>();

            int groupCount = data.PackingGroupCount();
            for (int i = 0; i < groupCount; i++)
            {
                PackingGroup group = data.GetPackingGroup(i);
                if (group.OutputSlot.Image)
                    exportableGroups.Add(group);
            }

            groupCount = exportableGroups.Count;
            if (groupCount > 0)
            {
                string folder = EditorUtility.SaveFolderPanel("Batch Export", data.Preferences.lastLoadSaveDirectory, "");
                if (folder == string.Empty)
                {
                    exportableGroups.Clear();
                    return;
                }
                data.Preferences.lastLoadSaveDirectory = folder;

                bool updateBeforeExport = UpdateOutputBeforeExportDialog("Update all Output Images", "Auto update is off, Would you like to update all output images before exporting.");
                for (int i = 0; i < groupCount; i++)
                {
                    if (updateBeforeExport)
                        exportableGroups[i].PackInputToOutputAll(true);

                    string extension = data.ExportFormats[exportableGroups[i].extensionIndex];
                    byte[] bytes = ChannelPackUtility.GenerateOutput(exportableGroups[i].OutputSlot, extension);
                    if (bytes != null)
                    {
                        string path = Path.Combine(folder, exportableGroups[i].OutputName) + extension;
                        File.WriteAllBytes(path, bytes);
                    }
                }
                exportableGroups.Clear();
                AssetDatabase.Refresh();
            }
        }
        void DelayOpenWithTemplateCall()
        {
            if (openWithTemplate)
            {
                ChannelPackUtility.GetData().SetTemplate(openWithTemplate);
                openWithTemplate = null;
            }
        }
        void LoadImage(object groupContext)
        {
            CreateImageContext context = (CreateImageContext)groupContext;
            string path = EditorUtility.OpenFilePanel("Load Image", ChannelPackUtility.GetData().Preferences.lastLoadSaveDirectory, ChannelPackUtility.GetSupportedLoadFormatsFilter());
            context.Group.LoadImageFromPath(path, context.InputIndex);
        }
        void ShowImageGeneratorWindow(object generatorContext)
        {
            CreateImageContext context = (CreateImageContext)generatorContext;
            ImageGeneratorWindow.ShowImageGeneratorAuxWindow(context.Group, context.InputIndex, context.Attribute, this);
        }
        void ShowImagePostprocessorWindow(object postprocessorContext)
        {
            GroupSlotContext context = (GroupSlotContext)postprocessorContext;
            PostprocessorWindow.ShowPostprocessAuxWindow(context.Group, context.Slot, this);
        }
        void ShowMeshSelectWindow(object uvDisplayContext)
        {
            GroupSlotContext context = (GroupSlotContext)uvDisplayContext;
            MeshSelectWindow.ShowMeshSelectAuxWindow(context.Group, context.Slot, this);
        }
        void SetUvSetIndex(object uvDisplayContext)
        {
            ImageSlotUvDisplayContext context = (ImageSlotUvDisplayContext)uvDisplayContext;
            context.SetSlotUvDisplayIndex();
            Repaint();
        }
        bool UpdateOutputBeforeExportDialog(string title, string message)
        {
            UtilityData data = ChannelPackUtility.GetData();
            if (!data.Preferences.autoUpdate)
                return EditorUtility.DisplayDialog(title, message, "Yes", "No");
            return false;
        }
    }
}