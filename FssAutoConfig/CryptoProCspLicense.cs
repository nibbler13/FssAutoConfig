using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FssAutoConfig {
	class CryptoProCspLicense {
		private const string MACHINES_TO_RUN = @"\\mskv-dc\NETLOGON\FSS_ARM_LPU\MachinesWithoutCryptoProCspLicense.txt";
		private const string CRYPTOPRO_LICENSES = @"\\mskv-dc\NETLOGON\FSS_ARM_LPU\CryptoProCspLicenses.ini";

		public static void InstallLicense() {
			LoggingService.LogMessageToFile("Установка лицензии для CryptoProCsp");

			try {
				foreach (string item in new string[] { MACHINES_TO_RUN, CRYPTOPRO_LICENSES })
					if (!File.Exists(item)) {
						LoggingService.LogMessageToFile("Не удается найти файл: " + item);
						return;
					}

				LoggingService.LogMessageToFile("Считывание списка систем для применения: " + MACHINES_TO_RUN);
				string[] machinesToRun = File.ReadAllLines(MACHINES_TO_RUN);
				string machineName = Environment.MachineName.ToUpper();
				if (!machinesToRun.Contains(machineName)) {
					LoggingService.LogMessageToFile("Данная система отсутствует в списке на установку лицензии CryptoProCsp");
					return;
				}

				LoggingService.LogMessageToFile("Считывания списка лицензий CryptoProCsp: " + CRYPTOPRO_LICENSES);
				string[] licenses = IniFile.ReadKeyValuePairs("main", CRYPTOPRO_LICENSES);
				string licenseToUse = string.Empty;

				foreach (string item in licenses) {
					string[] license = item.Split('=');
					if (license.Length == 2) {
						if (license[1].ToUpper().Equals(machineName.ToUpper())) {
							LoggingService.LogMessageToFile("Скрипт выполнялся ранее на этой системе");
							return;
						}

						if (!string.IsNullOrEmpty(license[1]))
							continue;
					}

					licenseToUse = license[0];
					break;
				}

				if (string.IsNullOrEmpty(licenseToUse)) {
					LoggingService.LogMessageToFile("Не удалось найти свободную лицензию");
					return;
				}

				LoggingService.LogMessageToFile("Запись лицензии " + licenseToUse + " в реестр");

				string cryptoProKey =
					@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\" +
					@"S-1-5-18\Products\7AB5E7046046FB044ACD63458B5F481C\InstallProperties";
				RegistryKey key = Registry.LocalMachine.OpenSubKey(cryptoProKey, true);

				if (key == null) {
					LoggingService.LogMessageToFile("CryptoProCsp не установлена на компьютере, не удается найти ветку реестра: " +
						"HKLM\\" + cryptoProKey);
					return;
				}

				key.SetValue("ProductID", licenseToUse);
				LoggingService.LogMessageToFile("Ключ успешно записан в реестр");

				if (!IniFile.WriteValue("main", licenseToUse, machineName, CRYPTOPRO_LICENSES))
					LoggingService.LogMessageToFile("Не удалось обновить данные в файле: " + CRYPTOPRO_LICENSES);
			} catch (Exception e) {
				LoggingService.LogMessageToFile(e.Message + Environment.NewLine + e.StackTrace);
			}
		}
	}
}
