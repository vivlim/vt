using Terminal.Gui;

namespace vt;

/*
public class SubprocessTerminalView : TerminalView {
	int ptyFd;
	int childPid;
	
	void SendDataToChild (byte [] data)
	{
		unsafe {
			fixed (byte* p = &data [0]) {
				var n = Mono.Unix.Native.Syscall.write (ptyFd, (void*)((IntPtr)p), (ulong)data.Length);
			}
		}
	}

	void NotifyPtySizeChanged (int cols, int rows)
	{
		UnixWindowSize nz = new UnixWindowSize ();
		nz.col = (short) Frame.Width;
		nz.row = (short) Frame.Height;
		var res = Pty.SetWinSize (ptyFd, ref nz);
	}

	public SubprocessTerminalView ()
	{
		var size = new UnixWindowSize () {
			col = (short) terminal.Cols,
			row = (short) terminal.Rows,
		};

		childPid  = Pty.ForkAndExec ("/bin/bash", new string [] { "/bin/bash" }, XtermSharp.Terminal.GetEnvironmentVariables ("xterm"), out ptyFd, size);
		var unixMainLoop = Application.MainLoop.Driver as Mono.Terminal.UnixMainLoop;
		unixMainLoop.AddWatch (ptyFd, Mono.Terminal.UnixMainLoop.Condition.PollIn, PtyReady);

		this.UserInput = SendDataToChild;
		this.TerminalSizeChanged += NotifyPtySizeChanged;
	}

	byte [] buffer = new byte [8192];
	bool PtyReady (Mono.Terminal.MainLoop mainloop)
	{
		unsafe {
			long n;
			fixed (byte* p = &buffer[0]) {

				n = Mono.Unix.Native.Syscall.read (ptyFd, (void*)((IntPtr)p), (ulong)buffer.Length);
				Debug.Print(System.Text.Encoding.UTF8.GetString (buffer, 0, (int) n));
				Feed (buffer, (int)n);
			}
		}
		return true;
	}
}
*/
