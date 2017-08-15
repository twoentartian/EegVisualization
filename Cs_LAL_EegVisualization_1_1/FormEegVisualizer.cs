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

		private void InitCharts()
		{
			chartTimeDomain.Series.Clear();
			chartTimeDomain.Series.Add(TimeDomainTag);
			chartTimeDomain.Series[TimeDomainTag].ChartType = SeriesChartType.FastLine;

			chartFrequencyDomain.Series.Clear();
			chartFrequencyDomain.Series.Add(FrequencyDomainTag);
			chartFrequencyDomain.Series[FrequencyDomainTag].ChartType = SeriesChartType.FastLine;
		}

		public void RefreshTimeChart(double[] data)
		{
			chartTimeDomain.Series[TimeDomainTag].Points.Clear();
			for (int i = 0; i < data.Length; i++)
			{
				chartTimeDomain.Series[TimeDomainTag].Points.AddXY(i + 1, data[i]);
			}
		}

		public void RefreshFrequencyChart(double[] data)
		{
			chartFrequencyDomain.Series[FrequencyDomainTag].Points.Clear();
			for (int i = 0; i < data.Length; i++)
			{
				chartFrequencyDomain.Series[FrequencyDomainTag].Points.AddXY(i + 1, data[i]);
			}
		}

		#endregion

		#region Timer

		private static double i = 0;

		private void timerRefreshData_Tick(object sender, EventArgs e)
		{
			DataManager dm = DataManager.GetInstance();
			RefreshTimeChart(dm.GetDataSequence());
			Complex[] fftComplexData = FftPart.FFT(dm.GetDataSequence(), false);
			double[] fftDoubleData = new double[fftComplexData.Length/2];
			for (int i = 0; i < fftComplexData.Length/2; i++)
			{
				fftDoubleData[i] = fftComplexData[i].Magnitude;
			}
			RefreshFrequencyChart(fftDoubleData);


			dm.AddData(Math.Sin(i));
			i += 2.5;
		}

		#endregion

		#region Form

		private void FormEegVisualizer_FormClosed(object sender, FormClosedEventArgs e)
		{
			FormMain.GetInstance().WriteToConsoleInfo("Visualizer Is Closed");
			_isRunning = false;
		}

		#endregion

	}
}
