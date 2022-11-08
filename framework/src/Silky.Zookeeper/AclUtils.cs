using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using org.apache.zookeeper;
using org.apache.zookeeper.data;

namespace Silky.Zookeeper;

public static class AclUtils
{
    public static AuthScheme GetAuthScheme(string auth)
    {
        if (string.IsNullOrEmpty(auth))
        {
            return AuthScheme.World;
        }

        return Enum.Parse<AuthScheme>(auth);
    }

    // public static List<ACL> GetAcls(string scheme, string auth, ZooDefs.Perms perms = ZooDefs.Perms.ALL)
    // {
    //     var userName = auth.Split(":").First();
    //     var id = new Id(scheme, userName);
    //     return new List<ACL>()
    //     {
    //         new((int)perms, id)
    //     };
    // }

    public static List<ACL> GetAcls(AuthScheme scheme, string auth, params ZooDefs.Perms[] perms)
    {
        if (scheme == AuthScheme.Digest)
        {
            auth = GenerateDigest(auth);
        }
        var id = new Id(scheme.ToString().ToLower(), auth);
        var aclList = new List<ACL>();
        if (perms.Any())
        {
            foreach (var perm in perms)
            {
                aclList.Add(new ACL((int)perm,id));
            }
        }
        else
        {
            aclList.Add(new ACL((int)ZooDefs.Perms.ALL,id));
        }
        return aclList;
    }

    public static string GenerateDigest(string iPassword)
    {
        if (string.IsNullOrEmpty(iPassword))
        {
            throw new ArgumentNullException(nameof(iPassword));
        }

        using var sha1 = SHA1.Create();
        var authInfo = iPassword.Split(":");
        var digest = sha1.ComputeHash(Encoding.UTF8.GetBytes(iPassword));
        var userName = authInfo[0];
        return userName + ":" + Convert.ToBase64String(digest);
    }
    
}