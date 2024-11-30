using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vt.Ui;
using XtermSharp;

namespace vt.Ssh;

public static class Sudo
{
    private static readonly ConcurrentDictionary<(string machine, string username), string> rememberedPasswords = new();

    public static async Task<bool> ElevateShellAsync(ShellStream shellStream, string machineName, string username, bool exitAfter = true)
    {
        shellStream.WriteLine("sudo -k $(ps -o exe --no-headers $$)" + (exitAfter ? "; exit" : ""));
        string? prompt = shellStream.Expect(new Regex($@"\[sudo\] password for {username}:"));
        if (prompt is null)
        {
            throw new InvalidOperationException("didn't see expected sudo prompt");
        }

        var password = await GetPasswordAsync(machineName, username);

        if (password == null)
        {
            shellStream.WriteByte(0x03); // Ctrl+C
            shellStream.Flush();
            return false;
        }

        shellStream.WriteLine(password);
        prompt = shellStream.Expect(new Regex(@"root.*#"));
        if (prompt is null)
        {
            // forget password, because it's wrong.

            rememberedPasswords.Remove((machineName, username), out var _);
            throw new InvalidOperationException("password was incorrect");
        }

        return true;
    }

    private static async Task<string?> GetPasswordAsync(string machineName, string username)
    {
        if (rememberedPasswords.TryGetValue((machineName, username), out string? password))
        {
            return password;
        }

        var pd = new PromptDialog($"sudo: enter password for {username}@{machineName}");
        password = await pd.Show();

        if (password is not null)
        {
            rememberedPasswords[(machineName, username)] = password;
        }

        return password;
    }
}
