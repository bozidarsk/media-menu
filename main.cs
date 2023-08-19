using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.IO;

public static class Program 
{
	private static readonly string SHELL = Environment.GetEnvironmentVariable("SHELL");
	private static Player LastPlayer;

	public static bool IsMute 
	{
		// set => Shell(Config.MuteSetCommand.Replace("%value%", value.ToString()));
		get => bool.Parse(Shell(Config.MuteGetCommand));
	}

	public static int Volume 
	{
		set => Shell(Config.VolumeSetCommand.Replace("%value%", value.ToString()));
		get => int.Parse(Shell(Config.VolumeGetCommand));
	}

	public static int Brightness 
	{
		set => Shell(Config.BrightnessSetCommand.Replace("%value%", value.ToString()));
		get => int.Parse(Shell(Config.BrightnessGetCommand));
	}

	public static void Previous() => LastPlayer.Previous();
	public static void PlayPause() => LastPlayer.PlayPause();
	public static void Next() => LastPlayer.Next();

	public static bool GetAudioInfo(out Gtk.Image image, out string name, out string description) 
	{
		name = LastPlayer.Title;
		description = LastPlayer.Artist;

		string url = LastPlayer.ImageUrl;
		if (!string.IsNullOrEmpty(url)) 
		{
			Shell($"curl '{url}' 2> /dev/null > /tmp/media-menu-lastplayer");
			image = new Gtk.Image("/tmp/media-menu-image");
		} else { image = null; }

		return Shell($"playerctl -p '{LastPlayer.Name}' status") == "Playing\n";
	}

	public static string Shell(string command) 
	{
		ProcessStartInfo info = new ProcessStartInfo();
		info.FileName = SHELL;
		info.Arguments = $"-c \"{command}\"";
		info.UseShellExecute = false;
		info.RedirectStandardOutput = true;
		info.RedirectStandardError = false;

		Process proc = new Process();
		proc.StartInfo = info;
		proc.Start();
		proc.WaitForExit();

		return proc.StandardOutput.ReadToEnd();
	}

	[DllImport("glib-2.0")] private static extern uint g_idle_add(IntPtr function, IntPtr data);
	private delegate bool GSourceFunc(IntPtr data);

	private static void KeyboardHook() 
	{
		Func<IntPtr, bool> onMedia = (IntPtr data) => 
		{
			LastPlayer = Player.GetLastPlayer();
			if (LastPlayer != null) { Window.Update(WindowType.Media); }
			return false;
		};
		Func<IntPtr, bool> onVolume = (IntPtr data) => 
		{
			LastPlayer = Player.GetLastPlayer();
			Window.Update((LastPlayer != null) ? WindowType.Media : WindowType.Volume);
			return false;
		};
		Func<IntPtr, bool> onBrightness = (IntPtr data) => 
		{
			Window.Update(WindowType.Brightness);
			return false;
		};

		string line;
		while ((line = Console.ReadLine()) != null) 
		{
			if (!line.Contains("KEYBOARD_KEY")) { continue; }

			line = line.Remove(0, line.IndexOf("KEY_"));
			string[] tokens = line.Split(' ');

			Input.KeyCode key = (Input.KeyCode)int.Parse(tokens[1].Substring(1, tokens[1].Length - 2));
			Input.KeyState state = Enum.Parse<Input.KeyState>(tokens[2], true);

			if (state != Input.KeyState.Pressed) { continue; }

			switch (key) 
			{
				case Input.KeyCode.PlayPause:
				case Input.KeyCode.NextSong:
				case Input.KeyCode.PreviousSong:
					g_idle_add(Marshal.GetFunctionPointerForDelegate(onMedia), IntPtr.Zero);
					break;
				case Input.KeyCode.VolumeUp:
				case Input.KeyCode.VolumeDown:
				case Input.KeyCode.Mute:
					g_idle_add(Marshal.GetFunctionPointerForDelegate(onVolume), IntPtr.Zero);
					break;
				case Input.KeyCode.BrightnessUp:
				case Input.KeyCode.BrightnessDown:
					Thread.Sleep(50);
					g_idle_add(Marshal.GetFunctionPointerForDelegate(onBrightness), IntPtr.Zero);
					break;
			}
		}
	}

	private static int Main(string[] args) 
	{
		if (args.Length == 0 || (args.Length == 1 && (args[0] == "-h" || args[0] == "--help" || args[0] == "help"))) 
		{
			Console.WriteLine("Usage:\n\tmedia-menu <command (not window)> [options]");
			Console.WriteLine("\tlibinput debug-events --show-keycodes | media-menu window [options]");
			Console.WriteLine("\nCommands:");
			Console.WriteLine("\twindow              Opens a gtk3 gui window.");
			Console.WriteLine("\tdefaults            Creates default configuration and style files.");
			Console.WriteLine("\nOptions:");
			Console.WriteLine("\t--config-dir <dir>  Directory which contains configuration and style files.");
			return 0;
		}

		Config.Initialize(ref args);

		if (args.Length == 0) { Console.WriteLine("No commands provided."); return 1; }
		switch (args[0]) 
		{
			case "window":
				if (File.Exists("/tmp/media-menu-lastplayer")) { File.Delete("/tmp/media-menu-lastplayer"); }
				Process.Start(SHELL, "-c \"playerctl status --format '{{ playerName }}' --follow > /tmp/media-menu-lastplayer\"");

				Window.Initialize();
				LastPlayer = Player.GetLastPlayer();

				new Thread(() => KeyboardHook()).Start();
				Gtk.Application.Run();

				break;
			case "defaults":
				Config.CreateDefaults();
				break;
			default:
				Console.WriteLine("Unknown command.");
				return 1;
		}		

		return 0;
	}
}