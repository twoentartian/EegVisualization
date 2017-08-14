using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cs_LAL_EegVisualization_1_1
{
	class DataManager
	{
		#region Singleton

		private DataManager()
		{
			
		}

		private static DataManager _instance;

		public static DataManager GetInstance()
		{
			return _instance ?? (_instance = new DataManager());
		}

		#endregion

		private double[] _rawData;

		public double[] RawData => _rawData;

		public void SetDataLength(int length)
		{
			_rawData = new double[length];
		}

		public void ClearData()
		{
			_rawData = new double[_rawData.Length];
		}

		public void AddData(double value)
		{
			for (int i = 0; i < _rawData.Length - 1; i++)
			{
				_rawData[i] = _rawData[i + 1];
			}
			_rawData[_rawData.Length - 1] = value;
		}
	}
}
