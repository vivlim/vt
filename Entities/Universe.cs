using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace vt.Entities;

internal class Universe : IInspectable
{
    public string? Group => null;

    public List<IInspectable> Items { get; } = new();

    public async IAsyncEnumerable<View> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new Label()
        {
            Text = "universe..."
        };
    }

    public override string ToString()
    {
        return "/";
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

    public async IAsyncEnumerable<View> GetViewsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new Label()
        {
            Text = "node with items"
        };
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
                Items = group.ToList()
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
