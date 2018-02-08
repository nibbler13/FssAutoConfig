using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;

namespace FssAutoConfig {
	public class DirectoryService {
		public static void ReadUserInfoFromActiveDirectory(
			out string userDisplayName,
			out string userMail,
			out string userPhoneNumber) {
			userDisplayName = string.Empty;
			userMail = string.Empty;
			userPhoneNumber = string.Empty;

			try {
				using (DirectoryEntry user = GetDirectoryEntry(Environment.UserName)) {
					userDisplayName = user.Properties["displayName"].Value.ToString();
					userMail = user.Properties["mail"].Value.ToString();
					userPhoneNumber = user.Properties["mobile"].Value.ToString();
					if (IsPhoneNumberCorrect(userPhoneNumber))
						return;

					List<string> otherPhoneNumbers = new List<string> {
							user.Properties["pager"].Value.ToString(),
							user.Properties["homePhone"].Value.ToString(),
							user.Properties["facsimileTelephoneNumber"].Value.ToString(),
							user.Properties["ipPhone"].Value.ToString(),
							user.Properties["telephoneNumber"].Value.ToString()
						};

					foreach (string phoneNumber in otherPhoneNumbers) {
						if (IsPhoneNumberCorrect(phoneNumber)) {
							userPhoneNumber = phoneNumber;
							return;
						}
					}
				}
			} catch (Exception e) {
				LoggingService.LogMessageToFile(e.Message + Environment.NewLine + e.StackTrace);
			}
		}

		public static string ReadMachineDistinguishName() {
			try {
				using (DirectoryEntry machine = GetDirectoryEntry(Environment.MachineName + "$")) {
					return machine.Properties["distinguishedName"].Value.ToString();
				}
			} catch (Exception e) {
				LoggingService.LogMessageToFile(e.Message + Environment.NewLine + e.StackTrace);
			}

			return string.Empty;
		}

		private static DirectoryEntry GetDirectoryEntry(string sAMAccountName) {
			using (DirectorySearcher dsSearcher = new DirectorySearcher()) {
				dsSearcher.Filter = "sAMAccountName=" + sAMAccountName;
				SearchResult searchResult = dsSearcher.FindOne();
				return new DirectoryEntry(searchResult.Path);
			}
		}

		private static bool IsPhoneNumberCorrect(string phoneNumber) {
			return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length >= 7;
		}
	}
}
