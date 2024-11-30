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
using vt.Ui;

namespace vt.Entities;

internal class SystemdUnits() : SshCommandJsonTableBase<SystemdUnit[]>("systemctl list-units --type service --full --all --output json --no-pager"), IInspectable
{
    public override async IAsyncEnumerable<InspectionPart> ProduceAsync(SystemdUnit[] value, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var basePart in base.ProduceAsync(value, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return basePart;
        }

        var unitCommandInspectables = value.Where(u => !u.Unit.Contains("systemd") && u.Load == "loaded")
            .Select(u =>
            new SshCommandText()
            {
                Host = this.Host,
                User = this.User,
                Name = u.Unit,
                Group = $"{this.Host} units: {u.Active}",
                Command = $"systemctl status {u.Unit} --no-pager -l",
                BecomeRoot = true,
            }).ToArray();
        if (unitCommandInspectables.Length > 0)
        {
            yield return new AddInspectables(unitCommandInspectables);
        }
    }

    protected override void SetupTableView(TableView tv)
    {
        tv.CellActivated += Tv_CellActivated;
    }

    private void Tv_CellActivated(TableView.CellActivatedEventArgs obj)
    {
        if (obj.Col == 0)
        {
            var unit = this.Data[obj.Row];

            var window = new SystemdUnitWindow(unit);
            Universe.Instance.PartChannel.Writer.TryWrite(new SpawnWindow(window));
        }
    }
}

public record SystemdUnit
{
    [JsonProperty(Required = Required.Always)]
    public required string Unit { get; init; }
    [JsonProperty(Required = Required.Always)]
    public required string Load { get; init; }
    [JsonProperty(Required = Required.Always)]
    public required string Active { get; init; }
    [JsonProperty(Required = Required.Always)]
    public required string Sub { get; init; }
    [JsonProperty(Required = Required.Always)]
    public required string Description { get; init; }
}
