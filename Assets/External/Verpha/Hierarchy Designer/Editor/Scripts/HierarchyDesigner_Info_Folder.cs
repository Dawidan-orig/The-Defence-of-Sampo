#if UNITY_EDITOR
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    [System.Serializable]
    public class HierarchyDesigner_Info_Folder
    {
        #region Properties
        private string name = "Folder";
        private Color folderColor = Color.white;
        private FolderImageType folderImageType = FolderImageType.Default;
        #endregion

        #region Accessors
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Color FolderColor
        {
            get { return folderColor; }
            set { folderColor = value; }
        }

        public FolderImageType ImageType
        {
            get { return folderImageType; }
            set { folderImageType = value; }
        }

        public enum FolderImageType
        {
            Default,
            DefaultOutline,
            DefaultOutline2X
        }
        #endregion

        public HierarchyDesigner_Info_Folder(string name, Color folderColor, FolderImageType folderImageType)
        {
            this.name = name;
            this.folderColor = folderColor;
            this.folderImageType = folderImageType;
        }
    }
}
#endif