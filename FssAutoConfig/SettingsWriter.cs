using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FssAutoConfig {
	public class SettingsWriter {
		public static void WriteSettings(SettingsReader settingsReader, string file) {
			LoggingService.LogMessageToFile("Запись считанных настроек в файл: " + file);

			try {
				List<string> currentConfig =  File.ReadAllLines(file).ToList();

				foreach (KeyValuePair<string, string> pair in settingsReader.Settings) {
					string[] lines = currentConfig.ToArray();
					foreach (string line in lines) {
						if (line.StartsWith(pair.Key)) {
							currentConfig.Remove(line);
							break;
						}
					}

					currentConfig.Add(pair.Key + "=" + Conversion.ConvertCharStr2jEsc(pair.Value));
				}

				File.WriteAllLines(file, currentConfig.ToArray());
			} catch (Exception e) {
				LoggingService.LogMessageToFile(e.Message + Environment.NewLine + e.StackTrace);
			}
		}
	}
}
