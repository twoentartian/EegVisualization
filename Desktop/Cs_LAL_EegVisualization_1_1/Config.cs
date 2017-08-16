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

		private ConfigManager()
		{
			
		}

		private static ConfigManager _instance;

		public static ConfigManager GetInstance()
		{
			return _instance ?? (_instance = new ConfigManager());
		}

		#endregion

		private const string ConfigName = "Config.json";
		private readonly string ConfigPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + ConfigName;
		private ConfigData _currentConfig = new ConfigData();

		public ConfigData GetCurrentConfig()
		{
			return _currentConfig;
		}
		
		private ConfigData GetDefault()
		{
			ConfigData temp = new ConfigData();
			temp.Channel = 8;
			temp.DataCycleLength = 128;
			return temp;
		}

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
