using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cs_LAL_EegVisualization_1_1
{
	public partial class FormMain : Form
	{
		public FormMain()
		{
			InitializeComponent();
		}

		#region Delegate

		private delegate void WriteToConsoleHandler(string str);
		public void WriteToConsoleError(string error)
		{
			if (textBoxConsole.InvokeRequired == true)
			{
				WriteToConsoleHandler set = new WriteToConsoleHandler(WriteToConsoleError);
				textBoxConsole.Invoke(set, new object[] { error });
			}
			else
			{
				DateTime currentDateTime = DateTime.Now;
				textBoxConsole.AppendText($"{currentDateTime.Hour:D2}:{currentDateTime.Minute:D2}:{currentDateTime.Second:D2}: Error - ");
				textBoxConsole.AppendText(error + Environment.NewLine);
			}
		}
		public void WriteToConsoleInfo(string info)
		{
			if (textBoxConsole.InvokeRequired == true)
			{
				WriteToConsoleHandler set = new WriteToConsoleHandler(WriteToConsoleInfo);
				textBoxConsole.Invoke(set, new object[] { info });
			}
			else
			{
				DateTime currentDateTime = DateTime.Now;
				textBoxConsole.AppendText($"{currentDateTime.Hour:D2}:{currentDateTime.Minute:D2}:{currentDateTime.Second:D2}: Info - ");
				textBoxConsole.AppendText(info + Environment.NewLine);
			}
		}

		#endregion


		#region Button

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void buttonRefresh_Click(object sender, EventArgs e)
		{
			SerialRefresh();
		}

		private void buttonOpenClose_Click(object sender, EventArgs e)
		{

		}

		#endregion

		#region Serial

		private void SerialRefresh()
		{
			comboBoxSerialPort.Items.Clear();
			string[] availablePorts = SerialPort.GetPortNames();
			foreach (var port in availablePorts)
			{
				comboBoxSerialPort.Items.Add(port);
			}
			if (availablePorts.Length == 0)
			{
				WriteToConsoleError("No Port Found");
			}
			else if (availablePorts.Length == 1)
			{
				WriteToConsoleInfo($"Find 1 Port");
				comboBoxSerialPort.SelectedIndex = 0;
			}
			else
			{
				WriteToConsoleInfo($"Find {availablePorts.Length:D} Ports");
			}
		}

		#endregion

		#region Form

		private void FormMain_Load(object sender, EventArgs e)
		{
			WriteToConsoleInfo("Program Start");
			SerialRefresh();
		}

		#endregion

		
	}
}
