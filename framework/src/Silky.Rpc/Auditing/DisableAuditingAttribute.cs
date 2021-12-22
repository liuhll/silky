using System;

namespace Silky.Rpc.Auditing;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property)]
public class DisableAuditingAttribute : Attribute
{

}