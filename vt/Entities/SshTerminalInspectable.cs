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

internal class SshTerminalInspectable : IInspectable
{
    public string Key => $"{nameof(SshTerminalInspectable)} {this.Name}: {this.User}@{this.Host}$";

    public required string Name { get; init; }

    public string? Group { get; init; }

    public required string Host { get; init; }
    public required string User { get; init; }

    public bool BecomeRoot { get; init; }

    public string? Command { get; init; }

    public async IAsyncEnumerable<InspectionPart> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var client = new SshClient(this.Host, this.User, SshKeySource.Instance.GetKey());

        var tv = new SshTerminalView(client)
        {
            AutoSize = true,
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            OnConnectedAction = this.OnConnectedAsync,
        };
        yield return new InspectionView(tv);
    }

    private async Task OnConnectedAsync(ShellStream stream)
    {
        if (this.BecomeRoot)
        {
            await this.ElevatePrompt(stream);

            if (this.Command is null)
            {
                // Empty line so that we can see the prompt.
                stream.WriteLine("");
            }
        }

        if (this.Command is not null)
        {
            stream.WriteLine(this.Command);
        }
    }

    private async Task ElevatePrompt(ShellStream stream)
    {
        bool success = await Sudo.ElevateShellAsync(stream, this.Host, this.User);
        if (!success)
        {
            throw new Exception("Didn't successfully elevate");
        }
    }

    public override string ToString()
    {
        return this.Name ?? "null";
    }
}
