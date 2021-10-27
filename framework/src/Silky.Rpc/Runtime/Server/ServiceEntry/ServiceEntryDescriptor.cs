using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Core.Rpc;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntryDescriptor
    {
        public ServiceEntryDescriptor()
        {
            Metadatas = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public string Id { get; set; }
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Method { get; set; }
        public string WebApi { get; set; }
        public HttpMethod? HttpMethod { get; set; }
        public ServiceProtocol ServiceProtocol { get; set; }

        public IDictionary<string, object> Metadatas { get; set; }

        public bool ProhibitExtranet { get; set; }

        public bool IsAllowAnonymous { get; set; }
        public bool IsDistributeTransaction { get; set; }

        public ServiceEntryGovernance GovernanceOptions { get; set; }

        public T GetMetadata<T>(string name, T def = default(T))
        {
            if (!Metadatas.ContainsKey(name))
                return def;

            return (T)Metadatas[name];
        }

        #region Equality Members

        public override bool Equals(object obj)
        {
            var model = obj as ServiceEntryDescriptor;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.Id != Id)
                return false;
            return model.Metadatas.Count == Metadatas.Count && model.Metadatas.All(metadata =>
            {
                object value;
                if (!Metadatas.TryGetValue(metadata.Key, out value))
                    return false;

                if (metadata.Value == null && value == null)
                    return true;
                if (metadata.Value == null || value == null)
                    return false;

                return metadata.Value.Equals(value);
            });
        }

        public static bool operator ==(ServiceEntryDescriptor model1, ServiceEntryDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceEntryDescriptor model1, ServiceEntryDescriptor model2)
        {
            return !Equals(model1, model2);
        }

        public override string ToString()
        {
            return Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}