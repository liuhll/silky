using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Silky.Http.Swagger.Internal;
using Silky.Swagger.Abstraction.SwaggerUI;

namespace Silky.Http.Swagger.Configuration
{
    public class SwaggerDocumentOptions
    {
        internal static string SwaggerDocument = "SwaggerDocument";

        public SwaggerDocumentOptions()
        {
            OrganizationMode = OrganizationMode.AllAndGroup;
            ShowMode = ShowMode.All;
            EnableMultipleServiceKey = true;
            ShowDashboardService = false;
            Description = "Swagger Document";
            Title = "Swagger UI";
            Version = "v1.0.0";
            Groups = new List<GroupDescription>();
            RoutePrefix = "";
            EnableAuthorized = true;
            if (EnableAuthorized)
            {
                SecurityDefinitions ??= new SpecificationOpenApiSecurityScheme[]
                {
                    new()
                    {
                        Id = "Bearer",
                        Type = SecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        Description = "JWT Authorization header using the Bearer scheme.",
                        BearerFormat = "JWT",
                        Scheme = "Bearer",
                        In = ParameterLocation.Header,
                        Requirement = new SpecificationOpenApiSecurityRequirementItem
                        {
                            Scheme = new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                },
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                            },
                            Accesses = Array.Empty<string>()
                        }
                    }
                };
            }
        }

        public OrganizationMode OrganizationMode { get; set; }
        
        public ShowMode ShowMode { get; set; }

        public bool EnableMultipleServiceKey { get; set; }

        public string Description { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public Uri TermsOfService { get; set; }

        public OpenApiContact Contact { get; set; }

        public IEnumerable<GroupDescription> Groups { get; set; }

        public bool FormatAsV2 { get; set; } = true;

        public string RoutePrefix { get; set; }

        public DocExpansion DocExpansionState { get; set; }
        public bool EnableAuthorized { get; set; }

        public SpecificationOpenApiSecurityScheme[] SecurityDefinitions { get; set; }

        public string[] XmlComments { get; set; }
        
        public bool ShowDashboardService { get; set; }
    }

    public enum OrganizationMode
    {
        NoGroup = 0,
        Group,
        AllAndGroup
    }

    public enum ShowMode
    {
        All,
        Interface,
        RegisterCenter
    }

    public class GroupDescription
    {
        public GroupDescription()
        {
        }

        public GroupDescription(string groupName, SwaggerDocumentOptions swaggerDocumentOptions)
        {
            ApplicationInterface = groupName;
            Title = groupName;
            Description = swaggerDocumentOptions.Description;
            Version = swaggerDocumentOptions.Version ?? "v1.0.0";
            TermsOfService = swaggerDocumentOptions.TermsOfService;
            Contact = swaggerDocumentOptions.Contact;
            // Version = "v1";
        }

        public string ApplicationInterface { get; set; }

        public string Description { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public Uri TermsOfService { get; set; }

        public OpenApiContact Contact { get; set; }
    }
}