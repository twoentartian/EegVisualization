using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cs_LAL_EegVisualization_1_1
{
	struct ConfigData
	{
		public int Channel;

		public int DataCycleLength;
	}

	class ConfigManager
	{
		#region Singleton
		/// <summary>
		/// 设计模式-单例模式
		/// 确保只有一个实例，并且该实例可以从任何地方访问
		/// </summary>
		private ConfigManager()
		{
			
		}

		private static ConfigManager _instance;

		public static ConfigManager GetInstance()
		{
			return _instance ?? (_instance = new ConfigManager());
		}

		#endregion

		private const string ConfigName = "Config.json";																												//配置文件的文件名
		private readonly string ConfigPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + ConfigName;         //配置文件的路径
		private ConfigData _currentConfig = new ConfigData();

		/// <summary>
		/// 获取当前配置
		/// </summary>
		/// <returns></returns>
		public ConfigData GetCurrentConfig()
		{
			return _currentConfig;
		}

		/// <summary>
		/// 获取默认配置
		/// </summary>
		/// <returns></returns>
		private ConfigData GetDefault()
		{
			ConfigData temp = new ConfigData();
			temp.Channel = 8;
			temp.DataCycleLength = 128;
			return temp;
		}

		/// <summary>
		/// 读取保存在JSON文件中的配置
		/// </summary>
		public void LoadConfig()
		{
			if (File.Exists(ConfigPath))
			{
				string jsonContent = File.ReadAllText(ConfigPath);
				_currentConfig = JsonConvert.DeserializeObject<ConfigData>(jsonContent);
				FormMain.GetInstance().WriteToConsoleInfo("Load Config");
			}
			else
			{
				_currentConfig = GetDefault();
				WriteConfig(_currentConfig);
				FormMain.GetInstance().WriteToConsoleInfo("No Config File Found, Use Default Config");
			}
		}

		/// <summary>
		/// 将配置写入文件中
		/// </summary>
		/// <param name="argConfigData"></param>
		public void WriteConfig(ConfigData argConfigData)
		{
			string jsonContent = JsonConvert.SerializeObject(argConfigData);
			using (FileStream fs = new FileStream(ConfigPath, FileMode.OpenOrCreate))
			{
				byte[] data = Encoding.ASCII.GetBytes(jsonContent);
				fs.Write(data, 0, data.Length);
				fs.Flush();
				fs.Close();
			}
		}
	}
}
