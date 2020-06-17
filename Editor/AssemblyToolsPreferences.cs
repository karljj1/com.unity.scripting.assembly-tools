using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace UnityEditor.AssemblyTools
{
    public static class AssemblyToolsPreferences
    {
        /// <summary>
        /// Used to provide live changes in the editor.
        /// </summary>
        internal static event Action NodePreferenceChanged;

        /// <summary>
        /// Location compilation logs will be stored.
        /// </summary>
        public static string HistoryDirectory
        {
            get { return EditorPrefs.GetString("AssemblyTools-HistoryDirectory", "/../Library/AssemblyToolsLogs/"); }
            set { EditorPrefs.SetString("AssemblyTools-HistoryDirectory", value); }
        }

        /// <summary>
        /// File to save node position data.
        /// </summary>
        public static string NodeLayoutFile
        {
            get { return EditorPrefs.GetString("AssemblyTools-NodeLayoutFile", "/../Library/AssemblyToolsNodeLayouts.json"); }
            set { EditorPrefs.SetString("AssemblyTools-NodeLayoutFile", value); }
        }

        /// <summary>
        /// Maximum number of compilation logs to store.
        /// </summary>
        public static int MaxHistorySize
        {
            get { return EditorPrefs.GetInt("AssemblyTools-MaxHistorySize", 10); }
            set { EditorPrefs.SetInt("AssemblyTools-MaxHistorySize", value); }
        }

        /// <summary>
        /// Color for editor nodes.
        /// </summary>
        public static Color EditorNodeColor
        {
            get
            {
                Color col;
                ColorUtility.TryParseHtmlString(EditorPrefs.GetString("AssemblyTools-EditorNodeColor", "#de8427"), out col);
                return col;
            }

            set { EditorPrefs.SetString("AssemblyTools-EditorNodeColor", "#" + ColorUtility.ToHtmlStringRGB(value)); }
        }

        /// <summary>
        /// Color for player nodes.
        /// </summary>
        public static Color PlayerNodeColor
        {
            get
            {
                Color col;
                ColorUtility.TryParseHtmlString(EditorPrefs.GetString("AssemblyTools-PlayerNodeColor", "#29739b"), out col);
                return col;
            }

            set { EditorPrefs.SetString("AssemblyTools-PlayerNodeColor", "#" + ColorUtility.ToHtmlStringRGB(value)); }
        }

        /// <summary>
        /// Color for test nodes.
        /// </summary>
        public static Color TestNodeColor
        {
            get
            {
                Color col;
                ColorUtility.TryParseHtmlString(EditorPrefs.GetString("AssemblyTools-TestNodeColor", "#006531"), out col);
                return col;
            }

            set { EditorPrefs.SetString("AssemblyTools-TestNodeColor", "#" + ColorUtility.ToHtmlStringRGB(value)); }
        }

        [SettingsProvider]
        internal static SettingsProvider CreateProjectSettingsProvider()
        {
            return new SettingsProvider("Preferences/Assembly Tools", SettingsScope.User) { activateHandler = (s, element) =>
            {
                element.Add(new Label("History Directory"));
                var histDir = new TextField();
                histDir.name = "History Directory";
                histDir.value = HistoryDirectory;
                histDir.RegisterValueChangedCallback(evt => HistoryDirectory = evt.newValue);
                histDir.tooltip = "Where to store the log files for compilation events. Relative to the Assets directory.";
                element.Add(histDir);

                element.Add(new Label("Node Layout File"));
                var nodeFile = new TextField();
                nodeFile.name = "Node Layout File";
                nodeFile.value = NodeLayoutFile;
                nodeFile.RegisterValueChangedCallback(evt => NodeLayoutFile = evt.newValue);
                element.Add(nodeFile);

                element.Add(new Label("Max History"));
                var maxHist = new IntegerField();
                maxHist.value = MaxHistorySize;
                maxHist.RegisterValueChangedCallback(evt =>
                {
                    var value = evt.newValue;
                    if (value < 0)
                    {
                        value = 0;
                        maxHist.SetValueWithoutNotify(value);
                    }

                    MaxHistorySize = value;
                });
                element.Add(maxHist);

                element.Add(new Label("Editor Node Color"));
                var editorNodeColor = new ColorField();
                editorNodeColor.name = "Editor Node Color";
                editorNodeColor.value = EditorNodeColor;
                editorNodeColor.RegisterValueChangedCallback(evt =>
                {
                    EditorNodeColor = evt.newValue;
                    if (NodePreferenceChanged != null)
                        NodePreferenceChanged();
                });
                element.Add(editorNodeColor);

                element.Add(new Label("Player Node Color"));
                var playerNodeColor = new ColorField();
                playerNodeColor.name = "Player Node Color";
                playerNodeColor.value = PlayerNodeColor;
                playerNodeColor.RegisterValueChangedCallback(evt =>
                {
                    PlayerNodeColor = evt.newValue;
                    if (NodePreferenceChanged != null)
                        NodePreferenceChanged();
                });
                element.Add(playerNodeColor);

                element.Add(new Label("Test Node Color"));
                var testNodeColor = new ColorField();
                testNodeColor.name = "Test Node Color";
                testNodeColor.value = TestNodeColor;
                testNodeColor.RegisterValueChangedCallback(evt =>
                {
                    TestNodeColor = evt.newValue;
                    if (NodePreferenceChanged != null)
                        NodePreferenceChanged();
                });
                element.Add(testNodeColor);
            }};
        }
    }
}
