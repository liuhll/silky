using System;
using Newtonsoft.Json;
using ProtoBuf;

namespace Silky.Codec.Message
{
    [ProtoContract]
    public class DynamicItem
    {
        #region Constructor

        public DynamicItem()
        {
        }

        public DynamicItem(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var valueType = value.GetType();
            var code = Type.GetTypeCode(valueType);

            if (code != TypeCode.Object)
                TypeName = valueType.FullName;
            else
                TypeName = valueType.AssemblyQualifiedName;
            if (valueType == UtilityType.JObjectType || valueType == UtilityType.JArrayType)
                Content = SerializerUtilitys.Serialize(value.ToString());
            else
                Content = SerializerUtilitys.Serialize(JsonConvert.SerializeObject(value));
        }

        #endregion Constructor

        #region Property

        [ProtoMember(1)] public string TypeName { get; set; }
        [ProtoMember(2)] public byte[] Content { get; set; }

        #endregion Property

        #region Public Method

        public object Get()
        {
            if (Content == null || TypeName == null)
                return null;
            var typeName = Type.GetType(TypeName);
            var contenString = SerializerUtilitys.Deserialize<string>(Content);
            return JsonConvert.DeserializeObject(contenString, typeName);
        }

        #endregion Public Method
    }
}