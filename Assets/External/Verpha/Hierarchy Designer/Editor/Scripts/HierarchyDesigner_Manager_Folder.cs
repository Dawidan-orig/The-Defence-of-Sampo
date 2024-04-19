#if UNITY_EDITOR
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    public static class HierarchyDesigner_Manager_Folder
    {
        private static readonly string inspectorFolderIconPath = "Hierarchy Designer Folder Icon Inspector";

        public static Texture2D GetFolderIcon(HierarchyDesigner_Info_Folder.FolderImageType folderImageType)
        {
            switch (folderImageType)
            {
                case HierarchyDesigner_Info_Folder.FolderImageType.DefaultOutline:
                    return HierarchyDesigner_Shared_TextureLoader.LoadTexture("Hierarchy Designer Folder Icon Outline");
                case HierarchyDesigner_Info_Folder.FolderImageType.DefaultOutline2X:
                    return HierarchyDesigner_Shared_TextureLoader.LoadTexture("Hierarchy Designer Folder Icon Outline Double");
                default:
                    return HierarchyDesigner_Shared_TextureLoader.LoadTexture("Hierarchy Designer Folder Icon");
            }
        }

        public static Texture2D InspectorFolderIcon
        {
            get { return HierarchyDesigner_Shared_TextureLoader.LoadTexture(inspectorFolderIconPath); }
        }
    }
}
#endif