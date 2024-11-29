namespace vt;
using Terminal.Gui;
using vt.Entities;

public class MyView : Terminal.Gui.Window {

    private TreeView<IInspectable> treeView;
    private Terminal.Gui.Label label1;
    private Terminal.Gui.Button button1;
    private Universe universe = new();

    
    private void InitializeComponent() {
        this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd a", Group = "group a" });
        this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd b", Group = "group a" });
        this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd c", Group = "group a" });
        this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd d", Group = "group b" });
        this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd e", Group = "group b" });
        this.universe.Items.Add(new InspectableAsyncCommand() { Name = "cmd f", Group = "group b" });

        this.treeView = new TreeView<IInspectable>()
        {
            X = 30,
            Y = 30,
            Width = 40,
            Height = 40,
            TreeBuilder = new GroupedUniverseTreeBuilder(this.universe),
        };
        treeView.AddObject(universe);

        //this.Add(this.treeView);
    }
    public MyView() {
        InitializeComponent();
    }
}
