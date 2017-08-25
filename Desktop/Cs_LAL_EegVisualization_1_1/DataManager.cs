using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace Cs_LAL_EegVisualization_1_1
{
	/// <summary>
	/// 一个脑电波数据的内容：时间+电压值
	/// </summary>
	struct EegData
	{
		public double Value;
		public long Time;
	}

	class DataManager
	{
		#region Singleton
		/// <summary>
		/// 设计模式-单例模式
		/// 确保只有一个实例，并且该实例可以从任何地方访问
		/// </summary>
		private DataManager()
		{
			
		}

		private static DataManager _instance;

		public static DataManager GetInstance()
		{
			return _instance ?? (_instance = new DataManager());
		}

		#endregion

		/// <summary>
		/// 原始数据，直接来源于串口
		/// </summary>
		private EegData[,] _rawData;
		public EegData[,] RawData => _rawData;

		/// <summary>
		/// 频域数据是否刷新的标志
		/// </summary>
		private bool _isFftDataFresh = false;
		public bool IsFftDataFresh => _isFftDataFresh;

		/// <summary>
		/// 频域数据
		/// </summary>
		private double[,] _fftData;
		public double[,] FftData
		{
			get
			{
				_isFftDataFresh = false;
				return _fftData;
			}
		}

		/// <summary>
		/// 当前采样频率
		/// </summary>
		private double _sampleFrequency;
		public double SampleFrequency => _sampleFrequency;

		private int count = 0;

		/// <summary>
		/// 设置数据管理器管理的数据长度和通道数
		/// </summary>
		/// <param name="length"></param>
		/// <param name="channel"></param>
		public void SetDataLengthAndChannel(int length, int channel)
		{
			_rawData = new EegData[channel, length];
			_fftData = new double[channel,length/2];
		}

		/// <summary>
		/// 获取某个通道的时域数据
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public double[] GetDataSequence(int channel)
		{
			double[] data = new double[_rawData.GetLength(1)];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = _rawData[channel, i].Value;
			}
			return data;
		}

		/// <summary>
		/// 获取某个通道的频域数据
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public double[] GetFrequencySequence(int channel)
		{
			double[] data = new double[_fftData.GetLength(1)];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = _fftData[channel, i];
			}
			_isFftDataFresh = false;
			return data;
		}

		/// <summary>
		/// 清空所有数据
		/// </summary>
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

		/// <summary>
		/// 添加单个数据
		/// </summary>
		/// <param name="time"></param>
		/// <param name="value"></param>
		public void AddData(long time, double[] value)
		{
			if (value.Length != ConfigManager.GetInstance().GetCurrentConfig().Channel)
			{
				throw new ApplicationException("The Number Of Data Do Not Equal To Channel Length");
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
			count++;
			if (count == ConfigManager.GetInstance().GetCurrentConfig().DataCycleLength)
			{
				count = 0;
				double sampleTime = (double)(_rawData[0, _rawData.GetLength(1) - 1].Time - _rawData[0, 0].Time) /
				                         (ConfigManager.GetInstance().GetCurrentConfig().DataCycleLength-1);
				_sampleFrequency = 1000000 / sampleTime;
				for (int channel = 0; channel < FftData.GetLength(0); channel++)
				{
					Complex[] fftComplexData = FftPart.FFT(GetDataSequence(channel), false);
					for (int i = 0; i < fftComplexData.Length / 2; i++)
					{
						_fftData[channel, i] = fftComplexData[i].Magnitude;
					}
				}
				_isFftDataFresh = true;
			}
		}
	}

	/// <summary>
	/// 数据记录器
	/// </summary>
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

		/// <summary>
		/// 使能数据记录器
		/// </summary>
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
			string startContent = "Time (us)" + LogSeperator;
			for (int channel = 0; channel < ConfigManager.GetInstance().GetCurrentConfig().Channel; channel++)
			{
				startContent += $"Channel {channel:D}" + LogSeperator;
			}
			startContent += Environment.NewLine;
			byte[] bufferB = Encoding.ASCII.GetBytes(startContent);
			_fs.Write(bufferB, 0, bufferB.Length);
		}

		/// <summary>
		/// 添加数据到日志文件
		/// </summary>
		/// <param name="time"></param>
		/// <param name="value"></param>
		public void WriteToLog(long time, double[] value)
		{
			if (_fs == null)
			{
				return;
			}
			string data = String.Empty;
			foreach (var d in value)
			{
				data = data + d + LogSeperator;
			}
			string bufferS = time + LogSeperator + data + Environment.NewLine;
			byte[] bufferD = Encoding.ASCII.GetBytes(bufferS);
			_fs.Write(bufferD,0,bufferD.Length);
		}

		/// <summary>
		/// 关闭日志记录器，并且清空文件写入缓存
		/// </summary>
		public void DisableLogger()
		{
			_fs.Flush();
			_fs.Close();
			_fs.Dispose();
		}
	}
}
