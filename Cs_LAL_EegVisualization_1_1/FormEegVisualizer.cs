using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Cs_LAL_EegVisualization_1_1
{
	public partial class FormEegVisualizer : Form
	{
		public FormEegVisualizer()
		{
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
			chartTimeDomain.Series[TimeDomainTag].Points.Clear();
			for (int i = 0; i < data.Length; i++)
			{
				chartFrequencyDomain.Series[FrequencyDomainTag].Points.AddXY(i + 1, data[i]);
			}
		}

		#endregion

		#region Timer

		private void timerRefreshData_Tick(object sender, EventArgs e)
		{
			DataManager dm = DataManager.GetInstance();
			RefreshTimeChart(dm.RawData);

			dm.AddData(new Random().NextDouble());
		}

		#endregion

	}
}
