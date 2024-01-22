using System;
using System.Collections.Generic;
using Core.Extensions;
using Newtonsoft.Json;

namespace DataAccess.Utils.JsonParse
{
    public class StringToDicStringFloat : JsonConverter<Dictionary<string, float>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<string, float> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, float> ReadJson(JsonReader reader, Type objectType,
            Dictionary<string, float> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var readerValue = reader.Value as string;

            if (readerValue.IsNullOrEmpty())
            {
                return null;
            }
            // readerValue = readerValue.Trim('"');
            return JsonConvert.DeserializeObject<Dictionary<string, float>>(readerValue);
        }
    }
}