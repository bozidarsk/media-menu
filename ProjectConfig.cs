using System;

public static partial class Config 
{
	public static int WindowMarginTop { private set; get; } = 10;
	public static int WindowMarginRight { private set; get; } = 0;
	public static int WindowMarginBottom { private set; get; } = 0;
	public static int WindowMarginLeft { private set; get; } = 10;
	public static Gtk.LayerShell.Edge[] WindowAnchor { private set; get; } = { Gtk.LayerShell.Edge.Top, Gtk.LayerShell.Edge.Left };
	public static int ImageSize { private set; get; } = 100;
	public static int TimeAlive { private set; get; } = 5;
	public static string BrightnessSetCommand { private set; get; } = @"brightnessctl set '%value%'% intel_backlight";
	public static string BrightnessGetCommand { private set; get; } = @"brightnessctl -m | sed -E 's/[^,]+,[^,]+,[^,]+,([^,]+)%.+/\1/'";
	public static string VolumeSetCommand { private set; get; } = @"pactl set-sink-volume @DEFAULT_SINK@ '%value%'%";
	public static string VolumeGetCommand { private set; get; } = @"pactl get-sink-volume @DEFAULT_SINK@ | tr -d '\n' | sed -E 's/.+\s+([0-9]+)%.+/\1/'";
	// public static string MuteSetCommand { private set; get; } = @"pactl get-sink-mute @DEFAULT_SINK@ '%value%'";
	public static string MuteGetCommand { private set; get; } = @"pactl get-sink-mute @DEFAULT_SINK@ | tr -d '\n' | sed -E 's/Mute: no/false/' | sed -E 's/Mute: yes/true/'";

	public static string Css { private set; get; } = "* {\n    font-family: \"Source Code Pro Bold\";\n    font-weight: bold;\n    transition: 0.2s;\n}\n\n#window, #box {\n    background-color: transparent;\n}\n\n#box-slider, #box-media {\n    border: 2px solid @theme_selected_bg_color;\n    border-radius: 8px;\n    background-color: @theme_base_color;\n}\n\n#slider-status, #box-controls {\n    font-family: \"Font Awesome 6 Sharp\";\n}\n\n#box-slider, #box-media {\n    padding: 10px;\n}\n\n#slider-status {\n    margin: 5px 0px;\n}\n\n#slider value {\n    margin-top: 5px;\n    color: @theme_text_color;\n}\n\n#box-media {\n    margin-left: 10px;\n    padding-bottom: 0px;\n    min-width: 415px;\n}\n\n#box-info, #box-info-noimage {\n    border-top-right-radius: 8px;\n    border-bottom-right-radius: 8px;\n    background-color: #38414e;\n}\n\n#box-info-noimage {\n    border-radius: 8px;\n}\n\n#image {\n    margin-right: -1px;\n    background-color: transparent;\n}\n\n#name {\n    margin: 15px 15px 0px 15px;\n    font-size: 20px;\n}\n\n#description {\n    margin: 10px 15px 0px 15px;\n    font-size: 12px;\n}\n\n#box-controls {\n    margin: 20px 0px;\n}\n\n#previous,\n#play-pause,\n#next {\n    border: 3px solid white;\n    min-width: 30px;\n}\n\n#play-pause {\n    margin: 0px 10px;\n}\n\n#previous {\n    margin-left: 120px;\n}";
	public static string ConfigDir { private set; get; } = Environment.GetEnvironmentVariable("HOME") + "/.config/media-menu";

	private static readonly Option[] OptionsDefinition = 
	{
		new Option("--config-dir", 'c', true, null),
	};
}