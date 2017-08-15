using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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

		public void AddData(double[] value)
		{
			if (value.Length != ConfigManager.GetInstance().GetCurrentConfig().Channel)
			{
				throw new ApplicationException("The Number Of Data Do Not Equal To Channel Length");
			}

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
			for (int channel = 0; channel < _rawData.GetLength(0); channel++)
			{
				for (int i = 0; i < _rawData.GetLength(1) - 1; i++)
				{
					_rawData[channel, i] = _rawData[channel, i + 1];
				}
				_rawData[channel, _rawData.GetLength(1) - 1].Value = value[channel];
				_rawData[channel, _rawData.GetLength(1) - 1].Time = time;
			}
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
			string logFolderPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + LogFolderName;
			if (!Directory.Exists(logFolderPath))
			{
				Directory.CreateDirectory(logFolderPath);
			}
			string logPath = logFolderPath + Path.DirectorySeparatorChar + $"{dt.Year:D4}-{dt.Month:D2}-{dt.Day:D2} {dt.Hour:D2}-{dt.Minute:D2}-{dt.Second:D2}" + LogExtName;
			_fs = new FileStream(logPath, FileMode.OpenOrCreate);
			string startContent = "Time (ms)" + LogSeperator;
			for (int channel = 0; channel < ConfigManager.GetInstance().GetCurrentConfig().Channel; channel++)
			{
				startContent += $"Channel {channel:D}" + LogSeperator;
			}
			startContent += Environment.NewLine;
			byte[] bufferB = Encoding.ASCII.GetBytes(startContent);
			_fs.Write(bufferB, 0, bufferB.Length);
		}

		public void WriteToLog(long time, double[] value)
		{
			string data = String.Empty;
			foreach (var d in value)
			{
				data = data + d + LogSeperator;
			}
			string bufferS = time + LogSeperator + data + Environment.NewLine;
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
