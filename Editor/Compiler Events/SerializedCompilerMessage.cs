using System;
using UnityEditor.Compilation;
using UnityEngine;

namespace UnityEditor.AssemblyTools
{
	[Serializable]
	public class SerializedCompilerMessage
	{
		[SerializeField] string m_Message;
		[SerializeField] string m_File;
		[SerializeField] int m_Line;
		[SerializeField] int m_Column;
		[SerializeField] CompilerMessageType m_Type;

		public string Message
		{
			get { return m_Message; }
			private set { m_Message = value; }
		}

		public string File
		{
			get { return m_File; }
			private set { m_File = value; }
		}

		public int Line
		{
			get { return m_Line; }
			private set { m_Line = value; }
		}

		public int Column
		{
			get { return m_Column; }
			private set { m_Column = value; }
		}

		public CompilerMessageType Type1
		{
			get { return m_Type; }
			private set { m_Type = value; }
		}

		public SerializedCompilerMessage(CompilerMessage msg)
		{
			Message = msg.message;
			File = msg.file;
			Line = msg.line;
			Column = msg.column;
			Type1 = msg.type;
		}
	}
}