namespace Cs_LAL_EegVisualization_1_1
{
	partial class FormEegVisualizer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
			this.timerRefreshData = new System.Windows.Forms.Timer(this.components);
			this.chartTimeDomain = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.chartFrequencyDomain = new System.Windows.Forms.DataVisualization.Charting.Chart();
			((System.ComponentModel.ISupportInitialize)(this.chartTimeDomain)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.chartFrequencyDomain)).BeginInit();
			this.SuspendLayout();
			// 
			// timerRefreshData
			// 
			this.timerRefreshData.Tick += new System.EventHandler(this.timerRefreshData_Tick);
			// 
			// chartTimeDomain
			// 
			chartArea1.Name = "ChartArea1";
			this.chartTimeDomain.ChartAreas.Add(chartArea1);
			legend1.Name = "Legend1";
			this.chartTimeDomain.Legends.Add(legend1);
			this.chartTimeDomain.Location = new System.Drawing.Point(12, 12);
			this.chartTimeDomain.Name = "chartTimeDomain";
			series1.ChartArea = "ChartArea1";
			series1.Legend = "Legend1";
			series1.Name = "Series1";
			this.chartTimeDomain.Series.Add(series1);
			this.chartTimeDomain.Size = new System.Drawing.Size(1491, 439);
			this.chartTimeDomain.TabIndex = 0;
			this.chartTimeDomain.Text = "chartTimeDomain";
			// 
			// chartFrequencyDomain
			// 
			chartArea2.Name = "ChartArea1";
			this.chartFrequencyDomain.ChartAreas.Add(chartArea2);
			legend2.Name = "Legend1";
			this.chartFrequencyDomain.Legends.Add(legend2);
			this.chartFrequencyDomain.Location = new System.Drawing.Point(12, 457);
			this.chartFrequencyDomain.Name = "chartFrequencyDomain";
			series2.ChartArea = "ChartArea1";
			series2.Legend = "Legend1";
			series2.Name = "Series1";
			this.chartFrequencyDomain.Series.Add(series2);
			this.chartFrequencyDomain.Size = new System.Drawing.Size(1491, 439);
			this.chartFrequencyDomain.TabIndex = 1;
			this.chartFrequencyDomain.Text = "chartFrequencyDomain";
			// 
			// FormEegVisualizer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1515, 935);
			this.Controls.Add(this.chartFrequencyDomain);
			this.Controls.Add(this.chartTimeDomain);
			this.Name = "FormEegVisualizer";
			this.Text = "FormEegVisualizer";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormEegVisualizer_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.chartTimeDomain)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chartFrequencyDomain)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timerRefreshData;
		private System.Windows.Forms.DataVisualization.Charting.Chart chartTimeDomain;
		private System.Windows.Forms.DataVisualization.Charting.Chart chartFrequencyDomain;
	}
}