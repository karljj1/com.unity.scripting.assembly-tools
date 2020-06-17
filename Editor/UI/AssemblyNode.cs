using System;
using System.Linq;
using UnityEditor.Compilation;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UnityEditor.AssemblyTools
{
    public class AssemblyNode : Node
    {
        CompilationEvent m_CompilationEvent;
        AssemblyCompilationEvent m_AssemblyCompilationEvent;

        public Assembly Assembly { get; }

        public AssemblyDefinitionFile AssemblyDefinitionFile { get; }

        public AssemblyNode(Assembly assembly)
        {
            AssemblyToolsPreferences.NodePreferenceChanged += UpdateNodeFromPerferences;

            Assembly = assembly;
            AssemblyDefinitionFile = Assembly.GetAssemblyDefinitionFile();

            name = title = assembly.name;
            capabilities |= Capabilities.Movable;
            //persistenceKey = assembly.name;
            //contentContainer.persistenceKey = "contents";
            userData = assembly;

            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "Input";
            inputPort.name = "input";
            if(AssemblyDefinitionFile == null || Assembly.IsReadOnly())
                inputPort.SetEnabled(false);

            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Output";
            outputPort.name = "output";
            inputContainer.Add(inputPort);
            outputContainer.Add(outputPort);

            if(AssemblyDefinitionFile == null)
                outputPort.SetEnabled(false);

            UpdateNodeFromPerferences();
            capabilities |= Capabilities.Movable;
            //persistenceKey = assembly.name;
        }

        ~AssemblyNode()
        {
            AssemblyToolsPreferences.NodePreferenceChanged -= UpdateNodeFromPerferences;
        }

        public override void OnSelected()
        {
            SelectEdges(true);
            base.OnSelected();
        }

        public override void OnUnselected()
        {
            SelectEdges(false);
            base.OnUnselected();
        }

        void SelectEdges(bool selected)
        {
            var inPort = inputContainer.Q<Port>();
            foreach (var inPortConnection in inPort.connections)
            {
                inPortConnection.selected = selected;
            }

            var outPort = outputContainer.Q<Port>();
            foreach (var outPortConnection in outPort.connections)
            {
                outPortConnection.selected = selected;
            }
        }

        public void AddReference(AssemblyNode node)
        {
            if (AssemblyDefinitionFile.references == null || AssemblyDefinitionFile.references.Length == 0)
            {
                AssemblyDefinitionFile.references = new []{ node.Assembly.name };
            }
            else
            {
                if (AssemblyDefinitionFile.references.Contains(node.Assembly.name))
                    return;

                Array.Resize(ref AssemblyDefinitionFile.references, AssemblyDefinitionFile.references.Length + 1);
                AssemblyDefinitionFile.references[AssemblyDefinitionFile.references.Length - 1] = node.Assembly.name;
            }
            Assembly.SaveChanges(AssemblyDefinitionFile);
        }

        public void RemoveReference(AssemblyNode node)
        {
            if (AssemblyDefinitionFile.references != null || AssemblyDefinitionFile.references.Length > 0)
            {
                for (int i = 0; i < AssemblyDefinitionFile.references.Length; ++i)
                {
                    if(AssemblyDefinitionFile.references[i] == node.Assembly.name)
                    {
                        // Swap and remove the end item
                        AssemblyDefinitionFile.references[i] = AssemblyDefinitionFile.references[AssemblyDefinitionFile.references.Length - 1];
                        Array.Resize(ref AssemblyDefinitionFile.references, AssemblyDefinitionFile.references.Length - 1);
                        Assembly.SaveChanges(AssemblyDefinitionFile);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a reference to the node is possible. If one already exists then returns true;
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool CanReference(AssemblyNode node)
        {
            if (AssemblyDefinitionFile == null)
                return false;

            if (AssemblyDefinitionFile.references != null)
            {
                if (AssemblyDefinitionFile.references.Contains(node.Assembly.name))
                    return false;
            }
            return true;
        }

        void UpdateNodeFromPerferences()
        {
            if(Assembly.IsTestAssembly())
                titleContainer.style.backgroundColor = AssemblyToolsPreferences.TestNodeColor;
            else if (Assembly.flags == AssemblyFlags.EditorAssembly)
                titleContainer.style.backgroundColor = AssemblyToolsPreferences.EditorNodeColor;
            else
                titleContainer.style.backgroundColor = AssemblyToolsPreferences.PlayerNodeColor;
        }

        /// <summary>
        /// Reset the node to its default visual state.
        /// </summary>
        public void Reset()
        {
            // Reset node color.
            var col = titleContainer.style.backgroundColor.value;
            col.a = 1;
            titleContainer.style.backgroundColor = col;

            var foldout = contentContainer.Q<Foldout>();
            if(foldout != null)
                foldout.RemoveFromHierarchy();

            var compTimeNode = titleContainer.Q<Label>("compilation time");
            if(compTimeNode != null)
                compTimeNode.RemoveFromHierarchy();

            var autoRef = contentContainer.Q("Auto Referenced");
            if(autoRef != null)
                autoRef.RemoveFromHierarchy();
        }

        /// <summary>
        /// Setup the node for editing the related asmdef file.
        /// </summary>
        public void SetupForEditing()
        {
            Reset();

            if (AssemblyDefinitionFile != null)
            {
                var autoRefToggle = new Toggle() { text = "Auto Referenced", name = "Auto Referenced", value = AssemblyDefinitionFile.autoReferenced};
                autoRefToggle.SetEnabled(!Assembly.IsReadOnly());
                autoRefToggle.RegisterValueChangedCallback(evt =>
                {
                    AssemblyDefinitionFile.autoReferenced = evt.newValue;
                    Assembly.SaveChanges(AssemblyDefinitionFile);
                });
                contentContainer.Add(autoRefToggle);
            }

            // Files
            var sourceFiles = Assembly.sourceFiles;
            var foldout = new Foldout(){ value = false, text = string.Format("Source Files({0})", sourceFiles.Length)/*, persistenceKey = "contentsFoldout"*/};
            contentContainer.Add(foldout);

            foreach (var file in sourceFiles)
            {
                var label = new Label(file);
                label.RegisterCallback(new EventCallback<MouseDownEvent>(evt => EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(file))));
                foldout.Add(label);
            }
        }

        /// <summary>
        /// Setup the node to display data relating to a compilation event.
        /// </summary>
        /// <param name="assemblyCompEvent">The assembly compilation data. If null then this node assumes it was not part of the build.</param>
        public void SetupForHistory(CompilationEvent compilationEvent, AssemblyCompilationEvent assemblyCompEvent)
        {
            Reset();
            m_CompilationEvent = compilationEvent;
            m_AssemblyCompilationEvent = assemblyCompEvent;

            // Node color. Nodes with no data are faded.
            var col = titleContainer.style.backgroundColor.value;
            col.a = m_AssemblyCompilationEvent != null ? 1f : 0.2f;
            titleContainer.style.backgroundColor = col;

            if (m_AssemblyCompilationEvent == null)
            {
                return;
            }

            // Compilation time label
            var compTimeNode = new Label() { name = "compilation time" };
            titleContainer.Add(compTimeNode);
            compTimeNode.text = string.Format("{0:0}%({1:0.00})", (m_AssemblyCompilationEvent.TimeSecondsSeconds / m_CompilationEvent.AssemblyBuildTime) * 100,  m_AssemblyCompilationEvent.TimeSecondsSeconds);

            // Changed Files
            if (m_AssemblyCompilationEvent.ChangedFiles.Count > 0)
            {
                var foldout = new Foldout(){ value = false, text = string.Format("Changes ({0})", m_AssemblyCompilationEvent.ChangedFiles.Count)/*, persistenceKey = "contentsFoldout"*/};
                contentContainer.Add(foldout);

                foreach (var compilationDataChangedFile in m_AssemblyCompilationEvent.ChangedFiles)
                {
                    var label = new Label(compilationDataChangedFile);
                    label.RegisterCallback(new EventCallback<MouseDownEvent>(evt => EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(compilationDataChangedFile))));
                    foldout.Add(label);
                }
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!(evt.target is Node))
                return;
            //evt.menu.AppendAction("Disconnect all", new Action<DropdownMenu.MenuAction>(this.DisconnectAll), new Func<DropdownMenu.MenuAction, DropdownMenu.MenuAction.StatusFlags>(this.DisconnectAllStatus), (object) null);
            //evt.menu.AppendSeparator((string) null);
        }
    }
}
