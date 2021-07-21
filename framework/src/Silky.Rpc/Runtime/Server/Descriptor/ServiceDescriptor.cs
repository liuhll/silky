using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Silky.Rpc.Runtime.Server.Descriptor
{
    public class ServiceDescriptor
    {
        public ServiceDescriptor()
        {
            Metadatas = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        [NotNull] public string Id { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }

        public IDictionary<string, object> Metadatas { get; set; }

        public T GetMetadata<T>(string name, T def = default(T))
        {
            if (!Metadatas.ContainsKey(name))
                return def;

            return (T) Metadatas[name];
        }

        #region Equality Members

        public override bool Equals(object obj)
        {
            var model = obj as ServiceDescriptor;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.Id != Id)
                return false;
            if (model.ServiceProtocol != ServiceProtocol)
            {
                return false;
            }

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

        public static bool operator ==(ServiceDescriptor model1, ServiceDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceDescriptor model1, ServiceDescriptor model2)
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