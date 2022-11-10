using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using JetBrains.Annotations;
using Silky.Core.Configuration;
using Silky.Core.Extensions;

namespace Silky.Core.Reflection
{
    internal class AppDomainTypeFinder : ITypeFinder
    {
        private bool _ignoreReflectionErrors = true;
        protected ISilkyFileProvider _fileProvider;
        protected AppServicePlugInSourceList _servicePlugInSources;

        public AppDomainTypeFinder([NotNull] AppServicePlugInSourceList servicePlugInSources,
            ISilkyFileProvider fileProvider = null)
        {
            Check.NotNull(servicePlugInSources, nameof(servicePlugInSources));
            _servicePlugInSources = servicePlugInSources;
            _fileProvider = fileProvider ?? CommonSilkyHelpers.DefaultFileProvider;
        }

        #region Utilities

        private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!Matches(assembly.FullName))
                    continue;

                if (addedAssemblyNames.Contains(assembly.FullName))
                    continue;
                if (assembly.IsDynamic)
                    continue;
                assemblies.Add(assembly);
                addedAssemblyNames.Add(assembly.FullName);
            }
        }

        protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (var assemblyName in AssemblyNames)
            {
                var assembly = Assembly.Load(assemblyName);
                if (addedAssemblyNames.Contains(assembly.FullName))
                    continue;
                assemblies.Add(assembly);
                addedAssemblyNames.Add(assembly.FullName);
            }
        }

        protected virtual bool Matches(string assemblyFullName)
        {
            return AssemblyHelper.Matches(assemblyFullName);
        }

        protected virtual void LoadMatchingAssemblies(string directoryPath)
        {
            var loadedAssemblyNames = new List<string>();


            foreach (var a in GetAssemblies())
            {
                loadedAssemblyNames.Add(a.FullName);
            }

            if (!_fileProvider.DirectoryExists(directoryPath))
            {
                return;
            }

            foreach (var dllPath in _fileProvider.GetFiles(directoryPath, "*.dll"))
            {
                try
                {
                    var an = AssemblyName.GetAssemblyName(dllPath);
                    if (Matches(an.FullName) && !loadedAssemblyNames.Contains(an.FullName))
                    {
                        App.Load(an);
                    }

                    //old loading stuff
                    //Assembly a = Assembly.ReflectionOnlyLoadFrom(dllPath);
                    //if (Matches(a.FullName) && !loadedAssemblyNames.Contains(a.FullName))
                    //{
                    //    App.Load(a.FullName);
                    //}
                }
                catch (BadImageFormatException ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    if (genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition()))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        protected virtual IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies,
            bool onlyConcreteClasses = true)
        {
            var result = new List<Type>();
            try
            {
                foreach (var a in assemblies)
                {
                    Type[] types = null;
                    try
                    {
                        types = a.GetTypes();
                    }
                    catch
                    {
                        //Entity Framework 6 doesn't allow getting types (throws an exception)
                        if (!_ignoreReflectionErrors)
                        {
                            throw;
                        }
                    }

                    if (types == null)
                        continue;

                    foreach (var t in types)
                    {
                        if (!assignTypeFrom.IsAssignableFrom(t) && (!assignTypeFrom.IsGenericTypeDefinition ||
                                                                    !DoesTypeImplementOpenGeneric(t, assignTypeFrom)))
                            continue;

                        if (t.IsInterface)
                            continue;

                        if (onlyConcreteClasses)
                        {
                            if (t.IsClass && !t.IsAbstract)
                            {
                                result.Add(t);
                            }
                        }
                        else
                        {
                            result.Add(t);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                    msg += e.Message + Environment.NewLine;

                var fail = new Exception(msg, ex);
                Debug.WriteLine(fail.Message, fail);

                throw fail;
            }

            return result;
        }

        #endregion

        #region Methods

        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);
        }

        public virtual IList<Assembly> GetAssemblies()
        {
            var addedAssemblyNames = new List<string>();
            var assemblies = new List<Assembly>();

            if (LoadAppDomainAssemblies)
                AddAssembliesInAppDomain(addedAssemblyNames, assemblies);
            AddConfiguredAssemblies(addedAssemblyNames, assemblies);
            AddAppServiceAssemblies(addedAssemblyNames, assemblies);
            return assemblies;
        }

        protected virtual void AddAppServiceAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            var appServiceSources = _servicePlugInSources.GetAppServiceSources();
            foreach (var appService in appServiceSources)
            {
                LoadServiceAssemblies(appService.Folder, appService.Pattern, appService.SearchOption,
                    addedAssemblyNames, assemblies);
            }
        }

        protected virtual void LoadServiceAssemblies(string folder,
            string pattern,
            SearchOption searchOption,
            List<string> loadedAssemblyNames,
            List<Assembly> assemblies)
        {
            if (folder.IsNullOrWhiteSpace() || pattern.IsNullOrEmpty())
            {
                return;
            }

            if (!_fileProvider.DirectoryExists(folder))
            {
                return;
            }

            var assemblyFiles = AssemblyHelper.GetAssemblyFiles(folder, searchOption);
            if (!pattern.IsNullOrEmpty())
            {
                assemblyFiles = assemblyFiles.Where(a => AssemblyHelper.Matches(a, pattern));
            }

            var loadAssemblies =
                assemblyFiles.Select(f => AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(f)));
            foreach (var loadAssembly in loadAssemblies)
            {
                if (loadedAssemblyNames.Contains(loadAssembly.FullName))
                    continue;
                loadedAssemblyNames.Add(loadAssembly.FullName);
                assemblies.Add(loadAssembly);
            }
        }

        #endregion

        #region Properties

        public virtual AppDomain App => AppDomain.CurrentDomain;

        public bool LoadAppDomainAssemblies { get; set; } = true;

        public IList<string> AssemblyNames { get; set; } = new List<string>();

        #endregion
    }
}