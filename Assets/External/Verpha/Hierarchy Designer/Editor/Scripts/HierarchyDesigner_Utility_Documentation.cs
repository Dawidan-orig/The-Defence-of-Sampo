#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Verpha.HierarchyDesigner
{
    public static class HierarchyDesigner_Utility_Documentation
    {
        private const string DocumentationFileName = "Hierarchy Designer Documentation.pdf";

        [MenuItem("Hierarchy Designer/Hierarchy Designer Documentation", false, 150)]
        private static void OpenDocumentation()
        {
            string assetsPath = Application.dataPath;
            string searchPattern = "Verpha" + Path.DirectorySeparatorChar +
                                   "Hierarchy Designer" + Path.DirectorySeparatorChar +
                                   "Editor" + Path.DirectorySeparatorChar +
                                   "Documentation" + Path.DirectorySeparatorChar +
                                   DocumentationFileName;

            string[] allFiles = Directory.GetFiles(assetsPath, DocumentationFileName, SearchOption.AllDirectories);
            List<string> filteredFiles = new List<string>();

            foreach (string file in allFiles)
            {
                if (file.EndsWith(searchPattern))
                {
                    filteredFiles.Add(file);
                }
            }

            if (filteredFiles.Count > 0)
            {
                try
                {
                    Process.Start(filteredFiles[0]);
                }
                catch (System.Exception ex)
                {
                    EditorUtility.DisplayDialog("Error", $"Failed to open documentation: {ex.Message}", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Documentation Not Found", $"Documentation file not found: {DocumentationFileName}", "OK");
            }
        }
    }
}
#endif