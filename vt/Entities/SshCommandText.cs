using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Trees;
using vt.Ssh;

namespace vt.Entities;

internal class SshCommandText : IInspectable
{
    public string Key => $"{nameof(SshCommandText)} {this.Name}: {this.User}@{this.Host}$ {this.Command}";

    public required string Name { get; init; }

    public string? Group { get; init; }

    public required string Host { get; init; }
    public required string User { get; init; }

    public required string Command { get; init; }

    public bool BecomeRoot { get; init; } = false;

    public async IAsyncEnumerable<InspectionPart> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // speed bump to avoid the worst mistake
        if (this.Command.Contains("rm "))
        {
            throw new InvalidOperationException("command contains 'rm '");
        }

        using var client = new SshClient(this.Host, this.User, SshKeySource.Instance.GetKey());
        client.Connect();

        if (this.BecomeRoot)
        {
            using ShellStream stream = client.CreateShellStream("tty", 80, 24, 800, 600, 1024);
            bool success = await Sudo.ElevateShellAsync(stream, this.Host, this.User);
            if (!success)
            {
                throw new Exception("Failed to become root");
            }

            var output = await ShellStreamUtils.ExecuteCommandAsync(stream, this.Command, cancellationToken);
            yield return new InspectionView(new TextView
            {
                Text = output,
                AutoSize = true,
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                WordWrap = true,
            });
        }
        else
        {
            using SshCommand cmd = client.RunCommand(this.Command);

            if (cmd.ExitStatus != 0)
            {
                yield return new InspectionView(new TextView
                {
                    Text = $"stdout:\n{cmd.Result}\n\nstderr:\n{cmd.Error}",
                    AutoSize = true,
                    X = Pos.Center(),
                    Y = Pos.Center(),
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                    WordWrap = true,
                });
            }
            else
            {
                yield return new InspectionView(new TextView
                {
                    Text = cmd.Result,
                    AutoSize = true,
                    X = Pos.Center(),
                    Y = Pos.Center(),
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                    WordWrap = true,
                });
            }
        }
    }

    public override string ToString()
    {
        return this.Name ?? "null";
    }
}
