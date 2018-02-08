using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FssAutoConfig {
	class LoggingService {
		private static string LOG_FILE_NAME = "C:\\Temp\\" + Assembly.GetExecutingAssembly().GetName().Name + "_" + Environment.UserName + ".log";

		public static void LogMessageToFile(string msg) {
			try {
				using (System.IO.StreamWriter sw = System.IO.File.AppendText(LOG_FILE_NAME)) {
					string logLine = System.String.Format("{0:G}: {1}", System.DateTime.Now, msg);
					sw.WriteLine(logLine);
				}
			} catch (Exception e) {
				Console.WriteLine("Cannot write to log file: " + LOG_FILE_NAME + " | " + e.Message + " | " + e.StackTrace);
			}

			Console.WriteLine(msg);
		}

		public static void CheckAndCleanOldFile() {
			try {
				if (File.Exists(LOG_FILE_NAME))
					File.Delete(LOG_FILE_NAME);
			} catch (Exception e) {
				Console.WriteLine("Cannot delete old lig file: " + e.Message + Environment.NewLine + e.StackTrace);
			}
		}
	}
}
