using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace UnityEditor.AssemblyTools
{
    public class AssemblyGraphView : GraphView
    {
        public AssemblyGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // FIXME: add a coordinator so that ContentDragger and SelectionDragger cannot be active at the same time.
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            Insert(0, new GridBackground());
            //focusIndex = 0;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // Only allow nodes that do not already reference this node, are not read only and contain an AssembluyDefinitionFile.
            var compatiblePorts = new List<Port>();

            foreach (var p in ports.ToList())
            {
                if(p.direction == startPort.direction) continue;
                if(p.node == startPort.node) continue;

                var node = (AssemblyNode) p.node;
                if(node.Assembly.IsReadOnly()) continue;
                if(node.AssemblyDefinitionFile == null)continue;
                if(!node.CanReference((AssemblyNode)startPort.node))continue;
                compatiblePorts.Add(p);
            }

            return compatiblePorts;
        }
    }
}
