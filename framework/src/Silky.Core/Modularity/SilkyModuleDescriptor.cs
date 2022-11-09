using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using JetBrains.Annotations;
using Silky.Core.Extensions.Collections.Generic;

namespace Silky.Core.Modularity
{
    public class SilkyModuleDescriptor : ISilkyModuleDescriptor
    {
        public Type Type { get; }
        public Assembly Assembly { get; }
        public ISilkyModule Instance { get; }

        public string Name { get; }
        
        public bool IsLoadedAsPlugIn { get; }

        public IReadOnlyList<ISilkyModuleDescriptor> Dependencies => _dependencies.ToImmutableList();

        private readonly List<ISilkyModuleDescriptor> _dependencies;

        public SilkyModuleDescriptor([NotNull] Type type,
            [NotNull] ISilkyModule instance, bool isLoadedAsPlugIn)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(instance, nameof(instance));

            if (!type.GetTypeInfo().IsAssignableFrom(instance.GetType()))
            {
                throw new ArgumentException(
                    $"Given module instance ({instance.GetType().AssemblyQualifiedName}) is not an instance of given module type: {type.AssemblyQualifiedName}");
            }

            Type = type;
            Assembly = type.Assembly;
            Instance = instance;
            IsLoadedAsPlugIn = isLoadedAsPlugIn;
            Name = Instance.Name;
            _dependencies = new List<ISilkyModuleDescriptor>();
        }

        public void AddDependency(ISilkyModuleDescriptor descriptor)
        {
            _dependencies.AddIfNotContains(descriptor);
        }

        public override string ToString()
        {
            return $"[SilkyModuleDescriptor {Type.FullName}]";
        }
    }
}