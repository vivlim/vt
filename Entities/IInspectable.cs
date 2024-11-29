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
    public string? Group { get; }

    public IAsyncEnumerable<View> GetViewsAsync(CancellationToken cancellationToken);
}
