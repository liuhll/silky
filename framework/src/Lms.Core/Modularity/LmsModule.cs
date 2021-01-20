using System;
using Autofac;

namespace Lms.Core.Modularity
{
    public abstract class LmsModule : Module,IDisposable
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
        }

        public abstract void Dispose();
    }
}