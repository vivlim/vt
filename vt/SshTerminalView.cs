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
        await this.sshClient.ConnectAsync(cancellationToken);
        using ShellStream shellStream = this.sshClient.CreateShellStream("xterm", 80, 24, 800, 600, bufferSize);

        byte[] buffer = new byte[bufferSize];

        shellStream.WriteLine("top");

        using var closedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
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

    void SendDataToChild(byte[] data)
    {

    }

    void NotifyPtySizeChanged(int cols, int rows) 
    {
    }
}
