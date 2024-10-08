using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public static class CustomConverters
{
    public class BehaviourConvertor : JsonConverter<Behaviour>
    {
        public override Behaviour ReadJson(JsonReader reader, Type objectType, Behaviour existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            var type = jsonObject.GetValue("Type").ToString();
            var value = jsonObject.GetValue("Value").CreateReader();
            var obj = serializer.Deserialize(value, Type.GetType(type)) as Behaviour;

            return obj;
        }

        public override void WriteJson(JsonWriter writer, Behaviour value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Type");
            writer.WriteValue(value.GetType().Name);

            writer.WritePropertyName("Value");
            serializer.Serialize(writer, value);

            writer.WriteEndObject();
        }
    }
    public class EntityConvertor : JsonConverter<Entity>
    {
        public override Entity ReadJson(JsonReader reader, Type objectType, Entity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            var type = jsonObject.GetValue("Type").ToString();
            var value = jsonObject.GetValue("Value").CreateReader();
            var obj = serializer.Deserialize(value, Type.GetType(type)) as Entity;

            return obj;
        }

        public override void WriteJson(JsonWriter writer, Entity value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Type");
            writer.WriteValue(value.GetType().Name);

            writer.WritePropertyName("Value");
            serializer.Serialize(writer, value);

            writer.WriteEndObject();
        }
    }

    public static string Serialize<T>(T obj)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };
        return JsonConvert.SerializeObject(obj, settings);
    }

    public static T Deserialize<T>(string data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };
        return JsonConvert.DeserializeObject<T>(data, settings);
    }

}



