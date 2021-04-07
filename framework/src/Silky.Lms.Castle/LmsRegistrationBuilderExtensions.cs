using System;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Extras.DynamicProxy;
using Silky.Lms.Castle.Adapter;
using Silky.Lms.Core.Exceptions;

namespace Silky.Lms.Castle
{
    public static class LmsRegistrationBuilderExtensions
    {

        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> AddInterceptors<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrationBuilder,
            params Type[] interceptors)
            where TActivatorData : ReflectionActivatorData
        {
            var serviceType = registrationBuilder.RegistrationData.Services.OfType<IServiceWithType>().FirstOrDefault()?.ServiceType;
            if (serviceType == null)
            {
                throw new LmsException("获取指定的注册类型失败");
            }

            if (serviceType.IsInterface)
            {
                registrationBuilder = registrationBuilder.EnableInterfaceInterceptors();
            }
            else
            {
                (registrationBuilder as IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, TRegistrationStyle>)?.EnableClassInterceptors();
            }
            foreach (var interceptor in interceptors)
            {
                registrationBuilder.InterceptedBy(
                    typeof(LmsAsyncDeterminationInterceptor<>).MakeGenericType(interceptor)
                );
            }
            return registrationBuilder;
        }

       
    }
}