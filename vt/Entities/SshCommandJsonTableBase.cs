using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Trees;
using vt.Ssh;

namespace vt.Entities;

internal class SshCommandJsonTableBase<T>(string command) : IInspectable
{
    public string Key => $"{nameof(SshCommandJsonTableBase<T>)} {this.Name}: {this.User}@{this.Host}$ {this.Command}";
    public required string Name { get; init; }

    public string? Group { get; init; }

    public required string Host { get; init; }
    public required string User { get; init; }

    public virtual string Command { get; } = command;

    public bool BecomeRoot { get; init; } = false;

    protected T? Data { get; set; } = default(T);

    public async IAsyncEnumerable<InspectionPart> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var value = await this.GetJsonValue(cancellationToken);
        this.Data = value;
        await foreach (var p in this.ProduceAsync(value, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return p;
        }
    }
    public virtual async IAsyncEnumerable<InspectionPart> ProduceAsync(T value, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (value is not DataTable dt)
        {
            // yuck
            var json = JsonConvert.SerializeObject(value);
            dt = JsonConvert.DeserializeObject<DataTable>(json) ?? throw new InvalidOperationException("failed to deserialize datatable?");
        }
        var tv = new TableView
        {
            Table = dt,
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        this.SetupTableView(tv);
        yield return new InspectionView(tv);
    }

    protected virtual void SetupTableView(TableView tv) { }

    public virtual async Task<T?> GetJsonValue(CancellationToken cancellationToken)
    {
        // speed bump to avoid the worst mistake
        if (this.Command.Contains("rm "))
        {
            throw new InvalidOperationException("command contains 'rm '");
        }

        using var client = new SshClient(this.Host, this.User, SshKeySource.Instance.GetKey());
        client.Connect();

        if (!this.BecomeRoot)
        {
            using SshCommand cmd = client.RunCommand(this.Command);
            T? value = JsonConvert.DeserializeObject<T>(cmd.Result);
            return value;
        }
        else
        {
            using ShellStream stream = client.CreateShellStream("tty", 80, 24, 800, 600, 1024);
            bool success = await Sudo.ElevateShellAsync(stream, this.Host, this.User);
            if (!success)
            {
                throw new Exception("Failed to become root");
            }
            var resultText = await ShellStreamUtils.ExecuteCommandAsync(stream, this.Command, cancellationToken);
            T? value = JsonConvert.DeserializeObject<T>(resultText);
            return value;
        }
    }

    public override string ToString()
    {
        return this.Name ?? "null";
    }
}

internal class SshCommandJsonTable(string command): SshCommandJsonTableBase<DataTable>(command)
{
}
