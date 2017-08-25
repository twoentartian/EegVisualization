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
		/// <summary>
		/// 设计模式-单例模式
		/// 确保只有一个实例，并且该实例可以从任何地方访问
		/// </summary>
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
		/// <summary>
		/// 写入string数据到控制台，为跨线程调用，使用委托。
		/// </summary>
		/// <param name="str"></param>
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
		/// <summary>
		/// 当点击应用-退出按钮时发生
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		/// <summary>
		/// 当点击打开实时图像按钮时发生
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// 重新刷新串口
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonRefresh_Click(object sender, EventArgs e)
		{
			SerialRefresh();
		}

		/// <summary>
		/// 打开或者关闭串口
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
		/// <summary>
		/// 串口数据包的分隔符，分割时间和电压值
		/// </summary>
		private string[] Seperator = new string[]
		{
			";"
		};

		/// <summary>
		/// 属性：串口是否打开
		/// </summary>
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

		/// <summary>
		/// 计算机中目前所有可用的串口
		/// </summary>
		private string[] SerialAvailablePorts;

		/// <summary>
		/// 打开串口
		/// </summary>
		/// <param name="portName"></param>
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

		/// <summary>
		/// 关闭串口
		/// </summary>
		private void SerialClose()
		{
			serialPort.Close();
			SerialIsOpen = false;
		}

		/// <summary>
		/// 刷新目前所有的可用串口
		/// </summary>
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

		/// <summary>
		/// 当串口接收到数据时，执行此函数。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			string dataStr = serialPort.ReadLine();																					//读取一行数据
			string[] items = dataStr.Split(Seperator, StringSplitOptions.RemoveEmptyEntries);				//按照分隔符分开数据
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
		/// <summary>
		/// 加载主窗口时，执行此函数。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormMain_Load(object sender, EventArgs e)
		{
			WriteToConsoleInfo("Program Start");
			SerialRefresh();
			ConfigManager.GetInstance().LoadConfig();
			ConfigData cd = ConfigManager.GetInstance().GetCurrentConfig();
			DataManager.GetInstance().SetDataLengthAndChannel(cd.DataCycleLength, cd.Channel);
		}

		/// <summary>
		/// 主窗口关闭时的执行函数。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			Environment.Exit(0);
		}

		#endregion
	}
}
