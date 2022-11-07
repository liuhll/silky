using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Silky.Rpc.Runtime.Server;
using Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator;

namespace Silky.Swagger.Abstraction.SwaggerGen.XmlComments
{
    public class XmlCommentsDocumentFilter : IDocumentFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";

        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsDocumentFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (context.ServiceEntries == null)
            {
                return;
            }
            // Collect (unique) controller names and types in a dictionary
            var appServiceNamesAndTypes = context.ServiceEntries
                .Select(apiDesc => apiDesc as ServiceEntry)
                .SkipWhile(actionDesc => actionDesc == null)
                .GroupBy(actionDesc => actionDesc.ServiceEntryDescriptor.ServiceName)
                .Select(group => new KeyValuePair<string, Type>(group.Key, group.First().MethodInfo.DeclaringType));

            foreach (var nameAndType in appServiceNamesAndTypes)
            {
                var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(nameAndType.Value);
                var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

                if (typeNode != null)
                {
                    var summaryNode = typeNode.SelectSingleNode(SummaryTag);
                    if (summaryNode != null)
                    {
                        if (swaggerDoc.Tags == null)
                            swaggerDoc.Tags = new List<OpenApiTag>();

                        swaggerDoc.Tags.Add(new OpenApiTag
                        {
                            Name = nameAndType.Key,
                            Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml)
                        });
                    }
                }
            }
        }
    }
}