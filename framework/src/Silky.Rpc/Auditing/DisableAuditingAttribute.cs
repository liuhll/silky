using System;

namespace Silky.Rpc.Auditing;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class DisableAuditingAttribute : Attribute
{

}