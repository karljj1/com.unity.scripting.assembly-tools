
using UnityEngine.UIElements;

namespace UnityEditor.AssemblyTools
{
    public class CompileHistoryDetailsPane : VisualElement
    {
        CompilationEvent m_CompilationEvent;

        public CompilationEvent CompilationEvent
        {
            set
            {
                // TODO: This blocks graph input
                m_CompilationEvent = value;
                Clear();
                if(m_CompilationEvent != null)
                    UpdateDetails();
                SetEnabled(m_CompilationEvent != null);
            }
        }

        void UpdateDetails()
        {
            var foldOut = new Foldout() {text = "Details", value = true};
            Add(foldOut);

            double buildTime = m_CompilationEvent.AssemblyBuildTime;
            foldOut.Add(new Label(string.Format("Compile Time {0:00.00s}", buildTime)));
            foldOut.Add(new Label(string.Format("Assembly Reload Time {0:00.00s}", m_CompilationEvent.AssemblyReloadTime)));

            double total = buildTime + m_CompilationEvent.AssemblyReloadTime;
            foldOut.Add(new Label(string.Format("Total Compilation Time {0:00.00s}", total)));

            double unknown = m_CompilationEvent.Duration - total;
            foldOut.Add(new Label(string.Format("Editor Time {0:00.00s}", unknown)));
            foldOut.Add(new Label(string.Format("Total Time {0:00.00s}", m_CompilationEvent.Duration)));
        }
    }
}
