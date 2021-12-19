using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace Silky.Rpc.Configuration;

public class AuditingOptions
{
    //TODO: Consider to add an option to disable auditing for application service methods?

    public const string Auditing = "Auditing";
    public bool HideErrors { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsEnabledForAnonymousUsers { get; set; }

    public bool AlwaysLogOnException { get; set; }

    public List<Type> IgnoredTypes { get; }

    public bool IsEnabledForGetRequests { get; set; }

    public AuditingOptions()
    {
        IsEnabled = true;
        IsEnabledForAnonymousUsers = true;
        HideErrors = true;
        AlwaysLogOnException = true;

        IgnoredTypes = new List<Type>
        {
            typeof(Stream),
            typeof(Expression)
        };
    }
}