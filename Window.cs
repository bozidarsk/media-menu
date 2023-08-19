using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Linq;
using Gtk;

/*

**************** Window (window) ***************************************************************************
*                                                                                                          *
*  ************** HBox (box) ****************************************************************************  *
*  *                                                                                                    *  *
*  *  **** VBox (box-slider) ****  **** VBox (box-media) *********************************************  *  *
*  *  *                         *  *                                                                 *  *  *
*  *  *  Label (slider-status)  *  *  ** HBox (box-audio) *****************************************  *  *  *
*  *  *  VScale (slider)        *  *  *                                                           *  *  *  *
*  *  *                         *  *  *                                ** VBox (box-info) ******  *  *  *  *
*  *  *                         *  *  *                                *                       *  *  *  *  *
*  *  *                         *  *  *  Image (image)                 *  Label (name)         *  *  *  *  *
*  *  *                         *  *  *                                *  Label (description)  *  *  *  *  *
*  *  *                         *  *  *                                *                       *  *  *  *  *
*  *  *                         *  *  *                                *************************  *  *  *  *
*  *  *                         *  *  *                                                           *  *  *  *
*  *  *                         *  *  *************************************************************  *  *  *
*  *  *                         *  *                                                                 *  *  *
*  *  *                         *  *  ** HBox (box-controls) **************************************  *  *  *
*  *  *                         *  *  *                                                           *  *  *  *
*  *  *                         *  *  *  Button (previous)   Button (play-pause)   Button (next)  *  *  *  *
*  *  *                         *  *  *                                                           *  *  *  *
*  *  *                         *  *  *************************************************************  *  *  *
*  *  *                         *  *                                                                 *  *  *
*  *  ***************************  *******************************************************************  *  *
*  *                                                                                                    *  *
*  ******************************************************************************************************  *
*                                                                                                          *
************************************************************************************************************









*/

public enum WindowType 
{
	Media,
	Volume,
	Brightness,
}

public static class Window 
{
	private static Gtk.Window window;
	private static Thread timer;
	private static bool isOutside = true;
	private static bool isRunning = false;
	private static int timeAlive;
	private static readonly ThreadStart timerMethod = new ThreadStart(() => 
		{
			for (timeAlive = Config.TimeAlive; timeAlive > 0; timeAlive--) 
			{
				if (!isOutside) { timeAlive = Config.TimeAlive + 1; continue; }
				// Console.WriteLine($"TimeAlive={timeAlive}");
				Thread.Sleep(1000);
			}

			window.Hide();
			isRunning = false;
		}
	);

	private static WindowType windowType;
	private static Gtk.VBox box_media, box_info;
	private static Gtk.VScale slider;
	private static Gtk.Image image;
	private static Gtk.Label slider_status, name, description;
	private static Gtk.Button play_pause;
	public static void Update(WindowType windowType) 
	{
		Window.windowType = windowType;

		switch (windowType) 
		{
			case WindowType.Media:
				slider_status.LabelProp = Program.IsMute ? "" : "";

				bool isPlaying = Program.GetAudioInfo(out Gtk.Image audioImage, out string audioName, out string audioDescription);

				if (audioImage != null) 
				{
					image.Pixbuf = audioImage.Pixbuf.ScaleSimple(Config.ImageSize, Config.ImageSize, Gdk.InterpType.Bilinear);
					box_info.Name = "box-info";
				}
				else 
				{
					image.Pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 1, Config.ImageSize);
					box_info.Name = "box-info-noimage";
				}

				name.LabelProp = audioName;
				description.LabelProp = audioDescription;
				play_pause.Label = isPlaying ? "" : "";
				play_pause.Xalign = isPlaying ? 0.45f : 0.5f;
				slider.Value = Program.Volume;
				box_media.Show();
				break;

			case WindowType.Volume:
				slider_status.LabelProp = Program.IsMute ? "" : "";
				slider.Value = Program.Volume;
				box_media.Hide();
				break;

			case WindowType.Brightness:
				slider_status.LabelProp = "";
				slider.Value = Program.Brightness;
				box_media.Hide();
				break;
		}

		if (!isRunning) 
		{
			timer = new Thread(timerMethod);
			timer.Start();
			window.Show();
			isRunning = true;
		} else { timeAlive = Config.TimeAlive + 1; }
	}

	public static void Initialize() 
	{
		Application.Init();

		CssProvider provider = new CssProvider();
		provider.LoadFromData(Config.Css);
		StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider, 800);

		window = new Gtk.Window("media-menu");
		window.Name = "window";
		var box = new Gtk.HBox();
		box.Name = "box";
		var box_slider = new Gtk.VBox();
		box_slider.Name = "box-slider";
		slider_status = new Gtk.Label("");
		slider_status.Name = "slider-status";
		slider = new Gtk.VScale(0, 100, 5);
		slider.Name = "slider";
		box_media = new Gtk.VBox();
		box_media.Name = "box-media";
		var box_audio = new Gtk.HBox();
		box_audio.Name = "box-audio";
		image = new Gtk.Image();
		image.Name = "image";
		box_info = new Gtk.VBox();
		box_info.Name = "box-info";
		name = new Gtk.Label("Name");
		name.Name = "name";
		description = new Gtk.Label("Description");
		description.Name = "description";
		var box_controls = new Gtk.HBox();
		box_controls.Name = "box-controls";
		var previous = new Gtk.Button("");
		previous.Name = "previous";
		play_pause = new Gtk.Button("");
		play_pause.Name = "play-pause";
		var next = new Gtk.Button("");
		next.Name = "next";

		window.Add(box);
		box.PackStart(box_slider, false, false, 0);
		box_slider.PackStart(slider_status, false, false, 0);
		box_slider.PackStart(slider, true, true, 0);
		box.PackStart(box_media, false, false, 0);
		box_media.PackStart(box_audio, false, false, 0);
		box_audio.PackStart(image, false, false, 0);
		box_audio.PackStart(box_info, true, true, 0);
		box_info.PackStart(name, false, false, 0);
		box_info.PackStart(description, false, false, 0);
		box_media.PackStart(box_controls, true, true, 0);
		box_controls.PackStart(previous, false, false, 0);
		box_controls.PackStart(play_pause, false, false, 0);
		box_controls.PackStart(next, false, false, 0);

		slider.Inverted = true;
		slider.DrawValue = true;
		slider.ShowFillLevel = true;
		slider.ValuePos = PositionType.Bottom;
		slider.FormatValue += (object sender, FormatValueArgs e) => e.RetVal = e.Value.ToString() + "%";
		slider.ValueChanged += (object sender, EventArgs e) => 
		{
			switch (windowType) 
			{
				case WindowType.Media:
				case WindowType.Volume:
					Program.Volume = (int)slider.Value;
					break;
				case WindowType.Brightness:
					Program.Brightness = (int)slider.Value;
					break;
			}
		};

		previous.StyleContext.AddClass("circular");
		previous.Clicked += (object sender, EventArgs e) => Program.Previous();

		play_pause.StyleContext.AddClass("circular");
		play_pause.Clicked += (object sender, EventArgs e) => Program.PlayPause();

		next.StyleContext.AddClass("circular");
		next.Clicked += (object sender, EventArgs e) => Program.Next();

		name.SetAlignment(0f, 0f);
		description.SetAlignment(0f, 0f);

		LayerShell.InitWindow(window);
		LayerShell.SetLayer(window, LayerShell.Layer.Overlay);
		LayerShell.SetKeyboardInteractivity(window, true);
		LayerShell.SetKeyboardMode(window, LayerShell.KeyboardMode.None);
		LayerShell.SetMargin(window, LayerShell.Edge.Top, Config.WindowMarginTop);
		LayerShell.SetMargin(window, LayerShell.Edge.Right, Config.WindowMarginRight);
		LayerShell.SetMargin(window, LayerShell.Edge.Bottom, Config.WindowMarginBottom);
		LayerShell.SetMargin(window, LayerShell.Edge.Left, Config.WindowMarginLeft);
		for (int i = 0; Config.WindowAnchor != null && i < Config.WindowAnchor.Length; i++) { LayerShell.SetAnchor(window, Config.WindowAnchor[i], true); }

		window.Resizable = false;
		window.KeepAbove = true;
		window.SetDefaultSize(500, 200);
		window.ShowAll();

		EnterNotifyEventHandler setOutsideFalse = (object sender, EnterNotifyEventArgs e) => isOutside = false;
		LeaveNotifyEventHandler setOutsideTrue = (object sender, LeaveNotifyEventArgs e) => isOutside = true;
		System.Action<Widget> setEvents = (Widget widget) => 
		{
			widget.EnterNotifyEvent += setOutsideFalse;
			widget.LeaveNotifyEvent += setOutsideTrue;
		};

		setEvents(window);
		setEvents(box);
		setEvents(slider_status);
		setEvents(slider);
		setEvents(image);
		setEvents(name);
		setEvents(description);
		setEvents(previous);
		setEvents(play_pause);
		setEvents(next);

		window.Hide();
	}
}