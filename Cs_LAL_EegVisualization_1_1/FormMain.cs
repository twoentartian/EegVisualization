using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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


		#region Button

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		#endregion

	}
}
