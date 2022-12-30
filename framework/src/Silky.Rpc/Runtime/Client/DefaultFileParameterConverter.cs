using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client;

public class DefaultFileParameterConverter : IFileParameterConverter
{
    public object[] Convert(object[] parameters)
    {
       
        var newParameters = new List<object>();
        foreach (var parameter in parameters)
        {
            if (parameter is IFormFile formFile)
            {
                var silkyForm = formFile.ConventToSilkyFile();
                newParameters.Add(silkyForm);
                continue;
            }

            if (parameter is IFormFileCollection formFileCollection)
            {
                var silkyForms = formFileCollection.ConventToSilkyFiles();
                newParameters.Add(silkyForms);
                continue;
            }
            newParameters.Add(parameter);
        }
        return newParameters.ToArray();
    }

    public IDictionary<string, object> Convert(IDictionary<string, object> parameters)
    {
      
        foreach (var parameter in parameters)
        {
            if (parameter.Value is IFormFile formFile)
            {
                var silkyForm = formFile.ConventToSilkyFile();
                parameters[parameter.Key] = silkyForm;
                continue;
            }

            if (parameter.Value is IFormFileCollection formFileCollection)
            {
                var silkyForms = formFileCollection.ConventToSilkyFiles();
                parameters[parameter.Key] = silkyForms;
            }
        }

        return parameters;
    }

    public IDictionary<ParameterFrom, object> Convert(IDictionary<ParameterFrom, object> parameters)
    {
        foreach (var parameter in parameters)
        {
            
            if (parameter.Value is IFormFile formFile)
            {
                var silkyForm = formFile.ConventToSilkyFile();
                parameters[parameter.Key] = silkyForm;
                continue;
            }

            if (parameter.Value is IFormFileCollection formFileCollection)
            {
                var silkyForms = formFileCollection.ConventToSilkyFiles();
                parameters[parameter.Key] = silkyForms;
            }
        }

        return parameters;
    }
}