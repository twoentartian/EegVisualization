using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Cs_LAL_EegVisualization_1_1
{
	public partial class FormEegVisualizer : Form
	{
		private static bool _isRunning = false;

		public FormEegVisualizer()
		{
			if (_isRunning)
			{
				throw new ApplicationException("Visualizer Is Already Running");
			}
			else
			{
				_isRunning = true;
			}
			InitializeComponent();
			InitCharts();
			timerRefreshData.Enabled = true;
		}

		private string TimeDomainTag = "Time Domain";
		private string FrequencyDomainTag = "Frequency Domain";

		#region Func
		/// <summary>
		/// 初始化图表内容
		/// </summary>
		private void InitCharts()
		{
			chartTimeDomain.Series.Clear();
			chartTimeDomain.Series.Add(TimeDomainTag);
			chartTimeDomain.Series[TimeDomainTag].ChartType = SeriesChartType.FastLine;

			chartFrequencyDomain.Series.Clear();
			chartFrequencyDomain.Series.Add(FrequencyDomainTag);
			chartFrequencyDomain.Series[FrequencyDomainTag].ChartType = SeriesChartType.FastLine;

			for (int channel = 0; channel < ConfigManager.GetInstance().GetCurrentConfig().Channel; channel++)
			{
				comboBoxChannelSelect.Items.Add($"Channel: {channel:D}");
			}
			comboBoxChannelSelect.SelectedIndex = 0;
		}

		/// <summary>
		/// 刷新时域的图像
		/// </summary>
		/// <param name="data"></param>
		public void RefreshTimeChart(double[] data)
		{
			chartTimeDomain.Series[TimeDomainTag].Points.Clear();
			for (int i = 0; i < data.Length; i++)
			{
				chartTimeDomain.Series[TimeDomainTag].Points.AddXY(i + 1, data[i]);
			}
		}

		/// <summary>
		/// 刷新频域的图像
		/// </summary>
		/// <param name="data"></param>
		/// <param name="sampleFre"></param>
		public void RefreshFrequencyChart(double[] data, double sampleFre)
		{
			chartFrequencyDomain.Series[FrequencyDomainTag].Points.Clear();
			for (int i = 0; i < data.Length; i++)
			{
				chartFrequencyDomain.Series[FrequencyDomainTag].Points.AddXY(i + 1, data[i]);
			}
			chartFrequencyDomain.ChartAreas[0].AxisX.Minimum = 0;
			chartFrequencyDomain.ChartAreas[0].AxisX.Maximum = (int)(sampleFre/2);
		}

		#endregion

		#region Timer

		private static double i = 0;

		//Timer会每隔一定时间执行一次这个函数，执行的间隔可以在设计器中找到。
		private void timerRefreshData_Tick(object sender, EventArgs e)
		{
			int channel = comboBoxChannelSelect.SelectedIndex;

			DataManager dm = DataManager.GetInstance();
			RefreshTimeChart(dm.GetDataSequence(channel));

			if (dm.IsFftDataFresh)
			{
				RefreshFrequencyChart(dm.GetFrequencySequence(channel), dm.SampleFrequency);
			}

			//Complex[] fftComplexData = FftPart.FFT(dm.GetDataSequence(channel), false);
			//double[] fftDoubleData = new double[fftComplexData.Length/2];
			//for (int i = 0; i < fftComplexData.Length/2; i++)
			//{
			//	fftDoubleData[i] = fftComplexData[i].Magnitude;
			//}
			//RefreshFrequencyChart(fftDoubleData);
		}

		#endregion

		#region Form
		/// <summary>
		/// 当这个窗口关闭时发生的事件。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormEegVisualizer_FormClosed(object sender, FormClosedEventArgs e)
		{
			FormMain.GetInstance().WriteToConsoleInfo("Visualizer Is Closed");
			_isRunning = false;
		}

		#endregion

		#region Combobox
		/// <summary>
		/// 当channel选择框的选择内容改变时发生的事件。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void comboBoxChannelSelect_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		#endregion
	}
}
