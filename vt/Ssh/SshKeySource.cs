using Org.BouncyCastle.Security;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vt;

public class SshKeySource
{
    public static readonly SshKeySource Instance = new SshKeySource();

    private SshKeySource()
    {

    }

    public PrivateKeyFile GetKey(string? keyName = null)
    {
        if (keyName is null)
        {
            keyName = "id_ed25519";
        }

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", keyName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Can't find {path}");
        }

        return new PrivateKeyFile(path);
    }
}
