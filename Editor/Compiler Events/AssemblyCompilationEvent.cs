using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.AssemblyTools
{
	[Serializable]
	public class AssemblyCompilationEvent
	{
		[SerializeField] List<string> m_ChangedFiles = new List<string>();
		[SerializeField] string m_Name;
		[SerializeField] string m_Path;
		[SerializeField] double m_TimeSeconds;
		[SerializeField] List<SerializedCompilerMessage> m_CompilerMessages;

		public AssemblyCompilationEvent()
		{
			m_CompilerMessages = new List<SerializedCompilerMessage>();
		}

		public List<string> ChangedFiles
		{
			get { return m_ChangedFiles; }
			internal set { m_ChangedFiles = value; }
		}

		public string Name
		{
			get { return m_Name; }
			internal set { m_Name = value; }
		}

		public string Path
		{
			get { return m_Path; }
			internal set { m_Path = value; }
		}

		public double TimeSecondsSeconds
		{
			get { return m_TimeSeconds; }
			internal set { m_TimeSeconds = value; }
		}

		public List<SerializedCompilerMessage> CompilerMessages
		{
			get { return m_CompilerMessages; }
			internal set { m_CompilerMessages = value; }
		}
	}
}