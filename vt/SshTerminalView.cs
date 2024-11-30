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
            using ShellStream shellStream = this.sshClient.CreateShellStream("xterm", 80, 24, 800, 600, bufferSize);
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

        }
        finally
        {
            this.currentShellStream = null;
            closedCts.Cancel();
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
    }
}
