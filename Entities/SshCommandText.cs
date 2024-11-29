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

internal class SshCommandText : IInspectable
{
    public string Key => $"{nameof(SshCommandText)} {this.Name}: {this.User}@{this.Host}$ {this.Command}";

    public required string Name { get; init; }

    public string? Group { get; init; }

    public required string Host { get; init; }
    public required string User { get; init; }

    public required string Command { get; init; }

    public async IAsyncEnumerable<InspectionPart> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // speed bump to avoid the worst mistake
        if (this.Command.Contains("rm "))
        {
            throw new InvalidOperationException("command contains 'rm '");
        }

        using var client = new SshClient(this.Host, this.User, SshKeySource.Instance.GetKey());
        client.Connect();
        using SshCommand cmd = client.RunCommand(this.Command);
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

    public override string ToString()
    {
        return this.Name ?? "null";
    }
}
