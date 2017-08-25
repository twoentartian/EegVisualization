using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cs_LAL_EegVisualization_1_1
{
	public partial class FormMain : Form
	{
		#region Singleton

		private FormMain()
		{
			InitializeComponent();
		}

		private static FormMain _instance;

		public static FormMain GetInstance()
		{
			return _instance ?? (_instance = new FormMain());
		}

		#endregion

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
				textBoxConsole.AppendText($"{currentDateTime.Hour:D2}:{currentDateTime.Minute:D2}:{currentDateTime.Second:D2}: Error\t");
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
				textBoxConsole.AppendText($"{currentDateTime.Hour:D2}:{currentDateTime.Minute:D2}:{currentDateTime.Second:D2}: Info\t");
				textBoxConsole.AppendText(info + Environment.NewLine);
			}
		}

		#endregion


		#region Button

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void openRealTimeVisualizerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!SerialIsOpen)
			{
				WriteToConsoleError("Please A Open Serial Port First");
				return;
			}
			try
			{
				FormEegVisualizer eggVisualizerForm = new FormEegVisualizer();
				eggVisualizerForm.Show();
				WriteToConsoleInfo("Visualizer Is Running");
			}
			catch (ApplicationException exception)
			{
				WriteToConsoleError(exception.Message);
			}
		}

		private void buttonRefresh_Click(object sender, EventArgs e)
		{
			SerialRefresh();
		}

		private void buttonOpenClose_Click(object sender, EventArgs e)
		{
			if (SerialIsOpen)
			{
				SerialClose();
			}
			else
			{
				if (comboBoxSerialPort.SelectedIndex == -1)
				{
					WriteToConsoleError("No COM Port Selected");
				}
				if (string.IsNullOrEmpty(SerialAvailablePorts[comboBoxSerialPort.SelectedIndex]))
				{
					WriteToConsoleError("Invaild COM Port");
				}
				try
				{
					SerialOpen(SerialAvailablePorts[comboBoxSerialPort.SelectedIndex]);
				}
				catch (UnauthorizedAccessException)
				{
					WriteToConsoleError("This Port Is Occupied");
					return;
				}
				
			}
		}

		#endregion

		#region Serial

		private string[] Seperator = new string[]
		{
			";"
		};

		private bool SerialIsOpen
		{
			get { return serialPort.IsOpen; }
			set
			{
				if (value)
				{
					buttonOpenClose.Text = "Close";
					Logger.GetInstance().EnableLogger();
					WriteToConsoleInfo("Port Now Is Open");
				}
				else
				{
					buttonOpenClose.Text = "Open";
					Logger.GetInstance().DisableLogger();
					WriteToConsoleInfo("Port Now Is Close");
				}
			}
		}

		private string[] SerialAvailablePorts;
		private void SerialOpen(string portName)
		{
			serialPort.PortName = portName;
			try
			{
				serialPort.Open();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
			SerialIsOpen = true;
		}

		private void SerialClose()
		{
			serialPort.Close();
			SerialIsOpen = false;
		}

		private void SerialRefresh()
		{
			comboBoxSerialPort.Items.Clear();
			SerialAvailablePorts = SerialPort.GetPortNames();
			foreach (var port in SerialAvailablePorts)
			{
				comboBoxSerialPort.Items.Add(port);
			}
			if (SerialAvailablePorts.Length == 0)
			{
				WriteToConsoleError("No Port Found");
			}
			else if (SerialAvailablePorts.Length == 1)
			{
				WriteToConsoleInfo($"Find 1 Port");
				comboBoxSerialPort.SelectedIndex = 0;
			}
			else
			{
				WriteToConsoleInfo($"Find {SerialAvailablePorts.Length:D} Ports");
			}
		}

		private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			string dataStr = serialPort.ReadLine();
			string[] items = dataStr.Split(Seperator, StringSplitOptions.RemoveEmptyEntries);
			long time;
			double value;
			try
			{
				time = Convert.ToInt64(items[0]);
				value = Convert.ToDouble(items[1]);
			}
			catch (Exception)
			{
				return;
			}
			double[] values = new double[]
			{
				value
			};
			DataManager.GetInstance().AddData(time, values);
		}

		#endregion

		#region Form

		private void FormMain_Load(object sender, EventArgs e)
		{
			WriteToConsoleInfo("Program Start");
			SerialRefresh();
			ConfigManager.GetInstance().LoadConfig();
			ConfigData cd = ConfigManager.GetInstance().GetCurrentConfig();
			DataManager.GetInstance().SetDataLengthAndChannel(cd.DataCycleLength, cd.Channel);
		}

		private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			Environment.Exit(0);
		}

		#endregion
	}
}
