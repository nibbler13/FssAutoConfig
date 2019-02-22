using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FssAutoConfig {
	public class SettingsReader {
		private readonly string fileSettingsGeneral = Path.Combine(Program.rootFolder, "GeneralSettings.ini");
		private readonly string fileSettingsDepartments = Path.Combine(Program.rootFolder, "DepartmentsSettings.ini");
		private readonly string fileSettingsOrganizationUnitsSettings = Path.Combine(Program.rootFolder, "OrganizationUnitsSettings.ini");
        private readonly string fileUserCertificatesINI = Path.Combine(Program.rootFolder, "UserCertificates.ini");
		private readonly string fileUserCertificatesKontur = Path.Combine(Program.rootFolder, "rc1000193-all.csv");

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
		//private const string KEY_CERIFICATES_CERTWSNAME = "Имя сертификата МО"; //Replaced by UserCertificates.ini
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
			ReadUserCertificate(userDisplayName);

			LoggingService.LogMessageToFile("Считанные настройки: ");
			foreach (KeyValuePair<string, string> pair in Settings)
				LoggingService.LogMessageToFile(pair.Key + "=" + pair.Value);
		}

		private void ReadGeneralSettins() {
			if (!IsSettingsFileExist(fileSettingsGeneral))
				return;

			string[] requisites = IniFile.ReadSections(fileSettingsGeneral);
			if (requisites == null) {
				LoggingService.LogMessageToFile("Отсутствует секции с настройками");
				return;
			}
				
			if (requisites.Contains(SECTION_REQUISITES)) {
				Settings["LPU_NAME"] = IniFile.ReadValue(SECTION_REQUISITES, KEY_REQUISITES_NAME, fileSettingsGeneral);
				Settings["LPU_FULL_NAME"] = IniFile.ReadValue(SECTION_REQUISITES, KEY_REQUISITES_FULLNAME, fileSettingsGeneral);
				Settings["LPU_ORGN"] = IniFile.ReadValue(SECTION_REQUISITES, KEY_REQUISITES_OGRN, fileSettingsGeneral);
				Settings["LPU_OKPO"] = IniFile.ReadValue(SECTION_REQUISITES, KEY_REQUISITES_OKPO, fileSettingsGeneral);
			} else
				LoggingService.LogMessageToFile("Отсутсвует секция: " + SECTION_REQUISITES);

			if (requisites.Contains(SECTION_FSS_SERVICES)) {
				Settings["wsurl"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_URL, fileSettingsGeneral);
				Settings["wstimeout"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_TIMEOUT, fileSettingsGeneral);
				Settings["netuseproxy"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_USEPROXY, fileSettingsGeneral);
				Settings["netproxyhost"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYHOST, fileSettingsGeneral);
				Settings["netproxyport"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYPORT, fileSettingsGeneral);
				Settings["netuseproxyauth"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYAUTH, fileSettingsGeneral);
				Settings["netproxyuser"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYUSER, fileSettingsGeneral);
				Settings["netproxypassword"] = IniFile.ReadValue(SECTION_FSS_SERVICES, KEY_FSS_SERVICES_PROXYPASSWORD, fileSettingsGeneral);
			} else
				LoggingService.LogMessageToFile("Отсутсвует секция: " + SECTION_FSS_SERVICES);

			if (requisites.Contains(SECTION_DB)) {
				Settings["dbuser"] = IniFile.ReadValue(SECTION_DB, KEY_DB_USER, fileSettingsGeneral);
				Settings["dbpassword"] = IniFile.ReadValue(SECTION_DB, KEY_DB_PASSWORD, fileSettingsGeneral);
			} else
				LoggingService.LogMessageToFile("Отсутсвует секция: " + SECTION_DB);

			if (requisites.Contains(SECTION_CERIFICATES)) {
				Settings["performvalidation"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_PERFORMVALIDATION, fileSettingsGeneral);
				Settings["signoutmessage"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_SIGNOUTMESSAGE, fileSettingsGeneral);
				Settings["vefifyinmessage"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_VERIFYMESSAGE, fileSettingsGeneral);
				Settings["hprov"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_HPROV, fileSettingsGeneral);
				Settings["keywscontainer"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_KEYWSCONTAINER, fileSettingsGeneral);
				Settings["encryptmessages"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_ENCRYPTMESSAGES, fileSettingsGeneral);
				Settings["FSSkeywscontainer"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_FSSKEYWSCONTAINER, fileSettingsGeneral);
				Settings["certfssname"] = IniFile.ReadValue(SECTION_CERIFICATES, KEY_CERIFICATES_CERTFSSNAME, fileSettingsGeneral);
			} else
				LoggingService.LogMessageToFile("Отсутсвует секция: " + SECTION_CERIFICATES);
		}

		private void ReadDepartmentSettings() {
			if (!IsSettingsFileExist(fileSettingsDepartments) ||
                !IsSettingsFileExist(fileSettingsOrganizationUnitsSettings))
				return;

            string machineDN = DirectoryService.ReadMachineDistinguishName();
            bool isPresentInSettings = false;

            string[] sectionNamesOU = IniFile.ReadSections(fileSettingsOrganizationUnitsSettings);
            if (sectionNamesOU != null) {
                foreach (string sectionName in sectionNamesOU) {
                    if (machineDN.EndsWith(sectionName)) {
                        isPresentInSettings = true;

                        Settings["LPU_ADDRESS"] = IniFile.ReadValue(sectionName, KEY_REQUISITES_ADDRESS, fileSettingsOrganizationUnitsSettings);
                        Settings["dburl"] = IniFile.ReadValue(sectionName, KEY_DB_URL, fileSettingsOrganizationUnitsSettings);

                        break;
                    }
                }
            }

			string[] sectionNamesDepts = IniFile.ReadSections(fileSettingsDepartments);
			if (sectionNamesDepts != null && !isPresentInSettings) {
                foreach (string sectionName in sectionNamesDepts) {
                    if (machineDN.Contains(sectionName)) {
                        isPresentInSettings = true;

                        Settings["LPU_ADDRESS"] = IniFile.ReadValue(sectionName, KEY_REQUISITES_ADDRESS, fileSettingsDepartments);
                        Settings["dburl"] = IniFile.ReadValue(sectionName, KEY_DB_URL, fileSettingsDepartments);

                        break;
                    }
                }
            }

			if (!isPresentInSettings)
				LoggingService.LogMessageToFile("Не удалось найти секцию с именем филиала для ПК: " + machineDN);
		}

		private void ReadUserCertificate(string userName) {
			string userCertificate = string.Empty;

			if (IsSettingsFileExist(fileUserCertificatesINI))
				userCertificate = IniFile.ReadValue("Main", Environment.UserName, fileUserCertificatesINI);

			if (string.IsNullOrEmpty(userCertificate) && 
				IsSettingsFileExist(fileUserCertificatesKontur)) {
				try {
					using (TextFieldParser csvParser = new TextFieldParser(fileUserCertificatesKontur, Encoding.GetEncoding("windows-1251"))) {
						csvParser.CommentTokens = new string[] { "#" };
						csvParser.SetDelimiters(new string[] { ";" });
						csvParser.HasFieldsEnclosedInQuotes = true;
						csvParser.ReadLine();

						DateTime validTo = new DateTime();

						while (!csvParser.EndOfData) {
							string[] fields = csvParser.ReadFields();
							if (fields.Length < 14)
								continue;

							string lineUserName = fields[3];
							if (!ClearString(lineUserName).Equals(ClearString(userName)))
								continue;

							string isRevoked = fields[10];
							if (isRevoked.Equals("1"))
								continue;

							string lineCertificateSN = fields[6];
							DateTime lineValidTo = DateTime.Parse(fields[9]);
							if (string.IsNullOrEmpty(userCertificate)) {
								userCertificate = lineCertificateSN;
								validTo = lineValidTo;
								continue;
							}

							if (lineValidTo <= validTo)
								continue;

							userCertificate = lineCertificateSN;
							validTo = lineValidTo;
						}
					}
				} catch (Exception e) {
					LoggingService.LogMessageToFile(e.Message + Environment.NewLine + e.StackTrace);
				}
			}

			if (string.IsNullOrEmpty(userCertificate)) {
				string message = "Не удалось найти сертификат для пользователя '" +
					Environment.UserName + "@" + Environment.MachineName + 
					"' в файлах: " + fileUserCertificatesINI +
					", " + fileUserCertificatesKontur;
				LoggingService.LogMessageToFile(message);
				SystemMail.SendMail("FssAutoConfig", message);
				return;
			}

			Settings["certwsname"] = userCertificate;
		}

		private static string ClearString(string text) {
			return text.ToLower().TrimStart(' ').TrimEnd(' ').Replace("  ", " ");
		}

		private bool IsSettingsFileExist(string file) {
			LoggingService.LogMessageToFile("Считывание параметров из файла настроек: " + file);
			if (!File.Exists(fileSettingsGeneral)) {
				LoggingService.LogMessageToFile("Отсутствует файл с настройками: " + file);
				return false;
			}

			return true;
		}
	}
}
