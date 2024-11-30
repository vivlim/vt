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

internal class SshTerminalInspectable : IInspectable
{
    public string Key => $"{nameof(SshTerminalInspectable)} {this.Name}: {this.User}@{this.Host}$";

    public required string Name { get; init; }

    public string? Group { get; init; }

    public required string Host { get; init; }
    public required string User { get; init; }

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
        };
        yield return new InspectionView(tv);
    }

    public override string ToString()
    {
        return this.Name ?? "null";
    }
}
