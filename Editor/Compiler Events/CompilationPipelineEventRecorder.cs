using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AssemblyTools;
using UnityEngine;

namespace UnityEditor.Compilation.AssemblyTools
{
    [InitializeOnLoad]
    class CompilationPipelineEventRecorder : AssetPostprocessor
    {
        static CompilationEvent s_CompilationEvent;
        static Dictionary<string, DateTime> s_StartTimes;

        public const string DateTimeFormat = "MM-dd-yyyy HH-mm-ss";

        static string TempFile
        {
            get { return Application.dataPath + "/../Temp/CompilationPipelineEventRecorderTemp.json"; }
        }

        static CompilationPipelineEventRecorder()
        {
            CompilationPipeline.assemblyCompilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        /// <summary>
        /// The first step is to determine if this is a compilation build and what has changed.
        /// </summary>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            EditorPrefs.SetString("CompilationStartTime", DateTime.Now.ToBinary().ToString());
            s_CompilationEvent = new CompilationEvent(){ StarDateTimeUtc = DateTime.Now.ToString() };
            s_StartTimes = new Dictionary<string, DateTime>();

            // TODO: Dlls?
            foreach (var file in importedAssets)
            {
                if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    var assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(file);
                    if (assemblyName.EndsWith(".dll"))
                        assemblyName = assemblyName.Substring(0, assemblyName.Length - ".dll".Length);

                    var assemblyEvent = s_CompilationEvent.FindAssemblyCompilationEvent(assemblyName);
                    if (assemblyEvent == null)
                    {
                        assemblyEvent = new AssemblyCompilationEvent() { Name = assemblyName };
                        s_CompilationEvent.AssemblyEvents.Add(assemblyEvent);
                    }
                    assemblyEvent.ChangedFiles.Add(file);
                }
            }

            foreach (var file in deletedAssets)
            {
                if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    var assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(file);
                    if (assemblyName.EndsWith(".dll"))
                        assemblyName = assemblyName.Substring(0, assemblyName.Length - ".dll".Length);

                    var assemblyEvent = s_CompilationEvent.FindAssemblyCompilationEvent(assemblyName);
                    if (assemblyEvent == null)
                    {
                        assemblyEvent = new AssemblyCompilationEvent() { Name = assemblyName };
                        s_CompilationEvent.AssemblyEvents.Add(assemblyEvent);
                    }
                    assemblyEvent.ChangedFiles.Add(file);
                }
            }
        }

        /// <summary>
        /// Callback when compilation for each assembly is started. We record the start time here.
        /// </summary>
        /// <param name="path"></param>
        static void CompilationPipelineOnAssemblyCompilationStarted(string path)
        {
            s_StartTimes[path] = DateTime.Now;
        }

        /// <summary>
        /// Callback when an assembly has finished compiling. We can now calculate the time it took.
        /// </summary>
        static void CompilationPipelineOnAssemblyCompilationFinished(string path, CompilerMessage[] compilerMessages)
        {
            var startTime = s_StartTimes[path];

            var assembly = CompilationPipeline.GetAssemblies().First(o => o.outputPath == path);
            var assemblyEvent = s_CompilationEvent.FindAssemblyCompilationEvent(assembly.name);
            if (assemblyEvent == null)
            {
                assemblyEvent = new AssemblyCompilationEvent(){ Name = assembly.name };
                s_CompilationEvent.AssemblyEvents.Add(assemblyEvent);
            }
            assemblyEvent.Path = path;
            assemblyEvent.TimeSecondsSeconds = (DateTime.Now - startTime).TotalSeconds;

            foreach(var cm in compilerMessages)
            {
                assemblyEvent.CompilerMessages.Add(new SerializedCompilerMessage(cm));
            }
        }

        /// <summary>
        /// Callback for when the assemby is about to be reload. We record the start time as an EditorPreference as data will not persist between reloads.
        /// </summary>
        static void OnBeforeAssemblyReload()
        {
            if (s_StartTimes!= null && s_StartTimes.Count > 0)
            {
                EditorPrefs.SetString("AssemblyReloadTime", DateTime.Now.ToBinary().ToString());

                // Fetch the names of the assemblies from the paths
                var nameLookup = new Dictionary<string, Assembly>();
                foreach (var assembly in CompilationPipeline.GetAssemblies())
                {
                    nameLookup[assembly.outputPath] = assembly;
                }

                foreach (var assemblyCompilationEvent in s_CompilationEvent.AssemblyEvents)
                {
                    Assembly assembly = null;
                    if (nameLookup.TryGetValue(assemblyCompilationEvent.Path, out assembly))
                    {
                        assemblyCompilationEvent.Name = assembly.name;
                    }
                }

                // Save a temp version as it will be lost during reload.
                var json = EditorJsonUtility.ToJson(s_CompilationEvent, true);
                File.WriteAllText(TempFile, json);
            }
        }

        /// <summary>
        /// Callback after an assembly reload. We can now calculate the total time.
        /// </summary>
        static void OnAfterAssemblyReload()
        {
            // Do we have temp data?
            if (File.Exists(TempFile))
            {
                var tempJson = File.ReadAllText(TempFile);
                var compilationEvent = new CompilationEvent();
                EditorJsonUtility.FromJsonOverwrite(tempJson, compilationEvent);
                File.Delete(TempFile);

                var reloadStartTime =  DateTime.FromBinary(long.Parse(EditorPrefs.GetString("AssemblyReloadTime")));
                compilationEvent.AssemblyReloadTime = (DateTime.Now - reloadStartTime).TotalSeconds;

                var startTime = DateTime.FromBinary(long.Parse(EditorPrefs.GetString("CompilationStartTime")));
                compilationEvent.Duration = (DateTime.Now - startTime).TotalSeconds;

                var json = EditorJsonUtility.ToJson(compilationEvent, true);
                var dir = Application.dataPath + AssemblyToolsPreferences.HistoryDirectory;
                var path = dir + DateTime.Now.ToString(DateTimeFormat) + ".json";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, json);

                // Delete old files
                DirectoryInfo dirInfo = new DirectoryInfo (dir);
                FileInfo[] files = dirInfo.GetFiles("*.json").OrderByDescending(p=>p.CreationTime).ToArray();
                for (int i = AssemblyToolsPreferences.MaxHistorySize; i < files.Length; ++i)
                {
                    files[i].Delete();
                }
            }
        }
    }
}