#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    public class HierarchyFolderWindow : EditorWindow
    {
        #region OnGUI Properties
        private Vector2 scrollPositionOuter;
        private Vector2 scrollPositionInner;
        private static bool hasModifiedChanges = false;
        private GUIStyle customSettingsStyle;
        private GUIStyle customSettingsStyleSecondary;
        private GUIStyle headerLabelStyle;
        private GUIStyle contentLabelStyle;
        private GUIStyle utilityLabelStyle;
        #endregion
        #region Folder Properties
        private string newFolderName = "";
        private Color newFolderIconColor = Color.white;
        private HierarchyDesigner_Info_Folder.FolderImageType newFolderImageType = HierarchyDesigner_Info_Folder.FolderImageType.Default;
        private const string FolderPrefKey = "HierarchyFolders";
        public static Dictionary<string, HierarchyDesigner_Info_Folder> folders = new Dictionary<string, HierarchyDesigner_Info_Folder>();
        #endregion
        #region Global Fields
        private Color tempGlobalFolderIconColor = Color.white;
        private HierarchyDesigner_Info_Folder.FolderImageType tempGlobalFolderImageType = HierarchyDesigner_Info_Folder.FolderImageType.Default;
        #endregion

        [MenuItem("Hierarchy Designer/Hierarchy Folder/Hierarchy Folder Manager")]
        public static void OpenWindow()
        {
            LoadFolders();
            HierarchyFolderWindow window = GetWindow<HierarchyFolderWindow>("Hierarchy Folder Manager");
            window.minSize = new Vector2(300, 150);
        }

        private void InitializeStyles()
        {
            customSettingsStyle = HierarchyDesigner_Info_OnGUI.CreateCustomStyle();
            customSettingsStyleSecondary = HierarchyDesigner_Info_OnGUI.CreateCustomStyle(true);
            headerLabelStyle = HierarchyDesigner_Info_OnGUI.HeaderLabelStyle;
            contentLabelStyle = HierarchyDesigner_Info_OnGUI.ContentLabelStyle;
            utilityLabelStyle = HierarchyDesigner_Info_OnGUI.UtilityLabelStyle;
        }

        private void OnGUI()
        {
            InitializeStyles();
            EditorGUILayout.BeginVertical(customSettingsStyle);

            GUILayout.Space(4);
            EditorGUILayout.LabelField("Folders Manager", headerLabelStyle);
            GUILayout.Space(8);

            scrollPositionOuter = EditorGUILayout.BeginScrollView(scrollPositionOuter, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            #region Folder Creation Fields
            using (new HierarchyDesigner_Info_OnGUI.LabelWidth(60))
            {
                newFolderName = EditorGUILayout.TextField("Name", newFolderName);
                newFolderIconColor = EditorGUILayout.ColorField("Color", newFolderIconColor);
                newFolderImageType = (HierarchyDesigner_Info_Folder.FolderImageType)EditorGUILayout.EnumPopup("Image", newFolderImageType);
            }
            #endregion

            #region Add Folder
            GUILayout.Space(10);
            if (GUILayout.Button("Add Folder", GUILayout.Height(25)))
            {
                if (IsFolderNameValid(newFolderName))
                {
                    HierarchyDesigner_Info_Folder newFolder = new HierarchyDesigner_Info_Folder(newFolderName, newFolderIconColor, newFolderImageType);
                    folders[newFolderName] = newFolder;
                    newFolderName = "";
                    GUI.FocusControl(null);
                    hasModifiedChanges = true;
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder Name", "Folder name is either duplicate or invalid.", "OK");
                }
            }
            GUILayout.Space(5);
            #endregion

            #region Folders List
            float maxWidth = HierarchyDesigner_Info_OnGUI.CalculateMaxLabelWidth(folders.Keys);

            if (folders.Count > 0)
            {
                #region Global Fields
                GUILayout.Space(2);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Folders' Global Fields", utilityLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Space(6);

                EditorGUILayout.BeginHorizontal();
                #region Changes Check
                EditorGUI.BeginChangeCheck();
                tempGlobalFolderIconColor = EditorGUILayout.ColorField(tempGlobalFolderIconColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (HierarchyDesigner_Info_Folder folder in folders.Values)
                    {
                        folder.FolderColor = tempGlobalFolderIconColor;
                    }
                    hasModifiedChanges = true;
                }

                EditorGUI.BeginChangeCheck();
                tempGlobalFolderImageType = (HierarchyDesigner_Info_Folder.FolderImageType)EditorGUILayout.EnumPopup(tempGlobalFolderImageType, GUILayout.Width(258));
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (HierarchyDesigner_Info_Folder folder in folders.Values)
                    {
                        folder.ImageType = tempGlobalFolderImageType;
                    }
                    hasModifiedChanges = true;
                }
                #endregion
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                GUILayout.Space(10);
                #endregion

                EditorGUILayout.BeginVertical(customSettingsStyleSecondary);

                GUILayout.Space(2);
                EditorGUILayout.LabelField("Folders’ List", contentLabelStyle);
                GUILayout.Space(10);

                scrollPositionInner = EditorGUILayout.BeginScrollView(scrollPositionInner, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                foreach (var folderEntry in folders)
                {
                    string key = folderEntry.Key;
                    HierarchyDesigner_Info_Folder folder = folderEntry.Value;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.LabelField(folder.Name, GUILayout.Width(maxWidth));
                    folder.FolderColor = EditorGUILayout.ColorField(folder.FolderColor, GUILayout.MinWidth(200), GUILayout.ExpandWidth(true));
                    folder.ImageType = (HierarchyDesigner_Info_Folder.FolderImageType)EditorGUILayout.EnumPopup(folder.ImageType, GUILayout.Width(125));
                    if (EditorGUI.EndChangeCheck())
                    {
                        hasModifiedChanges = true;
                    }

                    if (GUILayout.Button("Create", GUILayout.Width(60)))
                    {
                        HierarchyDesigner_Utility_Folder.CreateFolder(folder);
                    }
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        folders.Remove(key);
                        hasModifiedChanges = true;
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(5);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                GUILayout.Space(2);
                if (GUILayout.Button("Update Folders", GUILayout.Height(30)))
                {
                    HierarchyDesigner_Visual_Folder.UpdateFolderVisuals();
                }
                GUILayout.Space(2);
                if (GUILayout.Button("Save Folders", GUILayout.Height(30)))
                {
                    SaveFolders();
                    HierarchyDesigner_Visual_Folder.UpdateFolderVisuals();
                }
                GUILayout.Space(5);
            }
            #endregion

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private bool IsFolderNameValid(string folderName)
        {
            return !string.IsNullOrEmpty(folderName) && !folders.ContainsKey(folderName);
        }

        public static void SaveFolders()
        {
            List<string> serializedParts = new List<string>();
            foreach (var folder in folders)
            {
                HierarchyDesigner_Info_Folder f = folder.Value;
                serializedParts.Add($"{f.Name},{HierarchyDesigner_Shared_ColorUtility.ColorToString(f.FolderColor)},{f.ImageType}");
            }
            string serialized = string.Join(";", serializedParts);
            EditorPrefs.SetString(FolderPrefKey, serialized);
            hasModifiedChanges = false;
        }

        public static void LoadFolders()
        {
            string serialized = EditorPrefs.GetString(FolderPrefKey, "");
            folders.Clear();

            foreach (string serializedFolder in serialized.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = serializedFolder.Split(',');
                if (parts.Length == 3)
                {
                    string name = parts[0];
                    Color iconColor = HierarchyDesigner_Shared_ColorUtility.ParseColor(parts[1]);
                    HierarchyDesigner_Info_Folder.FolderImageType folderImageType = HierarchyDesigner_Shared_EnumFilter.ParseEnum(parts[2], HierarchyDesigner_Info_Folder.FolderImageType.Default);
                    folders[name] = new HierarchyDesigner_Info_Folder(name, iconColor, folderImageType);
                }
            }
            hasModifiedChanges = false;
        }

        private void OnDestroy()
        {
            if (hasModifiedChanges)
            {
                bool shouldSave = EditorUtility.DisplayDialog("Folder(s) Have Been Modified",
                    "Do you want to save the changes you made in the folders?",
                    "Save", "Don't Save");

                if (shouldSave)
                {
                    SaveFolders();
                }
            }
            hasModifiedChanges = false;
        }
    }

    public class HierarchyDesigner_Utility_Folder
    {
        #region Default Folder
        [MenuItem("Hierarchy Designer/Hierarchy Folder/Create Default Folder", false, 2)]
        public static void CreateDefaultFolder()
        {
            CreateFolderObject("New Folder", HierarchyDesigner_Info_Folder.FolderImageType.Default);
        }

        private static void CreateFolderObject(string folderName, HierarchyDesigner_Info_Folder.FolderImageType folderImageType)
        {
            GameObject folder = new GameObject(folderName);
            folder.AddComponent<HierarchyDesignerFolder>();

            Texture2D folderIcon = HierarchyDesigner_Manager_Folder.GetFolderIcon(folderImageType);
            if (folderIcon != null)
            {
                EditorGUIUtility.SetIconForObject(folder, folderIcon);
            }

            Undo.RegisterCreatedObjectUndo(folder, $"Create {folderName}");
            EditorGUIUtility.PingObject(folder);
        }
        #endregion

        [MenuItem("Hierarchy Designer/Hierarchy Folder/Create All Folders", false, 1)]
        public static void CreateAllFoldersFromList()
        {
            foreach (HierarchyDesigner_Info_Folder folderInfo in HierarchyFolderWindow.folders.Values)
            {
                CreateFolder(folderInfo);
            }
        }

        [MenuItem("Hierarchy Designer/Hierarchy Folder/Create Missing Folders", false, 2)]
        public static void CreateMissingFolders()
        {
            foreach (HierarchyDesigner_Info_Folder folderInfo in HierarchyFolderWindow.folders.Values)
            {
                if (!FolderExists(folderInfo.Name))
                {
                    CreateFolder(folderInfo);
                }
            }
        }

        public static void CreateFolder(HierarchyDesigner_Info_Folder folderInfo)
        {
            GameObject folder = new GameObject(folderInfo.Name);

            folder.AddComponent<HierarchyDesignerFolder>();

            Undo.RegisterCreatedObjectUndo(folder, $"Create {folderInfo.Name}");

            Texture2D inspectorIcon = HierarchyDesigner_Manager_Folder.InspectorFolderIcon;
            if (inspectorIcon != null)
            {
                EditorGUIUtility.SetIconForObject(folder, inspectorIcon);
            }

            EditorGUIUtility.PingObject(folder);
        }

        private static bool FolderExists(string folderName)
        {
            HierarchyDesignerFolder[] allFolders = UnityEngine.Object.FindObjectsOfType<HierarchyDesignerFolder>(includeInactive: true);
            foreach (HierarchyDesignerFolder folder in allFolders)
            {
                if (folder.gameObject.name.Equals(folderName, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
#endif