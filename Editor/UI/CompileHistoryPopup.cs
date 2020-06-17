using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Compilation.AssemblyTools;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.AssemblyTools
{
    public class CompileHistoryPopup : PopupField<string>
    {
        public new class UxmlFactory : UxmlFactory<CompileHistoryPopup> {}

        public const string NoSelection = "None";

        public CompileHistoryPopup()
            : base(GetChoices(), 0)
        {
            style.width = 150;
            formatSelectedValueCallback = FormatLabel;
            formatListItemCallback = FormatLabel;
        }

        private string FormatLabel(string arg)
        {
            DateTime dt;
            if (DateTime.TryParseExact(arg, CompilationPipelineEventRecorder.DateTimeFormat, null, System.Globalization.DateTimeStyles.None, out dt))
            {
                return dt.ToString("MM-dd-yyyy HH:mm:ss");
            }

            return arg;
        }

        static List<string> GetChoices()
        {
            var choices = new List<string>();
            var dir = Application.dataPath + AssemblyToolsPreferences.HistoryDirectory;

            if (Directory.Exists(dir))
            {
                choices.Add(NoSelection);
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                FileInfo[] files = dirInfo.GetFiles("*.json").OrderByDescending(p=>p.CreationTime).ToArray();
                int extLen = ".json".Length;
                foreach (var fileName in files)
                {
                    choices.Add(fileName.Name.Substring(0, fileName.Name.Length - extLen));
                }

                return choices;
            }

            choices.Add("No Compilation Data");
            return choices;
        }
    }
}
