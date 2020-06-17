using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor.Compilation;

namespace UnityEditor.AssemblyTools
{
    [Serializable]
    public class CompilationEvent
    {
        [SerializeField] List<AssemblyCompilationEvent> m_AssemblyEvents;
        [SerializeField] string m_StartDateTimeUtc;
        [SerializeField] double m_AssemblyReloadTime;
        [SerializeField] double m_Duration;

        public string StarDateTimeUtc
        {
            get { return m_StartDateTimeUtc; }
            internal set { m_StartDateTimeUtc = value; }
        }

        public double AssemblyReloadTime
        {
            get { return m_AssemblyReloadTime; }
            internal set { m_AssemblyReloadTime = value; }
        }

        public double Duration
        {
            get { return m_Duration; }
            internal set { m_Duration = value; }
        }

        public List<AssemblyCompilationEvent> AssemblyEvents
        {
            get { return m_AssemblyEvents; }
            internal set { m_AssemblyEvents = value; }
        }

        public double AssemblyBuildTime
        {
            get
            {
                double total = 0;
                m_AssemblyEvents.ForEach(ae => total += ae.TimeSecondsSeconds);
                return total;
            }
        }

        public CompilationEvent()
        {
            m_AssemblyEvents = new List<AssemblyCompilationEvent>();
        }

        public AssemblyCompilationEvent FindAssemblyCompilationEvent(string name)
        {
            foreach (var assemblyCompilationEvent in m_AssemblyEvents)
            {
                if (assemblyCompilationEvent.Name == name)
                    return assemblyCompilationEvent;
            }

            return null;
        }
    }
}