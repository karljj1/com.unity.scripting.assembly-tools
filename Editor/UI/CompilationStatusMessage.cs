using System.IO;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.AssemblyTools
{
    class CompilationStatusMessage : VisualElement
    {
        Label m_Label;

        public CompilationStatusMessage()
        {
            // Prevent changes while we are building.
            CompilationPipeline.assemblyCompilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEventsOnBeforeAssemblyReload;
        }

        ~CompilationStatusMessage()
        {
            CompilationPipeline.assemblyCompilationStarted -= CompilationPipelineOnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished -= CompilationPipelineOnAssemblyCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload -= AssemblyReloadEventsOnBeforeAssemblyReload;
        }

        void AssemblyReloadEventsOnBeforeAssemblyReload()
        {
            LockView("Reloading Assemblies");
        }

        void CompilationPipelineOnAssemblyCompilationStarted(string obj)
        {
            LockView("Compiling\n" + Path.GetFileName(obj));
        }

        void CompilationPipelineOnAssemblyCompilationFinished(string arg1, CompilerMessage[] arg2)
        {
            UnlockView();
        }

        void LockView(string message)
        {
            parent.SetEnabled(false);

            if (m_Label == null)
            {
                m_Label = new Label() {name = "compilingMessage"};
                m_Label.style.alignSelf = Align.Center;
                m_Label.style.unityTextAlign = TextAnchor.MiddleCenter;
                m_Label.style.fontSize = 50;
                parent.Add(m_Label);
                m_Label.StretchToParentSize();
            }

            m_Label.text = message;
        }

        void UnlockView()
        {
            parent.SetEnabled(true);
            if (m_Label != null)
            {
                m_Label.RemoveFromHierarchy();
                m_Label = null;
            }
        }
    }
}
