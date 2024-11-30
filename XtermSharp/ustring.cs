using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtermSharp;

public struct Utf8String(byte[] bytes)
{
    public readonly byte[] Bytes = bytes;

    public override string ToString()
    {
        return UTF8Encoding.Unicode.GetString(this.Bytes);
    }

    public static Utf8String From(Rune rune)
    {
        byte[] b = new byte[rune.Utf8SequenceLength];
        rune.EncodeToUtf8(b);

        return new(b);
    }

    public static Utf8String From(params Rune[] runes)
    {
        var len = runes.Select(r => r.Utf8SequenceLength).Sum();
        byte[] b = new byte[len];
        int pos = 0;
        foreach (var rune in runes)
        {
            int rlen = rune.EncodeToUtf8(b.AsSpan(pos));
            pos += rlen;
        }

        return new(b);
    }

    public static Utf8String From(char c)
    {
        var rune = new Rune(c);
        return From(rune);
    }

    // viv: this could seriously use a unit test or several
    public Utf8String Replace(Utf8String target, Utf8String replacement)
    {
        var replacementSpan = replacement.Bytes.AsSpan();

        List<byte> builder = new(this.Bytes.Length);

        int pos = 0;
        while (pos < this.Bytes.Length)
        {
            var span = this.Bytes.AsSpan(pos);
            var nextReplacement = span.IndexOf(replacementSpan);

            if (nextReplacement == -1)
            {
                builder.AddRange(span);
                break;
            }
            else
            {
                builder.AddRange(span[..nextReplacement]);
                builder.AddRange(replacementSpan);
                pos += nextReplacement + replacementSpan.Length;
            }
        }

        return new(builder.ToArray());
    }

    public int IndexOf(Utf8String target)
    {
        var span = this.Bytes.AsSpan();
        var targetSpan = target.Bytes.AsSpan();

        return span.IndexOf(targetSpan);
    }

    public static readonly Utf8String Empty = new Utf8String([]);
}
