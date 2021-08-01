using System;
using Microsoft.AspNetCore.Authorization;

namespace Silky.Rpc.Security
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute, IAllowAnonymous
    {
    }
}