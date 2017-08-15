using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Cs_LAL_EegVisualization_1_1
{
	struct EegData
	{
		public double Value;
		public long Time;
	}

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

		private EegData[,] _rawData;
		public EegData[,] RawData => _rawData;

		private readonly Stopwatch _stopwatch = new Stopwatch();
		private bool _isFirstData = true;

		public void SetDataLengthAndChannel(int length, int channel)
		{
			_rawData = new EegData[channel, length];
			_isFirstData = true;
		}

		public double[] GetDataSequence(int channel)
		{
			double[] data = new double[_rawData.GetLength(1)];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = _rawData[channel, i].Value;
			}
			return data;
		}

		public void ClearData()
		{
			for (int i = 0; i < _rawData.GetLength(0); i++)
			{
				for (int j = 0; j < _rawData.GetLength(1); j++)
				{
					_rawData[i, j].Time = 0;
					_rawData[i, j].Value = 0;
				}
			}
		}

		public void ResetTimer()
		{
			_stopwatch.Stop();
			_isFirstData = true;
		}

		public void AddData(int channel, double value)
		{
			long time = 0;
			if (_isFirstData)
			{
				_isFirstData = false;
				_stopwatch.Reset();
				_stopwatch.Start();
			}
			else
			{
				time = _stopwatch.ElapsedMilliseconds;
			}

			for (int i = 0; i < _rawData.GetLength(1) - 1; i++)
			{
				_rawData[channel, i] = _rawData[channel, i + 1];
			}
			_rawData[channel, _rawData.GetLength(1) - 1].Value = value;
			_rawData[channel, _rawData.GetLength(1) - 1].Time = time;
			Logger.GetInstance().WriteToLog(time, value);
		}
	}

	class Logger
	{
		#region Singleton

		private Logger()
		{
			
		}

		private static Logger _instance;

		public static Logger GetInstance()
		{
			return _instance ?? (_instance = new Logger());
		}

		#endregion

		private const string LogFolderName = "Log";
		private const string LogExtName = ".csv";
		private const string LogSeperator = ",";
		private FileStream _fs;

		public void EnableLogger()
		{
			DateTime dt = DateTime.Now;
			string LogFolderPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + LogFolderName;
			if (!Directory.Exists(LogFolderPath))
			{
				Directory.CreateDirectory(LogFolderPath);
			}
			string LogPath = LogFolderPath + Path.DirectorySeparatorChar + $"{dt.Year:D4}-{dt.Month:D2}-{dt.Day:D2} {dt.Hour:D2}-{dt.Minute:D2}-{dt.Second:D2}" + LogExtName;
			_fs = new FileStream(LogPath, FileMode.OpenOrCreate);
		}

		public void WriteToLog(long time, double value)
		{
			string bufferS = time + LogSeperator + value.ToString("F5") + Environment.NewLine;
			byte[] bufferD = Encoding.ASCII.GetBytes(bufferS);
			_fs.Write(bufferD,0,bufferD.Length);
		}

		public void DisableLogger()
		{
			DataManager.GetInstance().ResetTimer();
			_fs.Flush();
			_fs.Close();
			_fs.Dispose();
		}
	}
}
