using System;
using System.Collections.Generic;
using Silky.Lms.Core.Collections;

namespace Silky.Lms.Validation
{
    public class LmsValidationOptions
    {
        public List<Type> IgnoredTypes { get; }

        public ITypeList<IObjectValidationContributor> ObjectValidationContributors { get; set; }

        public LmsValidationOptions()
        {
            IgnoredTypes = new List<Type>();
            ObjectValidationContributors = new TypeList<IObjectValidationContributor>();
        }
    }
}