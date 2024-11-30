using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtermSharpTerminalView;

public static class AnsiColor
{
    public static Terminal.Gui.Color ToTerminalGuiColor(int ansiColor, Terminal.Gui.Color fallback)
    {
        return ansiColor switch
        {
            0 => Terminal.Gui.Color.Black,
            1 => Terminal.Gui.Color.Red,
            2 => Terminal.Gui.Color.Green,
            3 => Terminal.Gui.Color.BrightYellow,
            4 => Terminal.Gui.Color.Blue,
            5 => Terminal.Gui.Color.Magenta,
            6 => Terminal.Gui.Color.Cyan,
            7 => Terminal.Gui.Color.White,
            60 => Terminal.Gui.Color.Black,
            61 => Terminal.Gui.Color.BrightRed,
            62 => Terminal.Gui.Color.BrightGreen,
            63 => Terminal.Gui.Color.BrightYellow,
            64 => Terminal.Gui.Color.BrightBlue,
            65 => Terminal.Gui.Color.BrightMagenta,
            66 => Terminal.Gui.Color.BrightCyan,
            67 => Terminal.Gui.Color.White,
            _ => fallback,
        };
    }
}
