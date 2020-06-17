using System;
using UnityEditor.Compilation;
using UnityEngine;
using System.IO;

namespace UnityEditor.AssemblyTools
{
    /// <summary>
    /// Copied from the internal class. We need to keep this in sync if it changes in the future.
    /// https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html
    /// </summary>
    [Serializable]
    public class AssemblyDefinitionFile
    {
        public string name;
        public string[] references;
        public string[] optionalUnityReferences;
        public string[] includePlatforms;
        public string[] excludePlatforms;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public bool autoReferenced;
        public string[] defineConstraints;

        public static AssemblyDefinitionFile FromJson(string json)
        {
            AssemblyDefinitionFile scriptAssemblyData = new AssemblyDefinitionFile();
            scriptAssemblyData.autoReferenced = true;
            JsonUtility.FromJsonOverwrite(json, (object) scriptAssemblyData);
            if (scriptAssemblyData == null)
                throw new Exception("Json file does not contain an assembly definition");
            if (string.IsNullOrEmpty(scriptAssemblyData.name))
                throw new Exception("Required property 'name' not set");
            if (scriptAssemblyData.excludePlatforms != null && scriptAssemblyData.excludePlatforms.Length > 0 && (scriptAssemblyData.includePlatforms != null && scriptAssemblyData.includePlatforms.Length > 0))
                throw new Exception("Both 'excludePlatforms' and 'includePlatforms' are set.");
            return scriptAssemblyData;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
    }

    public static class AssemblyExtensions
    {
        const string k_TestReference = "TestAssemblies";

        public static AssemblyDefinitionFile GetAssemblyDefinitionFile(this Assembly assembly)
        {
            var assetAsmdefFile = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
            return string.IsNullOrEmpty(assetAsmdefFile) ? null : AssemblyDefinitionFile.FromJson(File.ReadAllText(assetAsmdefFile));
        }

        public static bool IsTestAssembly(this Assembly assembly)
        {
            var asmdef = GetAssemblyDefinitionFile(assembly);
            if (asmdef != null && asmdef.optionalUnityReferences != null && asmdef.optionalUnityReferences.Length > 0)
            {
                foreach (var asmdefOptionalUnityReference in asmdef.optionalUnityReferences)
                {
                    if (asmdefOptionalUnityReference == k_TestReference)
                        return true;
                }
            }

            return false;
        }

        public static void SaveChanges(this Assembly assembly, AssemblyDefinitionFile data)
        {
            var assetAsmdefFile = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
            File.WriteAllText(assetAsmdefFile, data.ToJson());
            AssetDatabase.ImportAsset(assetAsmdefFile);
        }

        public static bool IsReadOnly(this Assembly assembly)
        {
            var assetAsmdefFile = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
            if (string.IsNullOrEmpty(assetAsmdefFile))
                return true;
            var file = Path.Combine(Application.dataPath.Replace("/Assets", ""), assetAsmdefFile);
            return (File.GetAttributes(file) & FileAttributes.ReadOnly) != 0;
        }
    }
}