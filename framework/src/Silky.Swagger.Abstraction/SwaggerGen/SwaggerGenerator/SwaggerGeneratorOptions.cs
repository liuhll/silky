using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Silky.Rpc.Runtime.Server;
using Microsoft.OpenApi.Models;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public class SwaggerGeneratorOptions
    {
        public SwaggerGeneratorOptions()
        {
            SwaggerDocs = new Dictionary<string, OpenApiInfo>();
            DocInclusionPredicate = DefaultDocInclusionPredicate;
            OperationIdSelector = DefaultOperationIdSelector;
            TagsSelector = DefaultTagsSelector;
            SortKeySelector = DefaultSortKeySelector;
            Servers = new List<OpenApiServer>();
            SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>();
            SecurityRequirements = new List<OpenApiSecurityRequirement>();
            ParameterFilters = new List<IParameterFilter>();
            RequestBodyFilters = new List<IRequestBodyFilter>();
            OperationFilters = new List<IOperationFilter>();
            DocumentFilters = new List<IDocumentFilter>();
        }

        public IDictionary<string, OpenApiInfo> SwaggerDocs { get; set; }

        public Func<string, ServiceEntry, bool> DocInclusionPredicate { get; set; }

        public bool IgnoreObsoleteActions { get; set; }

        public Func<IEnumerable<ServiceEntry>, ServiceEntry> ConflictingActionsResolver { get; set; }

        public Func<ServiceEntry, string> OperationIdSelector { get; set; }

        public Func<ServiceEntry, IList<string>> TagsSelector { get; set; }

        public Func<ServiceEntry, string> SortKeySelector { get; set; }

        public bool DescribeAllParametersInCamelCase { get; set; }

        public List<OpenApiServer> Servers { get; set; }

        public IDictionary<string, OpenApiSecurityScheme> SecuritySchemes { get; set; }

        public IList<OpenApiSecurityRequirement> SecurityRequirements { get; set; }

        public IList<IParameterFilter> ParameterFilters { get; set; }

        public List<IRequestBodyFilter> RequestBodyFilters { get; set; }

        public List<IOperationFilter> OperationFilters { get; set; }

        public IList<IDocumentFilter> DocumentFilters { get; set; }

        private bool DefaultDocInclusionPredicate(string documentName, ServiceEntry serviceEntry)
        {
            return serviceEntry.ServiceEntryDescriptor.ServiceName == null ||
                   serviceEntry.ServiceEntryDescriptor.ServiceName == documentName ||
                   serviceEntry.ServiceEntryDescriptor.Id.Contains(documentName);
        }

        private string DefaultOperationIdSelector(ServiceEntry serviceEntry)
        {
            return serviceEntry.ServiceEntryDescriptor.ServiceName + "." + serviceEntry.ServiceEntryDescriptor.Method;
        }

        private IList<string> DefaultTagsSelector(ServiceEntry serviceEntry)
        {
            return new[] { serviceEntry.ServiceEntryDescriptor.ServiceName };
        }

        private string DefaultSortKeySelector(ServiceEntry serviceEntry)
        {
            return TagsSelector(serviceEntry).First();
        }
    }
}