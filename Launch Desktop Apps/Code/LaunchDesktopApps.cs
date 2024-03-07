using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

public class CPHInline
{
	// TODO: REPLACE THE CONTENTS WITHIN QUOTATION MARKS WITH THE ABSOLUTE DIRECTORY OF YOUR EXE FILE @""
	// YOU CAN ADD AS MANY COMMA SEPARATED EXE FILES AS YOU WANT CONTAIN IN @""

	public readonly List<string> ProgramPaths = new List<string>() {
		@"D:\OBS Studio\obs-studio\bin\64bit\obs64.exe",
		@"E:\TwitchSpeaker-0.0.48-x64 (1)\TwitchSpeaker.exe",
		@"E:\VSeeFace-v1.13.38\VSeeFace\VSeeFace.exe",
		@"E:\tits-windows\titsNew\Data\Application\Twitch Integrated Throwing System.exe",
	};




	public bool Execute()
	{
		foreach(string exePath in ProgramPaths)
		{
			// check if a process of the same name is already opened, if so skip this file
			var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exePath));
			if (processes.Length > 0)
			{
				continue;
			}
			// Initialize process
			var processStartInfo = new ProcessStartInfo();
			processStartInfo.CreateNoWindow = true;
			processStartInfo.FileName = Path.GetFileName(exePath);
			processStartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);

			// Launch the application
			Process.Start(processStartInfo);
			
		}
		return true;
	}
}
