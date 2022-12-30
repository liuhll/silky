using System;
using Silky.Core.Runtime.Rpc;

namespace Silky.Rpc.Runtime;


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public class HashShuntStrategyAttribute : Attribute
{
    public const string HashKey = AttachmentKeys.HashKey;
}