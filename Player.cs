using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;

public class Player 
{
	public string Name { private set; get; }
	public string Title { get => GetProperty("xesam:title"); }
	public string Artist { get => GetProperty("xesam:artist"); }
	public string Album { get => GetProperty("xesam:album"); }
	public string ImageUrl { get => GetProperty("mpris:artUrl"); }

	private IntPtr Handle;

	[DllImport("playerctl")] private static extern IntPtr playerctl_player_previous(IntPtr player, IntPtr gerror);
	public void Previous() => playerctl_player_previous(this.Handle, IntPtr.Zero);

	[DllImport("playerctl")] private static extern IntPtr playerctl_player_play_pause(IntPtr player, IntPtr gerror);
	public void PlayPause() => playerctl_player_play_pause(this.Handle, IntPtr.Zero);

	[DllImport("playerctl")] private static extern IntPtr playerctl_player_next(IntPtr player, IntPtr gerror);
	public void Next() => playerctl_player_next(this.Handle, IntPtr.Zero);

	public static Player GetLastPlayer() 
	{
		string name = Program.Shell(@"tail -1 /tmp/media-menu-lastplayer | tr -d '\n'");
		return !string.IsNullOrEmpty(name) 
			? new Player(
				IGetAllPlayers()
				.Where(x => x.Contains(name) || x == name)
				.ToArray()[0]
			)
			: null
		;
	}

	public static Player[] GetAllPlayers() => 
		IGetAllPlayers()
		.Select(x => new Player(x))
		.ToArray()
	;

	private static IEnumerable<string> IGetAllPlayers() => 
		Program.Shell(@"playerctl -l")
		.Split('\n')
		.Where(x => !string.IsNullOrEmpty(x))
	;

	[DllImport("playerctl")] private static extern IntPtr playerctl_player_print_metadata_prop(IntPtr player, IntPtr property, IntPtr gerror);
	private string GetProperty(string property) 
	{
		IntPtr ptr = playerctl_player_print_metadata_prop(this.Handle, Marshal.StringToCoTaskMemUTF8(property), IntPtr.Zero);
		return (ptr != IntPtr.Zero) ? Marshal.PtrToStringUTF8(ptr) : null;
	}

	[DllImport("playerctl")] private static extern IntPtr playerctl_player_new(IntPtr name, IntPtr gerror);
	private Player(string name) 
	{
		this.Handle = playerctl_player_new(Marshal.StringToCoTaskMemUTF8(name), IntPtr.Zero);
		this.Name = name;
	}
}