using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace vt.Entities;

internal class SshInteractiveShell : IInspectable
{
    public string Key => $"{nameof(SshInteractiveShell)} {this.Name}: {this.User}@{this.Host}$";

    public required string Name { get; init; }

    public string? Group { get; init; }

    public required string Host { get; init; }
    public required string User { get; init; }

    public async IAsyncEnumerable<InspectionPart> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var client = new SshClient(this.Host, this.User, SshKeySource.Instance.GetKey());
        client.Connect();
        var tv = new TextView
        {
            AutoSize = true,
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            WordWrap = true,
        };
        yield return new InspectionView(tv);

        using ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
        var sb = new StringBuilder();

        try
        {
            shellStream.WriteLine("top");
            string line;
            while ((line = shellStream.ReadLine(TimeSpan.FromSeconds(2))) != null)
            {
                sb.AppendLine(line);
                Application.MainLoop.Invoke(() =>
                {
                    tv.Text = sb.ToString();
                });
            }

            await Task.Delay(2000, cancellationToken);
        }
        finally
        {
            client.Disconnect();
        }
    }

    public override string ToString()
    {
        return this.Name ?? "null";
    }
}
