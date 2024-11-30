using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtermSharp;

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
            3 => Terminal.Gui.Color.Brown,
            4 => Terminal.Gui.Color.Blue,
            5 => Terminal.Gui.Color.Magenta,
            6 => Terminal.Gui.Color.Cyan,
            7 => Terminal.Gui.Color.Gray,
            60 => Terminal.Gui.Color.DarkGray,
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

    public static int? FromTerminalGuiColor(Terminal.Gui.Color color)
    {
        return color switch
        {
            Terminal.Gui.Color.Black => 0 ,
            Terminal.Gui.Color.Red => 1 ,
            Terminal.Gui.Color.Green => 2 ,
            Terminal.Gui.Color.Brown => 3 ,
            Terminal.Gui.Color.Blue => 4 ,
            Terminal.Gui.Color.Magenta => 5 ,
            Terminal.Gui.Color.Cyan => 6 ,
            Terminal.Gui.Color.Gray => 7 ,
            Terminal.Gui.Color.DarkGray => 60,
            Terminal.Gui.Color.BrightRed => 61 ,
            Terminal.Gui.Color.BrightGreen => 62 ,
            Terminal.Gui.Color.BrightYellow => 63 ,
            Terminal.Gui.Color.BrightBlue => 64 ,
            Terminal.Gui.Color.BrightMagenta => 65 ,
            Terminal.Gui.Color.BrightCyan => 66 ,
            Terminal.Gui.Color.White => 67 ,
            _ => null,
        };
    }


    public static byte[] ColorEscapeSequence(Terminal.Gui.Color fgColor, Terminal.Gui.Color bgColor)
    {
        if (FromTerminalGuiColor(fgColor) is not int fg || FromTerminalGuiColor(bgColor) is not int bg)
        {
            return [];
        }

        var seq = $"[0;{fg + 30};{bg + 40}m";
        return EscapeSequences.CmdEsc.Concat(Encoding.ASCII.GetBytes(seq)).ToArray();
    }
}
