using System;

namespace Silky.Core.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
public class InjectNamedAttribute : Attribute
{
    public InjectNamedAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }
}