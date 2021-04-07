using System;
using System.Linq;
using System.Reflection;
using Silky.Lms.Core;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Core.Extensions.Collections.Generic;
using Silky.Lms.Core.Utils;

namespace Silky.Lms.Validation
{
    public class MethodInvocationValidator : IMethodInvocationValidator, ITransientDependency
    {
        private readonly IObjectValidator _objectValidator;

        public MethodInvocationValidator(IObjectValidator objectValidator)
        {
            _objectValidator = objectValidator;
        }

        public virtual void Validate(MethodInvocationValidationContext context)
        {
            Check.NotNull(context, nameof(context));

            if (context.Parameters.IsNullOrEmpty())
            {
                return;
            }

            if (!context.Method.IsPublic)
            {
                return;
            }

            if (IsValidationDisabled(context))
            {
                return;
            }

            if (context.Parameters.Length != context.ParameterValues.Length)
            {
                throw new Exception("Method parameter count does not match with argument count!");
            }

            //todo: consider to remove this condition
            if (context.Errors.Any() && HasSingleNullArgument(context))
            {
                ThrowValidationError(context);
            }

            AddMethodParameterValidationErrors(context);

            if (context.Errors.Any())
            {
                ThrowValidationError(context);
            }
        }

        protected virtual bool IsValidationDisabled(MethodInvocationValidationContext context)
        {
            if (context.Method.IsDefined(typeof(EnableValidationAttribute), true))
            {
                return false;
            }
            
            if (ReflectionHelper.GetSingleAttributeOfMemberOrDeclaringTypeOrDefault<DisableValidationAttribute>(context.Method) != null)
            {
                return true;
            }

            return false;
        }

        protected virtual bool HasSingleNullArgument(MethodInvocationValidationContext context)
        {
            return context.Parameters.Length == 1 && context.ParameterValues[0] == null;
        }

        protected virtual void ThrowValidationError(MethodInvocationValidationContext context)
        {
            throw new ValidationException(
                "输入参数校验未通过",
                context.Errors         
            );
        }

        protected virtual void AddMethodParameterValidationErrors(MethodInvocationValidationContext context)
        {
            for (var i = 0; i < context.Parameters.Length; i++)
            {
                AddMethodParameterValidationErrors(context, context.Parameters[i], context.ParameterValues[i]);
            }
        }

        protected virtual void AddMethodParameterValidationErrors(ILmsValidationResult context, ParameterInfo parameterInfo, object parameterValue)
        {
            var allowNulls = parameterInfo.IsOptional ||
                             parameterInfo.IsOut ||
                             TypeHelper.IsPrimitiveExtended(parameterInfo.ParameterType, includeEnums: true);

            context.Errors.AddRange(
                _objectValidator.GetErrors(
                    parameterValue,
                    parameterInfo.Name,
                    allowNulls
                )
            );
        }
    }
}