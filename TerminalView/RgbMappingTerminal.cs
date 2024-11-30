using Wacton.Unicolour;
using XtermSharp;

namespace XtermSharpTerminalView;

public class RgbMappingTerminal : XtermSharp.Terminal
{
    public RgbMappingTerminal(ITerminalDelegate terminalDelegate = null, TerminalOptions options = null) : base(terminalDelegate, options)
    {
    }
    
    private static Dictionary<Unicolour, int> vgaColors = new()
    {
        [new Unicolour(ColourSpace.Rgb255, 0, 0, 0)] = 0, // black
        [new Unicolour(ColourSpace.Rgb255, 170, 0, 0)] = 1, // red
        [new Unicolour(ColourSpace.Rgb255, 0, 170, 0)] = 2, // green
        [new Unicolour(ColourSpace.Rgb255, 170, 85, 0)] = 3, // yellow
        [new Unicolour(ColourSpace.Rgb255, 0, 0, 170)] = 4, // blue
        [new Unicolour(ColourSpace.Rgb255, 170, 0, 170)] = 5, // magenta
        [new Unicolour(ColourSpace.Rgb255, 0, 170, 170)] = 6, // cyan
        [new Unicolour(ColourSpace.Rgb255, 170, 170, 170)] = 7, // white
        [new Unicolour(ColourSpace.Rgb255, 85, 85, 85)] = 60, // bright black
        [new Unicolour(ColourSpace.Rgb255, 255, 85, 85)] = 61, // bright red
        [new Unicolour(ColourSpace.Rgb255, 85, 255, 85)] = 62, // bright green
        [new Unicolour(ColourSpace.Rgb255, 255, 255, 85)] = 63, // bright yellow
        [new Unicolour(ColourSpace.Rgb255, 85, 85, 255)] = 64, // bright blue
        [new Unicolour(ColourSpace.Rgb255, 255, 85, 255)] = 65, // bright magenta
        [new Unicolour(ColourSpace.Rgb255, 85, 255, 255)] = 66, // bright cyan
        [new Unicolour(ColourSpace.Rgb255, 255, 255, 255)] = 67, // bright white
    };

    private static Dictionary<(int r, int g, int b), int> mappedColors = new();

    private static uint ColorToIndex(int r, int g, int b)
    {
        if (r > 255 || g > 255 || b > 255)
        {
            throw new InvalidOperationException("Color out of bounds");
        }
        return ColorBytesToIndex((byte)r, (byte)g, (byte)b);
    }

    private static uint ColorBytesToIndex(byte r, byte g, byte b)
    {
        return (uint)((r << 16) | (g << 8) | (b));

    }

    public override int MatchColor(int r1, int g1, int b1)
    {
        uint tableIndex = ColorToIndex(r1, g1, b1);
        
        if (mappedColors.TryGetValue((r1, g1, b1), out int nearestColor))
        {
            return nearestColor;
        }

        var thisColor = new Unicolour(ColourSpace.Rgb255, r1, g1, b1);

        IEnumerable<(double difference, Unicolour vgaColor)> differences = vgaColors.Keys.AsParallel().Select(vga => (vga.Difference(thisColor, DeltaE.Ciede2000), vga));
        (double difference, Unicolour vgaColor) closest = differences.OrderBy(d => d.difference).First();
        int colorCode = vgaColors[closest.vgaColor];

        mappedColors[(r1, g1, b1)] = colorCode;

        return colorCode;
    }
}
