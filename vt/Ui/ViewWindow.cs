using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using vt.Entities;

namespace vt.Ui;

public abstract class ViewWindow
{
    public abstract string Title { get; }

    public View CreateWindow()
    {
        var window = new Window()
        {
            Width = Dim.Percent(60),
            Height = Dim.Percent(60),
            X = Pos.Percent(100),
            Y = Pos.Center(),
            Title = this.Title,
        };

        this.CreateViews(window);

        return window;
    }

    public abstract void CreateViews(Window window);
}


public class SystemdUnitWindow(SystemdUnit unitData) : ViewWindow
{
    public override string Title => unitData.Unit;

    public override void CreateViews(Window window)
    {
        var label = new Label()
        {
            Text = unitData.Description,
            X = Pos.Center(),
            Y = Pos.Center(),
        };
        window.Add(label);
    }
}
