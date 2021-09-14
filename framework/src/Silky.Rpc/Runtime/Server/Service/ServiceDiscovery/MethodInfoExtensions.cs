using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Core.Extensions;
using Silky.Core.MethodExecutor;

namespace Silky.Rpc.Runtime.Server
{
    internal static class MethodInfoExtensions
    {
        private static (ICollection<HttpMethodAttribute>, bool) GetHttpMethodAttributeInfos(this MethodInfo method)
        {
            var httpMethods = method.GetCustomAttributes().OfType<HttpMethodAttribute>();
            if (httpMethods.Any())
            {
                return (httpMethods.ToArray(), true);
            }

            if (method.GetParameters().All(p => p.IsSampleType()))
            {
                return (new[] { new HttpGetAttribute() }, false);
            }

            return (new[] { new HttpPostAttribute() }, false);
        }


        public static ICollection<HttpMethodInfo> GetHttpMethodInfos(this MethodInfo method)
        {
            var httpMethodAttributeInfo = method.GetHttpMethodAttributeInfos();
            var httpMethods = new List<HttpMethodInfo>();

            foreach (var httpMethodAttribute in httpMethodAttributeInfo.Item1)
            {
                var httpMethod = httpMethodAttribute.HttpMethods.First().To<HttpMethod>();
                if (!httpMethodAttributeInfo.Item2)
                {
                    if (method.Name.StartsWith("Create"))
                    {
                        httpMethod = HttpMethod.Post;
                    }

                    if (method.Name.StartsWith("Update"))
                    {
                        httpMethod = HttpMethod.Put;
                    }

                    if (method.Name.StartsWith("Delete"))
                    {
                        httpMethod = HttpMethod.Delete;
                    }

                    if (method.Name.StartsWith("Search"))
                    {
                        httpMethod = HttpMethod.Get;
                    }

                    if (method.Name.StartsWith("Query"))
                    {
                        httpMethod = HttpMethod.Get;
                    }

                    if (method.Name.StartsWith("Get"))
                    {
                        httpMethod = HttpMethod.Get;
                    }
                }

                httpMethods.Add(new HttpMethodInfo()
                {
                    IsSpecify = httpMethodAttributeInfo.Item2,
                    Template = httpMethodAttribute.Template,
                    HttpMethod = httpMethod
                });
            }

            return httpMethods;
        }
    }
}