using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceDescriptor
    {
        public ServiceDescriptor()
        {
            Metadatas = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            ServiceEntries = new List<ServiceEntryDescriptor>();
        }

        [NotNull] public string Id { get; set; }

        [NotNull] public string ServiceName { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }

        public ICollection<ServiceEntryDescriptor> ServiceEntries { get; set; }

        public IDictionary<string, object> Metadatas { get; set; }

        public T GetMetadata<T>(string name, T def = default(T))
        {
            if (!Metadatas.ContainsKey(name))
                return def;

            if (Metadatas[name] == null)
            {
                return def;
            }

            if (Metadatas[name] is T)
            {
                return (T)Metadatas[name];
            }

            return Metadatas[name].ConventTo<T>();
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
            {
                return false;
            }

            if (model.ServiceProtocol != ServiceProtocol)
            {
                return false;
            }

            var isEqualsServiceEntriesCount = model.ServiceEntries.Count == ServiceEntries.Count;

            var isEqualsServiceEntries = model.ServiceEntries.All(se =>
            {
                var thisServiceDesc = ServiceEntries.FirstOrDefault(p => p == se);
                return thisServiceDesc != null;
            });

            var isEqualsMetaDataCount = model.Metadatas.Count == Metadatas.Count;
            var isEqualsMetaData = model.Metadatas.All(metadata =>
            {
                object value;
                if (!Metadatas.TryGetValue(metadata.Key, out value))
                    return false;

                if (metadata.Value == null && value == null)
                    return true;
                if (metadata.Value == null || value == null)
                    return false;

                var isEqualsMetadataVal = false;
                if (metadata.Value.GetType() != typeof(string))
                {
                    var serializer = EngineContext.Current.Resolve<ISerializer>();
                    isEqualsMetadataVal = serializer.Serialize(metadata.Value).Equals(serializer.Serialize(value));
                }
                else
                {
                    isEqualsMetadataVal = metadata.Value.Equals(value);
                }


                return isEqualsMetadataVal;
            });


            var isEquals = isEqualsServiceEntriesCount
                           && isEqualsServiceEntries
                           && isEqualsMetaDataCount
                           && isEqualsMetaData;
            return isEquals;
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
            return (Id
                    + string.Join("", Metadatas.OrderBy(p => p.Key).Select(p => p.Key + p.Value))
                    + string.Join("", ServiceEntries.OrderBy(p => p.Id).Select(p => p.Id))).GetHashCode();
        }

        #endregion
    }
}