using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace FssAutoConfig {
	class Program {
		private const string FSS_CONFIG_FILE = @"C:\FssTools\configuration\.settings\ru.ibs.fss.eln.prefs";

		static void Main(string[] args) {
			LoggingService.CheckAndCleanOldFile();

			if (!File.Exists(FSS_CONFIG_FILE)) {
				LoggingService.LogMessageToFile("Отсутсвует файл с конфигурацией ФСС АРМ ЛПУ: " + FSS_CONFIG_FILE);
				return;
			}

			SettingsReader settingsReader = new SettingsReader();
			SettingsWriter.WriteSettings(settingsReader, FSS_CONFIG_FILE);

			LoggingService.LogMessageToFile("Работа программы завершена");
		}
	}
}
