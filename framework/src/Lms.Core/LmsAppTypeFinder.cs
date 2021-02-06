using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lms.Core
{
    public class LmsAppTypeFinder : AppDomainTypeFinder
    {
        private bool _binFolderAssembliesLoaded;
        
        public LmsAppTypeFinder(ILmsFileProvider fileProvider = null) : base(fileProvider)
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