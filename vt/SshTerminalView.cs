using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using XtermSharpTerminalView;

namespace vt;

public class SshTerminalView : TerminalView
{
    private const int bufferSize = 1024;
    private readonly IMainLoopDriver mainLoopDriver;
    private readonly SshClient sshClient;
    public Func<ShellStream, Task>? WithConnection { get; init; }

    private ShellStream? currentShellStream = null;

    private TaskCompletionSource<(uint rows, uint cols)> initialSizeTcs = new();

    public SshTerminalView(SshClient sshClient) : base()
    {
        this.mainLoopDriver = Application.MainLoop.Driver;
        this.sshClient = sshClient;

        this.UserInput = SendDataToChild;
        this.TerminalSizeChanged += NotifyPtySizeChanged;

        Task.Run(() => this.ConnectAsync(CancellationToken.None));
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        using var closedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            await this.sshClient.ConnectAsync(cancellationToken);
            var initialSize = await this.initialSizeTcs.Task;
            using ShellStream shellStream = this.sshClient.CreateShellStream("xterm", initialSize.cols, initialSize.rows, 800, 600, bufferSize);
            this.currentShellStream = shellStream;

            byte[] buffer = new byte[bufferSize];

            //shellStream.WriteLine("htop");

            shellStream.Closed += (s, e) =>
            {
                closedCts.Cancel();
            };

            while (!closedCts.IsCancellationRequested)
            {
                var bytesRead = await shellStream.ReadAsync(buffer, closedCts.Token);
                var finishFeedTcs = new TaskCompletionSource();
                Application.MainLoop.Invoke(() =>
                {
                    try
                    {
                this.Feed(buffer, bytesRead);
                        finishFeedTcs.TrySetResult();
                    }
                    catch (Exception ex)
                    {
                        finishFeedTcs.TrySetException(ex);
                    }
                });
                await finishFeedTcs.Task;
            }
        }
        catch (Exception ex)
        {
            // dump the exception into the terminal.
            var exceptionMessage = AnsiColor.ColorEscapeSequence(Color.Red, Color.Black).Concat(Encoding.ASCII.GetBytes($"\n\ncaught exception: {ex}\n")).ToArray();
            this.Feed(exceptionMessage, exceptionMessage.Length);
        }
        finally
        {
            this.currentShellStream = null;
            closedCts.Cancel();
            this.sshClient.Dispose();

            var endMessage = AnsiColor.ColorEscapeSequence(Color.Red, Color.Black).Concat(Encoding.ASCII.GetBytes("\n\n[ connection was closed ]\n")).ToArray();

            this.Feed(endMessage,endMessage.Length);
        }
    }

    void SendDataToChild(byte[] data)
    {
        if (this.currentShellStream is null)
        {
            return;
        }

        this.currentShellStream.Write(data, 0, data.Length);
        this.currentShellStream.Flush();

    }

    void NotifyPtySizeChanged(int cols, int rows) 
    {
        this.initialSizeTcs.TrySetResult(((uint)rows, (uint)cols));
        if (this.currentShellStream is null)
        {
            return;
        }

        // Actually handling this is blocked on pr https://github.com/sshnet/SSH.NET/pull/1062.
        return;
    }
}
