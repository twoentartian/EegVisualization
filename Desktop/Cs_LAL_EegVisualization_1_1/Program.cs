using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cs_LAL_EegVisualization_1_1
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// 主程序入口
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(FormMain.GetInstance());							//从主窗口开始运行
		}
	}
}
