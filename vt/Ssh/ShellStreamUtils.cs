using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vt.Ssh;

public static class ShellStreamUtils
{
    private const string StartMarker = "VT_START_MARKER";
    private const string EndMarker = "VT_END_MARKER";
    public static async Task<string> ExecuteCommandAsync(ShellStream stream, string command, CancellationToken cancellationToken)
    {
        using var closedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        EventHandler<EventArgs> closedHandler = (s, e) =>
        {
            closedCts.Cancel();
        };

        stream.Closed += closedHandler;

        var wrappedCommand = $"echo {StartMarker};{command};echo {EndMarker};exit";
        stream.WriteLine(wrappedCommand);

        byte[] buffer = new byte[1024];
        List<byte> accumulated = new();

        // the command will be echoed; read up through the first occurrence of EndMarker to discard it

        int endMarkerIndex = 0;
        byte[] endMarkerBytes = Encoding.UTF8.GetBytes(EndMarker);
        for (int i = 0; i <= wrappedCommand.Length; i++)
        {
            int b = stream.ReadByte();
            if (b < 0)
            {
                throw new Exception("Reached end of stream unexpectedly early");
            }

            // Looking for an exact match for the end marker
            if (b == endMarkerBytes[endMarkerIndex])
            {
                endMarkerIndex++;
                if (endMarkerIndex == endMarkerBytes.Length)
                {
                    // consumed enough bytes
                    break;
                }
            }
            else
            {
                endMarkerIndex = 0;
            }
        }

        while (!closedCts.IsCancellationRequested)
        {
            var bytesRead = await stream.ReadAsync(buffer, closedCts.Token);
            accumulated.AddRange(buffer.Take(bytesRead));
        }

        var resultString = Encoding.UTF8.GetString(accumulated.ToArray());

        // Strip out color escape sequences
        resultString = Regex.Replace(resultString, @"\x1b\[[0-9;]+m", "");

        int dataStart = resultString.IndexOf(StartMarker);
        if (dataStart < 0) {
            throw new Exception("invalid output, start marker was missing");
        }
        dataStart += StartMarker.Length + 1;

        int dataEnd = resultString.IndexOf(EndMarker);
        if (dataEnd < 0) {
            throw new Exception("invalid output, end marker was missing");
        }

        if (dataStart > dataEnd)
        {
            throw new Exception("start came after end??");
        }

        int dataLength = dataEnd - dataStart;

        return resultString.Substring(dataStart, dataLength).Trim();
    }
}
