using System;

namespace Silky.Rpc.Runtime.Server.Parameter
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class HashKeyAttribute : Attribute, IHashKeyProvider
    {
    }
}