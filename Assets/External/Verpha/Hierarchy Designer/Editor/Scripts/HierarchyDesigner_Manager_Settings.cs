#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    public static class HierarchyDesigner_Manager_Settings
    {
        #region Keys
        private const string ShowMainIconOfGameObjectKey = "HierarchyDesigner_ShowMainIconOfGameObject";
        private const string ShowComponentIconsKey = "HierarchyDesigner_ShowComponentIcons";
        private const string ShowTransformComponentKey = "HierarchyDesigner_ShowTransformComponent";
        private const string ShowComponentIconsForFoldersKey = "HierarchyDesigner_ShowComponentIconsForFolders";
        private const string ShowHierarchyTreeKey = "HierarchyDesigner_ShowHierarchyTree";
        private const string ShowTagKey = "HierarchyDesigner_ShowTag";
        private const string ShowLayerKey = "HierarchyDesigner_ShowLayer";
        private const string DisableHierarchyDesignerDuringPlayModeKey = "HierarchyDesigner_DisableDuringPlayMode";
        private const string TagTextColorKey = "HierarchyDesigner_TagTextColor";
        private const string TagFontStyleKey = "HierarchyDesigner_TagFontStyle";
        private const string TagFontSizeKey = "HierarchyDesigner_TagFontSize";
        private const string LayerTextColorKey = "HierarchyDesigner_LayerTextColor";
        private const string LayerFontStyleKey = "HierarchyDesigner_LayerFontStyle";
        private const string LayerFontSizeKey = "HierarchyDesigner_LayerFontSize";
        private const string TreeColorKey = "HierarchyDesigner_TreeColor";
        private const string EnableDisableShortcutKey = "HierarchyDesigner_EnableDisableShortcut";
        private const string LockUnlockShortcutKey = "HierarchyDesigner_LockUnlockShortcut";
        private const string ChangeTagAndLayerShortcutKey = "HierarchyDesigner_ChangeTagAndLayerShortcut";
        private const string RenameGameObjectsShortcutKey = "HierarchyDesigner_RenameGameObjectsShortcut";
        #endregion
        #region General Settings
        private static bool showMainIconOfGameObject = EditorPrefs.GetBool(ShowMainIconOfGameObjectKey, true);
        private static bool showComponentIcons = EditorPrefs.GetBool(ShowComponentIconsKey, true);
        private static bool showTransformComponent = EditorPrefs.GetBool(ShowTransformComponentKey, true);
        private static bool showComponentIconsForFolders = EditorPrefs.GetBool(ShowComponentIconsForFoldersKey, true);
        private static bool showTag = EditorPrefs.GetBool(ShowTagKey, true);
        private static bool showLayer = EditorPrefs.GetBool(ShowLayerKey, true);
        private static bool showHierarchyTree = EditorPrefs.GetBool(ShowHierarchyTreeKey, true);
        private static bool disableHierarchyDesignerDuringPlayMode = EditorPrefs.GetBool(DisableHierarchyDesignerDuringPlayModeKey, true);
        #endregion
        #region Styling
        private static Color tagTextColor = HierarchyDesigner_Shared_ColorUtility.ParseColor(EditorPrefs.GetString(TagTextColorKey, HierarchyDesigner_Shared_ColorUtility.ColorToString(Color.gray)));
        private static FontStyle tagFontStyle = (FontStyle)EditorPrefs.GetInt(TagFontStyleKey, (int)FontStyle.BoldAndItalic);
        private static int tagFontSize = EditorPrefs.GetInt(TagFontSizeKey, 9);
        private static Color layerTextColor = HierarchyDesigner_Shared_ColorUtility.ParseColor(EditorPrefs.GetString(LayerTextColorKey, HierarchyDesigner_Shared_ColorUtility.ColorToString(Color.gray)));
        private static FontStyle layerFontStyle = (FontStyle)EditorPrefs.GetInt(LayerFontStyleKey, (int)FontStyle.BoldAndItalic);
        private static int layerFontSize = EditorPrefs.GetInt(LayerFontSizeKey, 9);
        private static Color treeColor = HierarchyDesigner_Shared_ColorUtility.ParseColor(EditorPrefs.GetString(TreeColorKey, HierarchyDesigner_Shared_ColorUtility.ColorToString(Color.white)));
        #endregion
        #region Major Shortcuts
        private static KeyCode enableDisableShortcut = (KeyCode)EditorPrefs.GetInt(EnableDisableShortcutKey, (int)KeyCode.Mouse2);
        private static KeyCode lockUnlockShortcutShortcut = (KeyCode)EditorPrefs.GetInt(LockUnlockShortcutKey, (int)KeyCode.F1);
        private static KeyCode changeTagAndLayerShortcut = (KeyCode)EditorPrefs.GetInt(ChangeTagAndLayerShortcutKey, (int)KeyCode.F2);
        private static KeyCode renameGameObjectsShortcut = (KeyCode)EditorPrefs.GetInt(RenameGameObjectsShortcutKey, (int)KeyCode.F3);
        #endregion

        #region Accessors
        public static bool ShowMainIconOfGameObject
        {
            get => showMainIconOfGameObject;
            set => SetAndSave(ref showMainIconOfGameObject, value, ShowMainIconOfGameObjectKey);
        }

        public static bool ShowComponentIcons
        {
            get => showComponentIcons;
            set => SetAndSave(ref showComponentIcons, value, ShowComponentIconsKey);
        }

        public static bool ShowTransformComponent
        {
            get => showTransformComponent;
            set => SetAndSave(ref showTransformComponent, value, ShowTransformComponentKey);
        }

        public static bool ShowComponentIconsForFolders
        {
            get => showComponentIconsForFolders;
            set => SetAndSave(ref showComponentIconsForFolders, value, ShowComponentIconsForFoldersKey);
        }

        public static bool ShowTag
        {
            get => showTag;
            set => SetAndSave(ref showTag, value, ShowTagKey);
        }

        public static bool ShowLayer
        {
            get => showLayer;
            set => SetAndSave(ref showLayer, value, ShowLayerKey);
        }

        public static bool ShowHierarchyTree
        {
            get => showHierarchyTree;
            set => SetAndSave(ref showHierarchyTree, value, ShowHierarchyTreeKey);
        }

        public static bool DisableHierarchyDesignerDuringPlayMode
        {
            get => disableHierarchyDesignerDuringPlayMode;
            set => SetAndSave(ref disableHierarchyDesignerDuringPlayMode, value, DisableHierarchyDesignerDuringPlayModeKey);
        }

        public static Color TagTextColor
        {
            get => tagTextColor;
            set
            {
                if (tagTextColor != value)
                {
                    tagTextColor = value;
                    EditorPrefs.SetString(TagTextColorKey, HierarchyDesigner_Shared_ColorUtility.ColorToString(value));
                }
            }
        }

        public static FontStyle TagFontStyle
        {
            get => tagFontStyle;
            set
            {
                if (tagFontStyle != value)
                {
                    tagFontStyle = value;
                    EditorPrefs.SetInt(TagFontStyleKey, (int)value);
                }
            }
        }

        public static int TagFontSize
        {
            get => tagFontSize;
            set
            {
                if (tagFontSize != value)
                {
                    tagFontSize = value;
                    EditorPrefs.SetInt(TagFontSizeKey, value);
                }
            }
        }

        public static Color LayerTextColor
        {
            get => layerTextColor;
            set
            {
                if (layerTextColor != value)
                {
                    layerTextColor = value;
                    EditorPrefs.SetString(LayerTextColorKey, HierarchyDesigner_Shared_ColorUtility.ColorToString(value));
                }
            }
        }

        public static FontStyle LayerFontStyle
        {
            get => layerFontStyle;
            set
            {
                if (layerFontStyle != value)
                {
                    layerFontStyle = value;
                    EditorPrefs.SetInt(LayerFontStyleKey, (int)value);
                }
            }
        }

        public static int LayerFontSize
        {
            get => layerFontSize;
            set
            {
                if (layerFontSize != value)
                {
                    layerFontSize = value;
                    EditorPrefs.SetInt(LayerFontSizeKey, value);
                }
            }
        }

        public static Color TreeColor
        {
            get => treeColor;
            set
            {
                if (treeColor != value)
                {
                    treeColor = value;
                    EditorPrefs.SetString(TreeColorKey, HierarchyDesigner_Shared_ColorUtility.ColorToString(value));
                }
            }
        }

        public static KeyCode EnableDisableShortcut
        {
            get => enableDisableShortcut;
            set => SetAndSave(ref enableDisableShortcut, value, EnableDisableShortcutKey);
        }

        public static KeyCode LockUnlockShortcut
        {
            get => lockUnlockShortcutShortcut;
            set => SetAndSave(ref lockUnlockShortcutShortcut, value, LockUnlockShortcutKey);
        }

        public static KeyCode ChangeTagAndLayerShortcut
        {
            get => changeTagAndLayerShortcut;
            set => SetAndSave(ref changeTagAndLayerShortcut, value, ChangeTagAndLayerShortcutKey);
        }

        public static KeyCode RenameGameObjectsShortcut
        {
            get => renameGameObjectsShortcut;
            set => SetAndSave(ref renameGameObjectsShortcut, value, RenameGameObjectsShortcutKey);
        }
        #endregion

        private static void SetAndSave(ref bool field, bool value, string key)
        {
            if (field != value)
            {
                field = value;
                EditorPrefs.SetBool(key, value);
            }
        }

        private static void SetAndSave<T>(ref T field, T value, string key)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                EditorPrefs.SetInt(key, Convert.ToInt32(value));
            }
        }
    }
}
#endif