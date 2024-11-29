using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace vt.Entities;

internal class InspectableAsyncCommand : IInspectable
{
    public required string Name { get; init; }

    public string? Group { get; init; }

    public async IAsyncEnumerable<View> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 0; i < 10; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine($"Emitting label {this.Name}: {i}");
            yield return new Label()
            {
                Text = $"{this.Name}: label {i}",
                AutoSize = true,
                X = 1,
                Y = i,
            };
            await Task.Delay(1000);
        }

    }

    public override string ToString()
    {
        return this.Name ?? "null";
    }
}
