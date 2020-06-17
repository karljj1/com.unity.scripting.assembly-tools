using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Compilation;
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.Animations;

namespace UnityEditor.AssemblyTools
{
    public class AssemblyDefinitionWizard : EditorWindow
    {
        string m_RuntimeDirectory = "Assets/Library/Runtime/";
        string m_EditorDirectory = "Assets/Library/Editor/";
        string m_RuntimeAssemblyName = "Library.Runtime";
        string m_EditorAssemblyName = "Library.Editor";
        string m_SearchRootDirectory = "Assets/";
        UpgradeMode m_UpgradeMode = UpgradeMode.MoveFiles;
        Vector2 m_ScrollPos;

        class CreatedAsmDefDetails
        {
            public AssemblyDefinitionFile asmdef;
            public string path;
            public bool foldout;
        }


        List<CreatedAsmDefDetails> m_CreatedAsmdefs = new List<CreatedAsmDefDetails>();
        List<KeyValuePair<string, string>> m_MovedAssets = new List<KeyValuePair<string, string>>();

        public enum UpgradeMode
        {
            // Split editor files and player files into an editor and player asmdef/directory. Files will be moved. Should provide the best performance...
            MoveFiles,

            // A single runtime asmdef with multple editor asmdefs for each Editor folder. Files are not moved. Non destructive.
            EditorFolderUpgrade
        }

        [MenuItem("Assets/Assembly Definition Helper")]
        public static void ShowWindow()
        {
            var window = (AssemblyDefinitionWizard)GetWindow(typeof(AssemblyDefinitionWizard));
            window.titleContent = new GUIContent("Assembly Definition Helper");
            window.minSize = new Vector2(500, 500);
            window.Show();
        }

        void OnGUI()
        {
            m_UpgradeMode = (UpgradeMode)EditorGUILayout.EnumPopup("Mode", m_UpgradeMode);
            if (m_UpgradeMode == UpgradeMode.MoveFiles)
            {
                m_RuntimeDirectory = EditorGUILayout.TextField("Runtime Directory", m_RuntimeDirectory);
                m_EditorDirectory = EditorGUILayout.TextField("Editor Directory", m_EditorDirectory);
            }

            m_RuntimeAssemblyName = EditorGUILayout.TextField("Runtime Assembly Name", m_RuntimeAssemblyName);
            m_EditorAssemblyName = EditorGUILayout.TextField("Editor Assembly Name", m_EditorAssemblyName);
            m_SearchRootDirectory = EditorGUILayout.TextField("Search Root Directory", m_SearchRootDirectory);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview"))
            {
                GenerateUpgradeSteps();
            }
            if (GUILayout.Button("Upgrade"))
            {

            }
            GUILayout.EndHorizontal();

            if (m_CreatedAsmdefs.Count > 0 || m_MovedAssets.Count > 0)
            {
                m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, EditorStyles.helpBox);
                EditorGUILayout.LabelField("Created Asmdefs:", EditorStyles.boldLabel);
                foreach (var createdAsmdef in m_CreatedAsmdefs)
                {
                    createdAsmdef.foldout = EditorGUILayout.Foldout(createdAsmdef.foldout, createdAsmdef.asmdef.name);
                    if (createdAsmdef.foldout)
                    {
                        EditorGUILayout.LabelField(createdAsmdef.path);
                        EditorGUILayout.LabelField("References:");
                        foreach (var asmdefReference in createdAsmdef.asmdef.references)
                        {
                            EditorGUILayout.LabelField(asmdefReference);
                        }

                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        void GenerateUpgradeSteps()
        {
            m_CreatedAsmdefs.Clear();
            m_MovedAssets.Clear();

            AssemblyDefinitionFile playerAsmdef;
            var foundFiles = Directory.GetFiles(m_SearchRootDirectory, "*.asmdef", SearchOption.TopDirectoryOnly);
            if (foundFiles.Length > 0)
            {
                var json = File.ReadAllText(foundFiles[0]);
                playerAsmdef = AssemblyDefinitionFile.FromJson(json);
            }
            else
            {
                playerAsmdef = new AssemblyDefinitionFile();
                playerAsmdef.name = m_RuntimeAssemblyName;
                playerAsmdef.autoReferenced = true;

                var references = CompilationPipeline.GetAssemblies(AssembliesType.Player);
                var referencesList = new List<string>();
                foreach (var reference in references)
                {
                    referencesList.Add(reference.name);
                }

                playerAsmdef.references = referencesList.ToArray();
                m_CreatedAsmdefs.Add(new CreatedAsmDefDetails()
                {
                    path = Path.Combine(m_SearchRootDirectory, m_RuntimeAssemblyName + ".asmdef"),
                    asmdef = playerAsmdef
                });
            }

            // Find all editor folders.
            var editorDirectories = Directory.GetDirectories(m_SearchRootDirectory, "Editor", SearchOption.AllDirectories);
            foreach (var editorDirectory in editorDirectories)
            {
                var di = new DirectoryInfo(editorDirectory);
                if (di.GetFiles("*.asmdef", SearchOption.TopDirectoryOnly).Length == 0)
                {
                    var editorAsmdef = new AssemblyDefinitionFile();
                    editorAsmdef.name = m_EditorAssemblyName + "." + di.Parent.Name;
                    editorAsmdef.autoReferenced = true;
                    editorAsmdef.includePlatforms = new[] { "Editor" };
                    editorAsmdef.references = new[] { playerAsmdef.name };
                    m_CreatedAsmdefs.Add(new CreatedAsmDefDetails()
                    {
                        path = di.FullName.Replace(Application.dataPath, "Assets") + editorAsmdef.name + ".asmdef",
                        asmdef = editorAsmdef
                    });
                }
            }
        }

    }
}