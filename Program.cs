using vt;
using Terminal.Gui;
using vt.Entities;

internal class Program
{
    private static void Main(string[] args)
    {
        var p = new Program();
        p.Start();
    }

    private static readonly char[] spinnerChars = ['/', '-', '\\', '|'];

    private Universe universe = new Universe();

    private readonly object changeInspectingLock = new object();
    private CancellationTokenSource inspectingCts = new();

    private Window? inspector;

    private void Start()
    {
        Application.Init();

        try
        {
            Application.Init();
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Quit", "", () => {
                        Application.RequestStop ();
                    })
                }),
            });

            var win = new Window("Hello")
            {
                X = 0,
                Y = 1,
                Width = Dim.Percent(30),
                Height = Dim.Fill() - 1
            };

            this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd a", Group = "group a" });
            this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd b", Group = "group a" });
            this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd c", Group = "group a" });
            this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd d", Group = "group b" });
            this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd e", Group = "group b" });
            this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd f", Group = "group b" });

            var treeView = new TreeView<IInspectable>()
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                TreeBuilder = new GroupedUniverseTreeBuilder(this.universe),
            };

            treeView.ObjectActivated += TreeView_ObjectActivated;
            treeView.AddObject(universe);
            treeView.ExpandAll();

            this.inspector = new Window("details")
            {
                X = Pos.Percent(30),
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 1,
            };

            win.Add(treeView);

            Application.Top.Add(menu, win, this.inspector);

            Application.Run();
        }
        finally
        {
            Application.Shutdown();
        }
    }

    void TreeView_ObjectActivated(Terminal.Gui.Trees.ObjectActivatedEventArgs<IInspectable> obj)
    {
        _ = Task.Run(() =>
        {
            return this.Inspect(obj.ActivatedObject);
        });
    }

    private async Task Inspect(IInspectable inspectable)
    {
        CancellationToken cancellation;
        lock (this.changeInspectingLock)
        {
            var oldCts = this.inspectingCts;
            this.inspectingCts = new();
            oldCts.Cancel();
            oldCts.Dispose();
            cancellation = this.inspectingCts.Token;
        }

        if (this.inspector is null)
        {
            throw new NullReferenceException("inspector is null");
        }

        this.inspector.RemoveAll();

        using var spinnerCts = new CancellationTokenSource();

        var spinnerTask = Task.Run(async () =>
        {
            int i = 0;

            try
            {
                while (!spinnerCts.Token.IsCancellationRequested)
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        this.inspector.Title = $"loading {spinnerChars[i]}";
                    });

                    i = (i + 1) % spinnerChars.Length;
                    await Task.Delay(50, spinnerCts.Token);
                }
            }
            finally
            {
                Application.MainLoop.Invoke(() =>
                {
                    this.inspector.Title = "inspector";
                });
            }
        });

        try
        {
            List<View> addedViews = new();
            await foreach (var v in inspectable.GetViewsAsync(cancellation))
            {
                Application.MainLoop.Invoke(() =>
                {
                    addedViews.Add(v);
                    this.inspector!.Add(v);
                    /*
                    var height = addedViews.Select(v => {
                        v.(out var h);
                        return h;
                    }).Sum();
                    var width = addedViews.Select(v => v.Height);
                    */
                });
            }
        }
        finally
        {
            spinnerCts.Cancel();
        }
    }
}