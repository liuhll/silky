using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using JetBrains.Annotations;
using Lms.Core.Extensions.Collections.Generic;

namespace Lms.Core.Modularity
{
    public class LmsModuleDescriptor : ILmsModuleDescriptor
    {
        public Type Type { get; }
        public Assembly Assembly { get; }
        public ILmsModule Instance { get; }

        public string Name { get; } 

        public IReadOnlyList<ILmsModuleDescriptor>  Dependencies => _dependencies.ToImmutableList();
        
        private readonly List<ILmsModuleDescriptor> _dependencies;
        
        public LmsModuleDescriptor(
            [NotNull] Type type, 
            [NotNull] ILmsModule instance)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(instance, nameof(instance));

            if (!type.GetTypeInfo().IsAssignableFrom(instance.GetType()))
            {
                throw new ArgumentException($"Given module instance ({instance.GetType().AssemblyQualifiedName}) is not an instance of given module type: {type.AssemblyQualifiedName}");
            }

            Type = type;
            Assembly = type.Assembly;
            Instance = instance;
            Name = Instance.Name;

            _dependencies = new List<ILmsModuleDescriptor>();
        }
        
        public void AddDependency(ILmsModuleDescriptor descriptor)
        {
            _dependencies.AddIfNotContains(descriptor);
        }

        public override string ToString()
        {
            return $"[LmsModuleDescriptor {Type.FullName}]";
        }
    }
}