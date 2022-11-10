using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Silky.Core.Configuration;

namespace Silky.Core.Reflection
{
    internal class SilkyAppTypeFinder : AppDomainTypeFinder
    {
        private bool _binFolderAssembliesLoaded;

        public SilkyAppTypeFinder([NotNull] AppServicePlugInSourceList servicePlugInSources,
            ISilkyFileProvider fileProvider = null) : base(servicePlugInSources,
            fileProvider)
        {
        }

        public bool EnsureBinFolderAssembliesLoaded { get; set; } = true;

        public virtual string GetBinDirectory()
        {
            return AppContext.BaseDirectory;
        }

        public override IList<Assembly> GetAssemblies()
        {
            if (!EnsureBinFolderAssembliesLoaded || _binFolderAssembliesLoaded)
                return base.GetAssemblies();

            _binFolderAssembliesLoaded = true;
            var binPath = GetBinDirectory();
            //binPath = _webHelper.MapPath("~/bin");
            LoadMatchingAssemblies(binPath);

            return base.GetAssemblies();
        }
    }
}