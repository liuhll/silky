using System;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Silky.Core.Serialization;

public class JObjectConverter : JsonConverter
{
    public override bool CanRead => false;

    public override bool CanConvert(Type objectType) => objectType == typeof(JObject);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        => throw new NotImplementedException();

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        => JToken.FromObject((value as JObject).ToObject<ExpandoObject>(), serializer).WriteTo(writer);
}