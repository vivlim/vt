using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace vt.Entities;

public interface IInspectable
{
    public string Key { get; }
    public string? Group { get; }

    public IAsyncEnumerable<InspectionPart> GetViewsAsync(CancellationToken cancellationToken);
}

public record InspectionPart();

public record InspectionView(View view) : InspectionPart;

public record AddInspectables(IInspectable[] NewInspectables) : InspectionPart;
