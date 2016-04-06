using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CygnusControls
{
	public class FolderBrowserControlCommand : ICommand
	{
		#region · Types ·
		public enum CommandType
		{
			UpdateCurrentPath
		}
		#endregion

		#region · Data Members ·
		private CommandType m_command_type;
		private FolderBrowserControl m_parent;
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets command type
		/// </summary>
		public CommandType Type
		{
			get { return m_command_type; }
		}
		#endregion

		#region · ICommand functions ·

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			if (m_parent != null)
				m_parent.ExecuteCommand(this, parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="in_type">Command type</param>
		/// <param name="in_parent">Parent FolderBrowserControl</param>
		public FolderBrowserControlCommand(CommandType in_type, FolderBrowserControl in_parent)
		{
			m_command_type = in_type;
			m_parent = in_parent;
		}

		#endregion
	}
}

