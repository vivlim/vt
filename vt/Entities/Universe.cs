using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace vt.Entities;

internal class Universe : IInspectable
{
    public string? Group => null;

    public IEnumerable<IInspectable> Items => this.items.Values;

    private readonly ConcurrentDictionary<string, IInspectable> items = new();

    public string Key => nameof(Universe);

    public Channel<InspectionPart> PartChannel { get; } = Channel.CreateUnbounded<InspectionPart>();

    public Universe()
    {
    }

    public async IAsyncEnumerable<InspectionPart> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new InspectionView(new Label()
        {
            Text = "universe..."
        });
    }

    public override string ToString()
    {
        return "/";
    }

    public void UpsertItem(IInspectable item)
    {
        var key = item.Key;
        items[key] = item;
    }
}

internal abstract class UniverseViewTreeBuilder : ITreeBuilder<IInspectable>
{
    private readonly Universe universe;

    public UniverseViewTreeBuilder(Universe universe)
    {
        this.universe = universe;
    }

    public bool SupportsCanExpand => false;

    bool ITreeBuilder<IInspectable>.CanExpand(IInspectable toExpand)
    {
        throw new NotImplementedException();
    }

    IEnumerable<IInspectable> ITreeBuilder<IInspectable>.GetChildren(IInspectable forObject)
    {
        if (forObject is Universe universe)
        {
            return this.BuildUniverseView(universe);
        }

        return this.GetChildrenOf(forObject);
    }

    protected abstract IEnumerable<IInspectable> BuildUniverseView(Universe universe);

    protected abstract IEnumerable<IInspectable> GetChildrenOf(IInspectable inspectable);
}

public class ViewGroupNode : IInspectable
{
    public string? Group => "ViewNode";

    public IList<IInspectable> Items { get; init; }

    public string Name { get; init; }

    public string Key => $"{this.Name}[]";

    public async IAsyncEnumerable<InspectionPart> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new InspectionView(new Label()
        {
            Text = $"{this.Name}: {this.Items.Count}",
        });
    }

    public override string ToString()
    {
        return this.Name;
    }
}

internal class GroupedUniverseTreeBuilder(Universe universe) : UniverseViewTreeBuilder(universe)
{
    protected override IEnumerable<IInspectable> BuildUniverseView(Universe universe)
    {
        foreach (var group in universe.Items.GroupBy(i => i.Group))
        {
            if (group.Key is null)
            {
                continue;
            }

            yield return new ViewGroupNode()
            {
                Name = group.Key,
                Items = group.OrderBy(i => i.ToString()).ToList()
            };
        }
    }

    protected override IEnumerable<IInspectable> GetChildrenOf(IInspectable inspectable)
    {
        if (inspectable is ViewGroupNode vgn)
        {
            return vgn.Items;
        }

        return [];
    }
}
