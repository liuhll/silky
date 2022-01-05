using System;

namespace Silky.Rpc.Runtime.Server;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public class ProhibitExtranetAttribute : Attribute
{
}