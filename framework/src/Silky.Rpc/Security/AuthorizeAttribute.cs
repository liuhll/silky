using System;
using Microsoft.AspNetCore.Authorization;

namespace Silky.Rpc.Security
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute, IAuthorizeData
    {
        public AuthorizeAttribute()
        {
        }

        public AuthorizeAttribute(string policy) => this.Policy = policy;

        public string? Policy { get; set; }

        public string? Roles { get; set; }

        public string? AuthenticationSchemes { get; set; }
    }
}