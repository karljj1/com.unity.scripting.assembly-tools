using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor.Compilation;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace UnityEditor.AssemblyTools
{
    class AsmdefWindow : EditorWindow
    {
        AssemblyGraphView m_View;
        CompileHistoryDetailsPane m_CompileHistoryDetailsPane;
        List<AssemblyNode> m_Nodes = new List<AssemblyNode>();
        NodeLayoutData m_LayoutData = new NodeLayoutData();

        [MenuItem("Window/Analysis/Assembly Definition Tools")]
        public static void ShowWindow()
        {
            var window = (AsmdefWindow)GetWindow(typeof(AsmdefWindow));
            window.titleContent = new GUIContent("Assembly Tools");
            window.minSize = new Vector2(500, 500);
            window.Show();
        }

        void OnEnable()
        {
            var root = this.rootVisualElement;

            // Toolbar
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.Add(new Label("Build Logs"));
            root.Add(toolbar);
            var histPopup = new CompileHistoryPopup();
            histPopup.RegisterValueChangedCallback(SelectHistory);
            toolbar.Add(histPopup);

            var miniMapToggle = new Toggle(){ text = "Mini Map" };
            miniMapToggle.RegisterValueChangedCallback(evt => ShowMiniMap(evt.newValue));
            ShowMiniMap(miniMapToggle.value);
            toolbar.Add(miniMapToggle);
            toolbar.Add(new ToolbarButton(() => LayoutNodes()) { text = "Layout"});

            m_View = new AssemblyGraphView
            {
                name = "Assembly View",
                //persistenceKey = "Assembly View"
            };
            m_View.style.flexGrow = 1;
            m_View.nodeCreationRequest += NodeCreationRequest;
            m_View.graphViewChanged = GraphViewChanged;
            root.Add(m_View);

            m_View.Add(new CompilationStatusMessage());

            m_CompileHistoryDetailsPane = new CompileHistoryDetailsPane();
            m_View.Add(m_CompileHistoryDetailsPane);

            Reload();
        }

        /// <summary>
        /// Toggle the mini map visibility.
        /// </summary>
        /// <param name="visible"></param>
        void ShowMiniMap(bool visible)
        {
            var root = this.rootVisualElement;
            var miniMap = root.Q<MiniMap>();

            if (visible && miniMap == null)
            {
                miniMap = new MiniMap();
                miniMap.SetPosition(new Rect(0, 372, 200, 176));
                m_View.Add(miniMap);
            }

            if(miniMap != null)
                miniMap.visible = visible;
        }

        void SelectHistory(ChangeEvent<string> evt)
        {
            if (evt.newValue == CompileHistoryPopup.NoSelection)
            {
                foreach (var node in m_Nodes)
                {
                    node.SetupForEditing();
                }

                m_CompileHistoryDetailsPane.CompilationEvent = null;
            }
            else
            {
                var compilationEvent = new CompilationEvent();
                var file = Path.Combine(Application.dataPath + AssemblyToolsPreferences.HistoryDirectory, evt.newValue + ".json");

                if (File.Exists(file))
                {
                    var json = File.ReadAllText(file);
                    EditorJsonUtility.FromJsonOverwrite(json, compilationEvent);

                    foreach (var node in m_Nodes)
                    {
                        node.SetupForHistory(compilationEvent, compilationEvent.AssemblyEvents.FirstOrDefault(o => o.Name == node.title));
                    }

                    m_CompileHistoryDetailsPane.CompilationEvent = compilationEvent;
                }
                else
                {
                    Debug.LogWarning("Could not find file: " + file);
                }
            }
        }

        void OnDestroy()
        {
            m_LayoutData.Save();
        }

        GraphViewChange GraphViewChanged(GraphViewChange graphviewchange)
        {
            if (graphviewchange.movedElements != null)
            {
                foreach (var element in graphviewchange.movedElements)
                {
                    var node = element as Node;
                    if (node != null)
                    {
                        m_LayoutData.UpdatePosition(node.name, node.GetPosition().position);
                    }
                }
            }

            if (graphviewchange.edgesToCreate != null)
            {
                foreach (var edge in graphviewchange.edgesToCreate)
                {
                    var node = edge.input.node as AssemblyNode;
                    var reference = edge.output.node as AssemblyNode;
                    node.AddReference(reference);
                }
            }

            if (graphviewchange.elementsToRemove != null)
            {
                foreach (var element in graphviewchange.elementsToRemove)
                {
                    var edge = element as Edge;
                    if (edge != null)
                    {
                        var node = edge.input.node as AssemblyNode;
                        var reference = edge.output.node as AssemblyNode;
                        node.RemoveReference(reference);
                    }
                }
            }

            return graphviewchange;
        }

        void NodeCreationRequest(NodeCreationContext obj)
        {
            Debug.Log("Create Request");
        }

        private Node CreateNode(Assembly assembly)
        {
            var nodeUI = new AssemblyNode(assembly);
            m_View.AddElement(nodeUI);
            m_Nodes.Add(nodeUI);
            return nodeUI;
        }

        private void CreateEdge(AssemblyNode assemblyNode)
        {
            Assembly assembly = assemblyNode.Assembly;

            foreach (var assemblyAllReference in assembly.assemblyReferences)
            {
                var target = m_Nodes.FirstOrDefault(n => n.name == assemblyAllReference.name);

                if (target != null)
                {
                    var inputNode = assemblyNode.Q<Port>("input");
                    var outputNde = target.Q<Port>("output");

                    var edge = inputNode.ConnectTo(outputNde);
                    //edge.persistenceKey = assembly.name + "->" + target.name;
                    m_View.AddElement(edge);
                }
            }
        }

        void Reload()
        {
            if (m_View == null)
                return;

            foreach (var assembly in CompilationPipeline.GetAssemblies())
            {
                CreateNode(assembly);
            }

            foreach (var node in m_Nodes)
            {
                CreateEdge(node);
            }

            LayoutNodes(true);
        }

        void LayoutNodes(bool useSavedPositions = false)
        {
            // GraphView has no auto layout at the moment. Lets just stack them by default.
            Vector2 startPos = Vector2.zero;
            foreach (var node in m_Nodes)
            {
                var rect = node.GetPosition();
                rect.position = startPos;
                if(useSavedPositions)
                    rect.position = m_LayoutData.GetPosition(node.name, rect.position);

                node.SetPosition(rect);
                node.SetupForEditing();
                startPos.y += 80;
            }
        }
    }
}
