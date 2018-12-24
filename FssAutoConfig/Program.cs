using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace FssAutoConfig {
	class Program {
		private const string FSS_CONFIG_FILE = @"C:\FssTools\configuration\.settings\ru.ibs.fss.eln.prefs";
        public static string rootFolder;

		static void Main(string[] args) {
			LoggingService.CheckAndCleanOldFile();

			if (!File.Exists(FSS_CONFIG_FILE)) {
				LoggingService.LogMessageToFile("Отсутсвует файл с конфигурацией ФСС АРМ ЛПУ: " + FSS_CONFIG_FILE);
				return;
			}

            if (args.Length != 1) {
                LoggingService.LogMessageToFile("Для запуска требуется передать один параметр - путь к папке с конфигурационными файлами");
                return;
            }

            rootFolder = args[0];

			SettingsReader settingsReader = new SettingsReader();
			SettingsWriter.WriteSettings(settingsReader, FSS_CONFIG_FILE);
			CryptoProCspLicense.InstallLicense();

			LoggingService.LogMessageToFile("Работа программы завершена");
		}
	}
}
