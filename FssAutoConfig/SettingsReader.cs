using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FssAutoConfig {
	public class SettingsReader {
		private const string FILE_SETTINGS_GENERAL = @"\\budzdorov.ru\NETLOGON\FSS_ARM_LPU\GeneralSettings.ini";
		private const string FILE_SETTINGS_DEPARTMENTS = @"\\budzdorov.ru\NETLOGON\FSS_ARM_LPU\DepartmentsSettings.ini";
		private const string FILE_USER_CERTIFICATES = @"\\budzdorov.ru\NETLOGON\FSS_ARM_LPU\UserCertificates.ini";

		private const string SECTION_REQUISITES = "Настройка реквизитов организации";
		private const string SECTION_FSS_SERVICES = "Настройка сервисов ФСС";
		private const string SECTION_DB = "Настройка соединения с базой данных";
		private const string SECTION_CERIFICATES = "Настройка подписи для сервисов";

		private const string KEY_REQUISITES_NAME = "Наименование МО";
		private const string KEY_REQUISITES_FULLNAME = "Полное наименование МО";
		private const string KEY_REQUISITES_OGRN = "ОГРН МО";
		private const string KEY_REQUISITES_OKPO = "ОКПО МО";
		private const string KEY_REQUISITES_ADDRESS = "Адрес МО";

		private const string KEY_FSS_SERVICES_URL = "Строка соединения ФСС";
		private const string KEY_FSS_SERVICES_TIMEOUT = "Время попытки подключения, секунд";
		private const string KEY_FSS_SERVICES_USEPROXY = "Использовать прокси";
		private const string KEY_FSS_SERVICES_PROXYHOST = "Прокси хост";
		private const string KEY_FSS_SERVICES_PROXYPORT = "Прокси порт";
		private const string KEY_FSS_SERVICES_PROXYAUTH = "Авторизация прокси";
		private const string KEY_FSS_SERVICES_PROXYUSER = "Прокси логин";
		private const string KEY_FSS_SERVICES_PROXYPASSWORD = "Прокси пароль";

		private const string KEY_DB_URL = "Строка соединения";
		private const string KEY_DB_USER = "Имя пользователя";
		private const string KEY_DB_PASSWORD = "Пароль";

		private const string KEY_CERIFICATES_PERFORMVALIDATION = "Выполнять форматно-логический контроль";
		private const string KEY_CERIFICATES_SIGNOUTMESSAGE = "Подписывать исходящие сообщения";
		private const string KEY_CERIFICATES_VERIFYMESSAGE = "Проверять подпись на входящих сообщениях";
		private const string KEY_CERIFICATES_HPROV = "Криптопровайдер";
		private const string KEY_CERIFICATES_KEYWSCONTAINER = "Тип контейнера личный";
		private const string KEY_CERIFICATES_CERTWSNAME = "Имя сертификата МО";
		private const string KEY_CERIFICATES_ENCRYPTMESSAGES = "Шифровать сообщение";
		private const string KEY_CERIFICATES_FSSKEYWSCONTAINER = "Тип контейнера фсс";
		private const string KEY_CERIFICATES_CERTFSSNAME = "Имя сертификата ФСС";

		public Dictionary<string, string> Settings { get; private set; }

		public SettingsReader() {
			LoggingService.LogMessageToFile("Считывание данных пользователя из ActiveDirectory");
			DirectoryService.ReadUserInfoFromActiveDirectory(
				out string userDisplayName,
				out string userMail,
				out string userPhoneNumber);
			LoggingService.LogMessageToFile("Отображаемое имя: " + userDisplayName);
			LoggingService.LogMessageToFile("Адрес электронной почты: " + userMail);
			LoggingService.LogMessageToFile("Номер телефона: " + userPhoneNumber);
			
			Settings = new Dictionary<string, string> {
				{ "AUTHOR", userDisplayName },
				{ "AUTHOR_EMAIL", userMail },
				{ "AUTHOR_PHONE", userPhoneNumber },
				{ "FSSkeywscontainer", "" },
				{ "LPU_ADDRESS", "" },
				{ "LPU_FULL_NAME", "" },
				{ "LPU_NAME", "" },
				{ "LPU_OKPO", "" },
				{ "LPU_ORGN", "" },
				{ "certfssname", "" },
				{ "certwsname", "" },
				{ "dbpassword", "" },
				{ "dburl", "" },
				{ "dbuser", "" },
				{ "encryptmessages", "" },
				{ "hprov", "" },
				{ "keywscontainer", "" },
				{ "netproxyhost", "" },
				{ "netproxypassword", "" },
				{ "netproxyport", "" },
				{ "netproxyuser", "" },
				{ "netuseproxy", "" },
				{ "netuseproxyauth", "" },
				{ "performvalidation", "" },
				{ "signoutmessage", "" },
				{ "vefifyinmessage", "" },
				{ "wstimeout", "" },
				{ "wsurl", "" }
			};

			ReadGeneralSettins();
			ReadDepartmentSettings();
			ReadUserCertificate();

			LoggingService.LogMessageToFile("Считанные настройки: ");
			foreach (KeyValuePair<string, string> pair in Settings)
				LoggingService.LogMessageToFile(pair.Key + "=" + pair.Value);
		}


		private void ReadGeneralSettins() {
			if (!IsSettingsFileExist(FILE_SETTINGS_GENERAL))
				return;

			string[] requisites = IniFile.ReadSections(FILE_SETTINGS_GENERAL);
			if (requisites == null) {
				LoggingService.LogMessageToFile("Отсутствует секции с настройками");
				return;
			}
				
			if (requisites.Contains(SECTION_REQUISITES)) {
				Settings["LPU_NAME"] = IniFile.ReadValue(SECTION_REQUISITES, KEY_REQUISITES_NAME, FILE_SETTINGS_GENERAL);
				Settings["LPU_FULL_NAME"] = IniFile.ReadValue(SECTION_REQUISITES, KEY_REQUISITES_FULLNAME, FILE_SETTINGS_GENERAL);
				Settings["LPU_ORGN"] = IniFile.ReadValue(SECTION_REQUISITES, KEY_REQUISITES_OGRN, FILE_SETTINGS_GENERAL);
				Settings["LPU_OKPO"] = IniFile.ReadValue(SECTION_REQUISITES, KEY_REQUISITES_OKPO, FILE_SETTINGS_GENERAL);
			} else
				LoggingService.LogMessageToFile("Отсутсвует секция: " + SECTION_REQUISITES);

			if (requisites.Contains(SECTION_FSS_SERVICES)) {
				Settings["wsurl"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_URL, FILE_SETTINGS_GENERAL);
				Settings["wstimeout"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_TIMEOUT, FILE_SETTINGS_GENERAL);
				Settings["netuseproxy"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_USEPROXY, FILE_SETTINGS_GENERAL);
				Settings["netproxyhost"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYHOST, FILE_SETTINGS_GENERAL);
				Settings["netproxyport"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYPORT, FILE_SETTINGS_GENERAL);
				Settings["netuseproxyauth"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYAUTH, FILE_SETTINGS_GENERAL);
				Settings["netproxyuser"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYUSER, FILE_SETTINGS_GENERAL);
				Settings["netproxypassword"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYPASSWORD, FILE_SETTINGS_GENERAL);
			} else
				LoggingService.LogMessageToFile("Отсутсвует секция: " + SECTION_FSS_SERVICES);

			if (requisites.Contains(SECTION_DB)) {
				Settings["dbuser"] = IniFile.ReadValue(SECTION_DB, KEY_DB_USER, FILE_SETTINGS_GENERAL);
				Settings["dbpassword"] = IniFile.ReadValue(SECTION_DB, KEY_DB_PASSWORD, FILE_SETTINGS_GENERAL);
			} else
				LoggingService.LogMessageToFile("Отсутсвует секция: " + SECTION_DB);

			if (requisites.Contains(SECTION_CERIFICATES)) {
				Settings["performvalidation"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_PERFORMVALIDATION, FILE_SETTINGS_GENERAL);
				Settings["signoutmessage"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_SIGNOUTMESSAGE, FILE_SETTINGS_GENERAL);
				Settings["vefifyinmessage"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_VERIFYMESSAGE, FILE_SETTINGS_GENERAL);
				Settings["hprov"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_HPROV, FILE_SETTINGS_GENERAL);
				Settings["keywscontainer"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_KEYWSCONTAINER, FILE_SETTINGS_GENERAL);
				Settings["encryptmessages"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_ENCRYPTMESSAGES, FILE_SETTINGS_GENERAL);
				Settings["FSSkeywscontainer"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_FSSKEYWSCONTAINER, FILE_SETTINGS_GENERAL);
				Settings["certfssname"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_CERTFSSNAME, FILE_SETTINGS_GENERAL);
			} else
				LoggingService.LogMessageToFile("Отсутсвует секция: " + SECTION_CERIFICATES);
		}

		private void ReadDepartmentSettings() {
			if (!IsSettingsFileExist(FILE_SETTINGS_DEPARTMENTS))
				return;

			string[] sectionNames = IniFile.ReadSections(FILE_SETTINGS_DEPARTMENTS);
			if (sectionNames == null) {
				LoggingService.LogMessageToFile("В файле настроек " + FILE_SETTINGS_GENERAL + " отсутсвуют секции с подразделениями");
				return;
			}

			string machineDN = DirectoryService.ReadMachineDistinguishName();
			bool isPresentInSettings = false;

			foreach (string sectionName in sectionNames) {
				if (machineDN.Contains(sectionName)) {
					isPresentInSettings = true;
					
					Settings["LPU_ADDRESS"] = IniFile.ReadValue(sectionName, KEY_REQUISITES_ADDRESS, FILE_SETTINGS_DEPARTMENTS);
					Settings["dburl"] = IniFile.ReadValue(sectionName, KEY_DB_URL, FILE_SETTINGS_DEPARTMENTS);

					break;
				}
			}

			if (!isPresentInSettings)
				LoggingService.LogMessageToFile("Не удалось найти секцию с именем филиала для ПК: " + machineDN);
		}

		private void ReadUserCertificate() {
			if (!IsSettingsFileExist(FILE_USER_CERTIFICATES))
				return;

			string userCertificate = IniFile.ReadValue("Main", Environment.UserName, FILE_USER_CERTIFICATES);
			if (string.IsNullOrEmpty(userCertificate)) {
				LoggingService.LogMessageToFile(
					"Не удалось найти сертификат для пользователя '" + 
					Environment.UserName + 
					"' в файле: " + FILE_USER_CERTIFICATES);
				return;
			}

			Settings["certwsname"] = userCertificate;
		}

		private bool IsSettingsFileExist(string file) {
			LoggingService.LogMessageToFile("Считывание параметров из файла настроек: " + file);
			if (!File.Exists(FILE_SETTINGS_GENERAL)) {
				LoggingService.LogMessageToFile("Отсутствует файл с настройками: " + file);
				return false;
			}

			return true;
		}
	}
}
