using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.AssemblyTools
{
    /// <summary>
    /// Stores positional data for nodes.
    /// </summary>
    [System.Serializable]
    public class NodeLayoutData
    {
        private string Path
        {
            get { return Application.dataPath + AssemblyToolsPreferences.NodeLayoutFile; }
        }

        [System.Serializable]
        public class LayoutPoint
        {
            public string name;
            public Vector2 position;
        }

        [SerializeField]
        List<LayoutPoint> layouts = new List<LayoutPoint>();

        public NodeLayoutData()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Save;
        }

        public Vector2 GetPosition(string name, Vector2 oldPos)
        {
            foreach(var li in layouts)
            {
                if(li.name == name)
                {
                    return li.position;
                }
            }

            var layoutInfo = new LayoutPoint() { name = name, position = oldPos };
            layouts.Add(layoutInfo);
            return oldPos;
        }

        public void UpdatePosition(string name, Vector2 pos)
        {
            foreach (var li in layouts)
            {
                if (li.name == name)
                {
                    li.position = pos;
                    return;
                }
            }

            var layoutInfo = new LayoutPoint() { name = name, position = pos };
            layouts.Add(layoutInfo);
        }

        public void Load()
        {
            if(System.IO.File.Exists(Path))
            {
                var json = System.IO.File.ReadAllText(Path);
                EditorJsonUtility.FromJsonOverwrite(json, this);
            }
        }

        public void Save()
        {
            var json = EditorJsonUtility.ToJson(this, true);
            System.IO.File.WriteAllText(Path, json);
        }
    }
}