using System;
using System.Collections.Generic;
using Silky.Core.Collections;

namespace Silky.Validation
{
    public class SilkyValidationOptions
    {
        public List<Type> IgnoredTypes { get; }

        public ITypeList<IObjectValidationContributor> ObjectValidationContributors { get; set; }

        public SilkyValidationOptions()
        {
            IgnoredTypes = new List<Type>();
            ObjectValidationContributors = new TypeList<IObjectValidationContributor>();
        }
    }
}