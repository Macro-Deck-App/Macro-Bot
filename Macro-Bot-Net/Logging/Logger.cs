namespace Develeon64.MacroBot.Logging {
	public static class Logger {
		private static Levels level = Levels.Trace;

		public static void Initialize (Levels level) {
			Logger.level = level;
		}

		public static void Log (Modules module, Levels level, string message) {
			if (level >= Logger.level) {
				string[] lines = message.Split('\n');
				string line = $"{DateTime.Now:d} {DateTime.Now:T} | {Enum.GetName(module).PadRight(8).Substring(0, 8)} | {Enum.GetName(level).PadRight(8).Substring(0, 8)} | {lines[0]}";
				for (int i = 1; i < lines.Length; i++)
					line += "\n                                            " + lines[i];

				Logger.LogFile(line);
				Logger.LogConsole(line);
			}
		}

		public static void Trace (Modules module, string message) {
			Logger.Log(module, Levels.Trace, message);
		}

		public static void Debug (Modules module, string message) {
			Logger.Log(module, Levels.Debug, message);
		}

		public static void Info (Modules module, string message) {
			Logger.Log(module, Levels.Info, message);
		}

		public static void Warning (Modules module, string message) {
			Logger.Log(module, Levels.Warning, message);
		}

		public static void Error (Modules module, string message) {
			Logger.Log(module, Levels.Error, message);
		}

		public static void Critical (Modules module, string message) {
			Logger.Log(module, Levels.Critical, message);
		}

		private static void LogConsole (string text) {
			Console.WriteLine(text);
		}

		private static void LogFile (string text) {
			string fileName = $"MacroBot_{DateTime.Now:d}.log";
			if (!Directory.Exists("Logs"))
				Directory.CreateDirectory("Logs");
			if (!File.Exists($"Logs\\{fileName}")) {
				File.Create($"Logs\\{fileName}").Close();
				Logger.Debug(Modules.System, "Log-File created!");
			}

			File.AppendAllText($"Logs\\{fileName}", $"{text}\n");
		}
	}
}
