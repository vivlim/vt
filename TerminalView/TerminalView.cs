using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using XtermSharp;

namespace XtermSharpTerminalView;

/// <summary>
/// Based on XtermSharp's GuiCsHost.TerminalView.
/// </summary>
public class TerminalView : View, ITerminalDelegate {
	internal RgbMappingTerminal terminal;
	bool cursesDriver = Application.Driver.GetType ().Name.IndexOf ("CursesDriver") != -1;
	bool terminalSupportsUtf8;

	bool mouseEventsIncludePressAndRelease = false;

	public TerminalView ()
	{
		terminal = new(this, new TerminalOptions () { Cols = 80, Rows = 25 });
		CanFocus = true;

		if (cursesDriver)
			terminalSupportsUtf8 = Environment.GetEnvironmentVariable ("LANG").IndexOf ("UTF-8", StringComparison.OrdinalIgnoreCase) != -1;
		else
			terminalSupportsUtf8 = true;
	}

    protected override void OnViewportChanged(DrawEventArgs e)
    {
        base.OnViewportChanged(e);

		var value = e.NewViewport;

        if (terminal is not null)
        {
            if (value.Width != terminal.Cols || value.Height != terminal.Rows) {
                terminal.Resize (value.Width, value.Height);
            }
        }

        TerminalSizeChanged?.Invoke (value.Width, value.Height);
    }

	/// <summary>
	///  This event is raised when the terminal size has change, due to a Gui.CS frame changed.
	/// </summary>
	public event Action<int, int> TerminalSizeChanged;

    //public override bool ProcessKey (KeyEvent keyEvent)
    public override bool OnKeyDown(Key keyEvent)
    {
		switch (keyEvent.KeyCode) {
		case KeyCode.Esc:
			Send (0x1b);
			break;
		case KeyCode.Space:
			Send (0x20);
			break;
		case KeyCode.Delete:
			Send (EscapeSequences.CmdDelKey);
			break;
		case KeyCode.Backspace:
			Send (0x7f);
			break;
		case KeyCode.CursorUp:
			Send (terminal.ApplicationCursor ? EscapeSequences.MoveUpApp : EscapeSequences.MoveUpNormal);
			break;
		case KeyCode.CursorDown:
			Send (terminal.ApplicationCursor ? EscapeSequences.MoveDownApp : EscapeSequences.MoveDownNormal);
			break;
		case KeyCode.CursorLeft:
			Send (terminal.ApplicationCursor ? EscapeSequences.MoveLeftApp : EscapeSequences.MoveLeftNormal);
			break;
		case KeyCode.CursorRight:
			Send (terminal.ApplicationCursor ? EscapeSequences.MoveRightApp : EscapeSequences.MoveRightNormal);
			break;
		case KeyCode.PageUp:
			if (terminal.ApplicationCursor)
				Send (EscapeSequences.CmdPageUp);
			else {
				// TODO: view should scroll one page up.
			}
			break;
		case KeyCode.PageDown:
			if (terminal.ApplicationCursor)
				Send (EscapeSequences.CmdPageDown);
			else {
				// TODO: view should scroll one page down
			}
			break;
		case KeyCode.Home:
			Send (terminal.ApplicationCursor ? EscapeSequences.MoveHomeApp : EscapeSequences.MoveHomeNormal);
			break;
		case KeyCode.End:
			Send (terminal.ApplicationCursor ? EscapeSequences.MoveEndApp : EscapeSequences.MoveEndNormal);
			break;
		case KeyCode.Insert:
			break;
		case KeyCode.F1:
			Send (EscapeSequences.CmdF [0]);
			break;
		case KeyCode.F2:
			Send (EscapeSequences.CmdF [1]);
			break;
		case KeyCode.F3:
			Send (EscapeSequences.CmdF [2]);
			break;
		case KeyCode.F4:
			Send (EscapeSequences.CmdF [3]);
			break;
		case KeyCode.F5:
			Send (EscapeSequences.CmdF [4]);
			break;
		case KeyCode.F6:
			Send (EscapeSequences.CmdF [5]);
			break;
		case KeyCode.F7:
			Send (EscapeSequences.CmdF [6]);
			break;
		case KeyCode.F8:
			Send (EscapeSequences.CmdF [7]);
			break;
		case KeyCode.F9:
			Send (EscapeSequences.CmdF [8]);
			break;
		case KeyCode.F10:
			Send (EscapeSequences.CmdF [9]);
			break;
				/*
				 * viv: i don't know what backtab is
		case KeyCode.BackTab:
			Send (EscapeSequences.CmdBackTab);
			break;
				*/
		default:
			if (keyEvent.IsCtrl)
			{
                var keyWithoutCtrl = keyEvent.NoCtrl;
				if (keyWithoutCtrl >= Key.A && keyWithoutCtrl <= Key.Z)
				{
					byte ctrlKey = (byte)(keyWithoutCtrl.KeyCode - 64);
					Send (ctrlKey);
					break;
				}
			}
			if (keyEvent.IsAlt) {
				Send (0x1b);
			}
			byte[] keyBytes = BitConverter.GetBytes((uint)keyEvent.KeyCode);

            var decodeStatus = Rune.DecodeFromUtf8(keyBytes, out Rune rune, out int len);
            if (decodeStatus is not System.Buffers.OperationStatus.Done)
            {
                // can't send this, i guess
                return false;
            }

			//var len = Rune.RuneLen (rune);
			if (len > 0) {
				var buff = new byte [len];
				var n = rune.Encode(buff);
				Send (buff);
			} else {
				Send ((byte)keyEvent.KeyCode);
			}
			break;
		}
		return true;
	}

	void SetAttribute (int attribute, bool invert)
	{
		int bg = attribute & 0x1ff;
		int fg = (attribute >> 9) & 0x1ff;
		var flags = (FLAGS)(attribute >> 18);

		if (cursesDriver) {
			var cattr = 0;
			if (fg == Renderer.DefaultColor && bg == Renderer.DefaultColor)
				cattr = 0;
			else {
				if (fg == Renderer.DefaultColor)
					fg = (short)ConsoleColor.Gray;
				if (bg == Renderer.DefaultColor)
					bg = (short)ConsoleColor.Black;

				// viv: todo after v2
				//Driver.SetColors ((ConsoleColor)fg, (ConsoleColor)bg);
				return;
			}

			if (flags.HasFlag (FLAGS.BOLD))
				cattr |= 0x200000; // A_BOLD
			if (flags.HasFlag (FLAGS.INVERSE))
				cattr |= 0x40000; // A_REVERSE
			if (flags.HasFlag (FLAGS.BLINK))
				cattr |= 0x80000; // A_BLINK
			if (flags.HasFlag (FLAGS.DIM))
				cattr |= 0x100000; // A_DIM
			if (flags.HasFlag (FLAGS.UNDERLINE))
				cattr |= 0x20000; // A_UNDERLINE
			if (flags.HasFlag (FLAGS.ITALIC))
				cattr |= 0x10000; // A_STANDOUT
			Driver.SetAttribute (new Terminal.Gui.Attribute (cattr));
		} else {
			var fgColor = AnsiColor.ToTerminalGuiColor(fg, ColorScheme.Normal.Foreground);
			var bgColor = AnsiColor.ToTerminalGuiColor(bg, ColorScheme.Normal.Background);

			if (!invert)
			{
				Driver.SetAttribute(new Terminal.Gui.Attribute(fgColor, bgColor));
			}
			else
			{
				Driver.SetAttribute(new Terminal.Gui.Attribute(bgColor, fgColor));
			}
		}
	}

    public override void OnDrawContent(Rectangle viewport)
	{
		Driver.SetAttribute (ColorScheme.Normal);
		Clear ();

		var maxCol = Frame.Width;
		var maxRow = Frame.Height;
		var yDisp = terminal.Buffer.YDisp;

		var cursorX = terminal.Buffer.X;
		var cursorY = terminal.Buffer.Y;
		
		for (int row = 0; row < maxRow; row++) {
			Move (Frame.X, Frame.Y + row);
			if (row >= terminal.Rows)
				continue;
			var line = terminal.Buffer.Lines [row+yDisp];
			for (int col = 0; col < maxCol; col++) {
				var ch = line [col];
				SetAttribute (ch.Attribute, row == cursorY && col == cursorX);
				Rune r;

				if (ch.Code == 0)
					r = new(' ');
				else {
					if (terminalSupportsUtf8)
						r = ch.Rune;
					else {
						switch (ch.Code) {
						case 0:
							r = new(' ');
							break;
						case 0x2518: // '┘'
							r = new(0x40006a); // ACS_LRCORNER;
							break;
						case 0x2510: // '┐'
							r = new(0x40006b); // ACS_URCORNER;
							break;
						case 0x250c: // '┌'
							r = new(0x40006c); // ACS_ULCORNER;
							break;
						case 0x2514: // '└'
							r = new(0x40006d); // ACS_LLCORNER;
							break;
						case 0x253c: // '┼'
							r = new(0x40006e); // ACS_PLUS;
							break;
						case 0x23ba: // '⎺'
						case 0x23bb: // '⎻'
						case 0x2500: // '─'
						case 0x23bc: // '⎼'
						case 0x23bd: // '⎽'
							r = new(0x400071); // ACS_VLINE
							break;
						case 0x251c: // '├'
							r = new(0x400074); // ACS_LTEE
							break;
						case 0x2524: // '┤'
							r = new(0x400075); // ACS_RTEE
							break;
						case 0x2534: // '┴'
							r = new(0x400076); // ACS_BTEE
							break;
						case 0x252c: // '┬'
							r = new(0x400077); // ACS_TTEE
							break;
						case 0x2502: // '│'
							r = new(0x400078); // ACS_VLINE
							break;
						default:
							r = ch.Rune;
							break;
						}
					}
				}
				AddRune (col, row, r);
			}
		}
		PositionCursor ();
	}

	protected override bool OnMouseEvent (MouseEvent mouseEvent)
	{
		if (terminal.MouseMode is not MouseMode.Off) {

			var f = mouseEvent.Flags;
			int button = -1;
			int action = -1; // 1: press, 2: release, 3: click

			if (f.HasFlag(MouseFlags.Button1Clicked))
			{
				button = 0;
				action = 3;
			}
			if (f.HasFlag (MouseFlags.Button1Pressed))
			{
				button = 0;
				action = 1;
			}
			if (f.HasFlag (MouseFlags.Button1Released))
            {
                button = 0;
                action = 2;
            }
			if (f.HasFlag (MouseFlags.Button2Clicked))
            {
                button = 1;
                action = 3;
            }
			if (f.HasFlag (MouseFlags.Button2Pressed))
            {
                button = 1;
                action = 1;
            }
			if (f.HasFlag (MouseFlags.Button2Released))
            {
                button = 1;
                action = 2;
            }
			if (f.HasFlag(MouseFlags.Button3Clicked))
			{
				button = 2;
				action = 3;
			}
			if (f.HasFlag(MouseFlags.Button3Pressed))
			{
				button = 2;
				action = 1;
			}
			if (f.HasFlag(MouseFlags.Button3Released))
			{
				button = 2;
				action = 2;
			}
			if (f.HasFlag(MouseFlags.WheeledUp))
			{
				var e = terminal.EncodeMouseButton (4, release: false, shift: false, meta: false, control: false);
                terminal.SendEvent (e, mouseEvent.Position.X, mouseEvent.Position.Y);
				if (terminal.MouseMode is MouseMode.VT200 or MouseMode.ButtonEventTracking or MouseMode.AnyEvent) {
					e = terminal.EncodeMouseButton(4, release: true, shift: false, meta: false, control: false);
					terminal.SendEvent (e, mouseEvent.Position.X, mouseEvent.Position.Y);
				}
				return true;
			}
			if (f.HasFlag(MouseFlags.WheeledDown))
			{
				var e = terminal.EncodeMouseButton (5, release: false, shift: false, meta: false, control: false);
                terminal.SendEvent (e, mouseEvent.Position.X, mouseEvent.Position.Y);
				if (terminal.MouseMode is MouseMode.VT200 or MouseMode.ButtonEventTracking or MouseMode.AnyEvent) {
					e = terminal.EncodeMouseButton(5, release: true, shift: false, meta: false, control: false);
					terminal.SendEvent (e, mouseEvent.Position.X, mouseEvent.Position.Y);
				}
				return true;
			}

			bool mouseModeWithReleaseEvents = terminal.MouseMode is MouseMode.VT200 or MouseMode.ButtonEventTracking or MouseMode.AnyEvent;

			if  (button != -1){
				if (!this.mouseEventsIncludePressAndRelease && (action == 1 || action == 2))
				{
					// set flag so we don't fake clicking
					this.mouseEventsIncludePressAndRelease = true;
				}

				if (this.mouseEventsIncludePressAndRelease && action == 3)
				{
					// Ignore click. we're just sending the presses and releases. 
					return true;
				}
				else if (action == 3 && !this.mouseEventsIncludePressAndRelease)
				{
					// when we see a click, simulate it by sending a press and release (only if we haven't seen a press and release - those are preferable)
                    var e = terminal.EncodeMouseButton (button, release: false, shift: false, meta: false, control: false);
                    terminal.SendEvent (e, mouseEvent.Position.X, mouseEvent.Position.Y);

                    // If in one of the mouse modes where we need to send release events, send those too
                    if (terminal.MouseMode is MouseMode.VT200 or MouseMode.ButtonEventTracking or MouseMode.AnyEvent) {
                        e = terminal.EncodeMouseButton(button, release: true, shift: false, meta: false, control: false);
                        terminal.SendEvent (e, mouseEvent.Position.X, mouseEvent.Position.Y);
                    }
				}
				else if (this.mouseEventsIncludePressAndRelease && mouseModeWithReleaseEvents && action == 1)
				{
					if (f.HasFlag(MouseFlags.ReportMousePosition))
					{
						terminal.SendMouseMotion(terminal.EncodeMouseButton(button, release: false, false, false, false), mouseEvent.Position.X, mouseEvent.Position.Y);
						return true;
					}
					else
					{
                        // press
                        var e = terminal.EncodeMouseButton (button, release: false, shift: false, meta: false, control: false);
                        terminal.SendEvent (e, mouseEvent.Position.X, mouseEvent.Position.Y);
					}
				}
				else if (this.mouseEventsIncludePressAndRelease && mouseModeWithReleaseEvents && action == 2)
				{
					// release
                    var e = terminal.EncodeMouseButton (button, release: true, shift: false, meta: false, control: false);
                    terminal.SendEvent (e, mouseEvent.Position.X, mouseEvent.Position.Y);
				}
				return true;
			}
		} else {
			// Not currently handled

		}
		return false;
	}

	public Action<byte []> UserInput;

	byte [] miniBuf = new byte [1];

	void Send (byte b)
	{
		miniBuf [0] = b;
		Send (miniBuf);
	}

	public void Send (byte [] data)
	{
		UserInput?.Invoke (data);
	}

	public void SetTerminalTitle (XtermSharp.Terminal source, string title)
	{
		//
	}
	public void SetTerminalIconTitle (XtermSharp.Terminal source, string title) { }

	public void ShowCursor (XtermSharp.Terminal source)
	{
		//
	}

	public void SizeChanged (XtermSharp.Terminal source)
	{
		// Triggered by the terminal
	}

	public string WindowCommand (XtermSharp.Terminal source, WindowManipulationCommand command, params int [] args)
	{
		return null;
	}

	/* not implemented in v2 yet...
	public override void PositionCursor ()
	{
		Move (terminal.Buffer.X, terminal.Buffer.Y);
	}
	*/

	bool UpdateDisplay ()
	{
		terminal.GetUpdateRange (out var rowStart, out var rowEnd);
		terminal.ClearUpdateRange ();
		var cols = terminal.Cols;
		var tb = terminal.Buffer;
		SetNeedsDisplay (new Rectangle (0, rowStart, Frame.Width, rowEnd+1));
		//SetNeedsDisplay ();
		pendingDisplay = false;
		return false;
	}

	bool pendingDisplay;
	void QueuePendingDisplay ()
	{
		// throttle
		if (!pendingDisplay) {
			pendingDisplay = true;
			Application.AddTimeout (TimeSpan.FromMilliseconds (1), UpdateDisplay);
		}
	}

	public void Feed (byte [] buffer, int n)
	{
		terminal.Feed (buffer, n);
		QueuePendingDisplay ();
	}

    public bool IsProcessTrusted()
    {
		return false;
    }
}
