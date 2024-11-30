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

namespace vt.Entities;

internal class SystemdUnits() : SshCommandJsonTableBase<IEnumerable<SystemdUnit>>("systemctl list-units --type service --full --all --output json --no-pager"), IInspectable
{
    public override async IAsyncEnumerable<InspectionPart> ProduceAsync(IEnumerable<SystemdUnit> value, [EnumeratorCancellation] CancellationToken cancellationToken)
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
